using Stream = Android.Media.Stream;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Media;
using Android.Net;
using Android.OS;
using Android.Runtime;
using Android.Text;
using Android.Text.Util;
using Android.Views;
using Android.Widget;
using BarCode2D_Receiver;
using Java.Lang;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.SqlServer.Server;
using Newtonsoft.Json;
using WMS.App;
using AlertDialog = Android.App.AlertDialog;

using TrendNET.WMS.Core.Data;
using TrendNET.WMS.Device.App;
using TrendNET.WMS.Device.Services;
using static BluetoothService;
using static EventBluetooth;
using Exception = System.Exception;

using AndroidX.AppCompat.App;

namespace WMS
{


    [Activity(Label = "IssuedGoodsIdentEntryWithTrail")]
    public class IssuedGoodsIdentEntryWithTrail : AppCompatActivity, IBarcodeResult
    {

        private NameValueObject moveHead = (NameValueObject)InUseObjects.Get("MoveHead");
        private NameValueObject openOrder = (NameValueObject)InUseObjects.Get("OpenOrder");
        private NameValueObject trailFilters = (NameValueObject)InUseObjects.Get("TrailFilters");
        private EditText tbOrder;
        private EditText tbReceiver;
        private EditText tbIdentFilter;
        private EditText tbLocationFilter;
        private ListView ivTrail;
        private List<Trail> ChosenOnes = new List<Trail>();
        private Button btConfirm;
        private Button btBack;
        private Button btDisplayPositions;
        private Button btLogout;
        SoundPool soundPool;
        int soundPoolId;
        private List<Trail> trails;
        private adapter adapterObj;
        public int selected;
        private string password;
        private Trail chosen;
        private IEnumerable<NameValueObject> openOrderLocal;
        private NameValueObject openIdent;
        public MyBinder binder;
        public bool isBound = false;
        public MyServiceConnection serviceConnection;
        private BluetoothService activityBluetoothService;
        private EventBluetooth send;
        private MyOnItemLongClickListener listener;
        private ApiResultSet result;
        private NameValueObjectList NameValueObjectVariableList;

        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            switch (keyCode)
            {
                // in smartphone
                case Keycode.F3:
                    if (btConfirm.Enabled == true)
                    {
                        BtConfirm_Click(this, null);
                    }
                    break;
                // return true;
                case Keycode.F4:
                    if (btDisplayPositions.Enabled == true)
                    {
                        BtDisplayPositions_Click(this, null);
                    }
                    break;
                case Keycode.F8:
                    if (btLogout.Enabled == true)
                    {
                        BtLogout_Click(this, null);
                    }
                    break;

            }
            return base.OnKeyDown(keyCode, e);
        }

        public void GetBarcode(string barcode)
        {
            try
            {
                if (barcode != "Scan fail" && barcode != "")
                {
                    if (tbIdentFilter.HasFocus)
                    {
                        Sound();
                        if (HelperMethods.is2D(barcode))
                        {
                            Parser2DCode parser2DCode = new Parser2DCode(barcode.Trim());
                            chooseIdent(parser2DCode.ident, parser2DCode.charge, parser2DCode.netoWeight);
                        }
                        else if (!CheckIdent(barcode) && barcode.Length > 17 && barcode.Contains("400"))
                        {
                            var ident = barcode.Substring(0, barcode.Length - 16);
                            tbIdentFilter.Text = ident;
                            adapterObj.Filter(trails, true, tbIdentFilter.Text, false);
                            if (adapterObj.returnNumberOfItems() == 0)
                            {
                                tbIdentFilter.Text = string.Empty;
                            }
                            chooseIdentOnly(ident);
                        }
                        else
                        {
                            tbIdentFilter.Text = barcode;
                            adapterObj.Filter(trails, true, tbIdentFilter.Text, false);
                            if (adapterObj.returnNumberOfItems() == 0)
                            {
                                tbIdentFilter.Text = string.Empty;
                            }

                        }
                    }
                    else if (tbLocationFilter.HasFocus)
                    {
                        Sound();
                        tbLocationFilter.Text = barcode;
                        adapterObj.Filter(trails, false, tbLocationFilter.Text, false);
                        if (adapterObj.returnNumberOfItems() == 0)
                        {
                            tbIdentFilter.Text = string.Empty;
                        }
                    }
                }
                listener.updateData(adapterObj.returnData());
            } catch (Exception err)
            {
                Crashes.TrackError(err);
                return;
            }
        }




        private bool CheckIdent(string barcode)
        {
            if (string.IsNullOrEmpty(barcode)) { return false; }
            try
            {
                string error;
                openIdent = Services.GetObject("id", barcode, out error);
                if (openIdent != null)
                {
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }

            return false;

        }


        private void chooseIdent(string ident, string serial, string qty)
        {

            var convertedIdent = string.Empty;
            string error;
            openIdent = Services.GetObject("id", ident, out error);

            if (openIdent != null)
            {
                convertedIdent = openIdent.GetString("Code");
                ident = convertedIdent;

            } else
            {
                return;
            }

            adapterObj.Filter(trails, true, ident, false);
            int numberOfHits = adapterObj.returnNumberOfItems();
            if (numberOfHits == 0)
            {
                adapterObj.Filter(trails, true, string.Empty, false);
                return;
            } else if (numberOfHits == 1)
            {
                Trail trail = adapterObj.returnData().ElementAt(0);


                if (trail.Location == string.Empty)
                {
                    Toast.MakeText(this, "Ni zaloge.", ToastLength.Long).Show();
                    return;
                }
                InUseObjects.Set("OpenOrder", trails.ElementAt(adapterObj.returnSelected().originalIndex));

                if (SaveMoveHeadObjectMode(trail))
                {
                    if (trails.Count - 1 == 1)
                    {
                        var lastItem = new NameValueObject("LastItem");
                        lastItem.SetBool("IsLastItem", true);
                        InUseObjects.Set("LastItem", lastItem);
                    }


                    Intent i = new Intent(Application.Context, typeof(IssuedGoodsSerialOrSSCCEntry));
                    i.PutExtra("ident", ident);
                    i.PutExtra("qty", qty);
                    i.PutExtra("serial", serial);
                    i.PutExtra("scan", true);
                    i.PutExtra("selected", Trail.Serialize(trail));
                    StartActivity(i);
                    this.Finish();

                }

            } else if (numberOfHits > 1)
            {
                return;
            }


            listener.updateData(adapterObj.returnData());

        }

        private void chooseIdentOnly(string charge)
        {
            int numberOfHits = adapterObj.returnNumberOfItems();
            if (numberOfHits == 0)
            {
                adapterObj.Filter(trails, true, string.Empty, false);
                return;
            }
            else if (numberOfHits == 1)
            {
                Trail trail = adapterObj.returnData().ElementAt(0);
                if (trail.Location == string.Empty)
                {
                    Toast.MakeText(this, "Ni zaloge.", ToastLength.Long).Show();
                    return;
                }
                InUseObjects.Set("OpenOrder", trails.ElementAt(adapterObj.returnSelected().originalIndex));

                if (SaveMoveHeadObjectMode(trail))
                {
                    if (trails.Count - 1 == 1)
                    {
                        var lastItem = new NameValueObject("LastItem");
                        lastItem.SetBool("IsLastItem", true);
                        InUseObjects.Set("LastItem", lastItem);
                    }
                    Intent i = new Intent(Application.Context, typeof(IssuedGoodsSerialOrSSCCEntry));
                    i.PutExtra("ident", trail.Ident);
                    i.PutExtra("qty", trail.Qty);
                    i.PutExtra("selected", Trail.Serialize(trail));
                    i.PutExtra("scan", true);
                    StartActivity(i);
                    this.Finish();
                }
            }
            else if (numberOfHits > 1)
            {
                return;
            }
            listener.updateData(adapterObj.returnData());
        }



      

        private async Task FillDisplayedOrderInfo()
        {
            await Task.Run(async () =>
            {

                try
                {
                    List<Trail> unfiltered = new List<Trail>();
                    var filterLoc = tbLocationFilter.Text;
                    var filterIdent = tbIdentFilter.Text;
                    try
                    {
                        if (openOrder != null)
                        {
                            RunOnUiThread(() =>
                            {
                                tbOrder.Text = openOrder.GetString("Key");
                                tbReceiver.Text = openOrder.GetString("Receiver");
                            });

                            password = openOrder.GetString("Key");

                        } else if (moveHead != null)
                        {
                            RunOnUiThread(() =>
                            {
                                tbOrder.Text = moveHead.GetString("LinkKey");
                                tbReceiver.Text = moveHead.GetString("Receiver");
                            });
                            password = moveHead.GetString("LinkKey");
                        }
                        string error;
                        var warehouse = moveHead.GetString("Wharehouse");
                        // qtyByLoc = Services.GetObjectList("ook", out error, password);
                        string sql = $"SELECT * FROM uWMSOrderItemByKeyOut WHERE acKey = '{password}';";
                        result = Services.GetObjectListBySql(sql);
                        
                        
                        
                        
                        NameValueObjectVariableList = result.ConvertToNameValueObjectList("OpenOrder");

                        if (result.Success && result.Rows.Count > 0)
                        {
                            trails.Clear();
                            int counter = 0;
                            foreach(var row in result.Rows) {
                                var ident = row.StringValue("acIdent");
                                var location = row.StringValue("aclocation");
                                var name = row.StringValue("acName");

                                if ((string.IsNullOrEmpty(filterLoc) || (location == filterLoc)) &&
                                   (string.IsNullOrEmpty(filterIdent) || (ident == filterIdent)))
                                {

                                    var key = row.StringValue("acKey");
                                    var lvi = new Trail();
                                    lvi.Key = key;
                                    lvi.Ident = ident;
                                    lvi.Location = location;
                                    lvi.Qty = string.Format("{0:###,##0.00}", row.DoubleValue("anQty"));
                                    lvi.originalIndex = counter;
                                    lvi.No = (int)row.IntValue("anNo");
                                    lvi.Name = name;
                                    counter ++;
                                    unfiltered.Add(lvi);
       
                                }                                                            
                            }
                        }


                        RunOnUiThread(() =>
                        {
                            trails = unfiltered;
                            adapterObj.NotifyDataSetChanged();
                            LoaderManifest.LoaderManifestLoopStop(this);
                            adapterObj.Filter(trails, true, string.Empty, false);
                            listener = new MyOnItemLongClickListener(this, adapterObj.returnData(), adapterObj);
                            ivTrail.OnItemLongClickListener = listener;


                            // Bluetooth
                           /* try
                            {
                                sendDataToDevice();
                            } catch (Exception ex)
                            {
                                Crashes.TrackError(ex);
                            }
                            // Bluetooth
                           */

                        });
                    }
                    catch(Exception error) {
                        var e = error;
                        Crashes.TrackError(e);
                    }               
                }
                catch (Exception error)
                {
                    Crashes.TrackError(error);
                }
            });
        }

       
        protected async override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetTheme(Resource.Style.AppTheme_NoActionBar);
            // Create your application here
            SetContentView(Resource.Layout.IssuedGoodsIdentEntryWithTrail);
            AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
            var _customToolbar = new CustomToolbar(this, toolbar, Resource.Id.navIcon);
            _customToolbar.SetNavigationIcon(settings.RootURL + "/Services/Logo");
            SetSupportActionBar(_customToolbar._toolbar);
            SupportActionBar.SetDisplayShowTitleEnabled(false);
            tbOrder = FindViewById<EditText>(Resource.Id.tbOrder);
            tbReceiver = FindViewById<EditText>(Resource.Id.tbReceiver);
            tbIdentFilter = FindViewById<EditText>(Resource.Id.tbIdentFilter);
            tbLocationFilter = FindViewById<EditText>(Resource.Id.tbLocationFilter);
            ivTrail = FindViewById<ListView>(Resource.Id.ivTrail);
            btConfirm = FindViewById<Button>(Resource.Id.btConfirm);
            btDisplayPositions = FindViewById<Button>(Resource.Id.btDisplayPositions);
            btBack = FindViewById<Button>(Resource.Id.btBack);
            btBack.Click += BtBack_Click;
            btLogout = FindViewById<Button>(Resource.Id.btLogout);
            soundPool = new SoundPool(10, Stream.Music, 0);
            soundPoolId = soundPool.Load(this, Resource.Raw.beep, 1);
            Barcode2D barcode2D = new Barcode2D();
            barcode2D.open(this, this);
            color();
            tbLocationFilter.FocusChange += TbLocationFilter_FocusChange;
            trails = new List<Trail>();
            adapterObj = new adapter(this, trails);
            ivTrail.Adapter = adapterObj;
            ivTrail.ItemClick += IvTrail_ItemClick;
            btConfirm.Click += BtConfirm_Click;
            btDisplayPositions.Click += BtDisplayPositions_Click;
            btLogout.Click += BtLogout_Click;

            if (openOrder == null && moveHead == null)
            {
                //  openOrder = Services.GetObject("oobl", moveItem.GetString("LinkKey") + "|" + moveItem.GetInt("LinkNo").ToString(), out error);
                AlertDialog.Builder alert = new AlertDialog.Builder(this);
                alert.SetTitle("Napaka");
                alert.SetMessage("Prišlo je do napake in aplikacija se bo zaprla");
                alert.SetPositiveButton("Ok", (senderAlert, args) =>
                {
                    alert.Dispose();
                    System.Threading.Thread.Sleep(500);
                    throw new ApplicationException("Error, openIdent");
                });
                Dialog dialog = alert.Create();
                dialog.Show();
            }



            if (trailFilters != null)
            {
                tbIdentFilter.Text = trailFilters.GetString("Ident");
                tbLocationFilter.Text = trailFilters.GetString("Location");
            }

            FillDisplayedOrderInfo();

            var _broadcastReceiver = new NetworkStatusBroadcastReceiver();
            _broadcastReceiver.ConnectionStatusChanged += OnNetworkStatusChanged;
            Application.Context.RegisterReceiver(_broadcastReceiver,
            new IntentFilter(ConnectivityManager.ConnectivityAction));



            tbIdentFilter.AfterTextChanged += TbIdentFilter_AfterTextChanged;
            tbLocationFilter.AfterTextChanged += TbLocationFilter_AfterTextChanged;

            // Parameter
            if (true)
            {
                // Binding to a service
                serviceConnection = new MyServiceConnection(this);
                Intent serviceIntent = new Intent(this, typeof(BluetoothService));
                BindService(serviceIntent, serviceConnection, Bind.AutoCreate);
            }


            tbIdentFilter.RequestFocus();

       
        }




        public class MyOnItemLongClickListener : Java.Lang.Object, AdapterView.IOnItemLongClickListener
        {
            Context context_;
            List<Trail> data_;
            adapter adapter_;


            public void updateData(List<Trail> data)
            {
                data_ = data;
            }

            public MyOnItemLongClickListener(Context context, List<Trail> data, adapter adapter) {
                context_ = context;
                data_ = data;
                adapter_ = adapter;
            }

            public bool OnItemLongClick(AdapterView parent, View view, int position, long id)
            {
                adapter_.setSelected(position);
                Trail selected = data_.ElementAt(position);
                AlertDialog.Builder builder = new AlertDialog.Builder(context_);
                builder.SetTitle("Podrobnosti");
                builder.SetMessage($"Ident: {selected.Ident}\nNaziv: {selected.Name}\nKljuč: {selected.Key}");
                builder.SetPositiveButton("OK", (s, args) =>
                {
                });
                AlertDialog alertDialog = builder.Create();
                alertDialog.Show();
                return true; // Return true to consume the long click event
            }
        }


        private void IvTrail_LongClick(object sender, View.LongClickEventArgs e)
        {
            Toast.MakeText(this, "Long", ToastLength.Long).Show();
        }

        private void TbLocationFilter_AfterTextChanged(object sender, AfterTextChangedEventArgs e)
        {
            try
            {
                adapterObj.Filter(trails, false, tbLocationFilter.Text, false);
                listener.updateData(adapterObj.returnData());
            } catch
            {
                return;
            }
        }

        private void TbIdentFilter_AfterTextChanged(object sender, AfterTextChangedEventArgs e)
        {
            try
            {
                adapterObj.Filter(trails, true, tbIdentFilter.Text, false);
                listener.updateData(adapterObj.returnData());
            } catch
            {
                return;
            }
        }
        public void OnServiceBindingComplete(BluetoothService service)
        {
            try
            {
                activityBluetoothService = service;
            } catch
            {
                return;
            }
        }

     

        private void sendDataToDevice()
        {
            if (activityBluetoothService != null)
            {
                send = new EventBluetooth();
                List<Position> positions = new List<Position>();    
                foreach (Trail trail in trails)
                {
                    positions.Add(new Position { Ident = trail.Ident, Key = trail.Key, Location = trail.Location, Name = trail.Name, Qty = trail.Qty });
                }
                send.positions = positions;
                send.eventType = EventBluetooth.EventType.IssuedList;
                send.isRefreshCallback = true;
                send.chosenPosition = null;
                send.orderNumber = tbOrder.Text;
                activityBluetoothService.sendObject(JsonConvert.SerializeObject(send));
            } else
            {
                return;
            }
        }
       
        private void BtBack_Click(object sender, EventArgs e)
        {
            OnBackPressed();
        }

        public bool IsOnline()
        {
            var cm = (ConnectivityManager)GetSystemService(ConnectivityService);
            return cm.ActiveNetworkInfo == null ? false : cm.ActiveNetworkInfo.IsConnected;
        }

        private void OnNetworkStatusChanged(object sender, EventArgs e)
        {
            if (IsOnline())
            {               
                try
                {
                    LoaderManifest.LoaderManifestLoopStop(this);
                }
                catch (Exception err)
                {
                    Crashes.TrackError(err);
                }
            }
            else
            {
                LoaderManifest.LoaderManifestLoop(this);
            }
        }

        private void TbLocationFilter_FocusChange(object sender, View.FocusChangeEventArgs e)
        {
           
        }

        private void color()
        {
            tbIdentFilter.SetBackgroundColor(Android.Graphics.Color.Aqua);
            tbLocationFilter.SetBackgroundColor(Android.Graphics.Color.Aqua);
        }

        private void BtLogout_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(MainMenu));
            HelpfulMethods.clearTheStack(this);
        }

        private void BtDisplayPositions_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(IssuedGoodsEnteredPositionsView));
            HelpfulMethods.clearTheStack(this);
        }

   



        private async void BtConfirm_Click(object sender, EventArgs e)
        {
            try
            {
                if (adapterObj.returnSelected() != null)
                {
                    if (string.IsNullOrEmpty(adapterObj.returnSelected().Location))
                    {
                        RunOnUiThread(() =>
                        {
                            Toast.MakeText(this, "Ta izdelek ni mogoče izdati ker nima zalogo.", ToastLength.Long).Show();
                        });
                        return;
                    }
                    else
                    {
                        string error;
                        var toSave = Services.GetObject("oobl", adapterObj.returnSelected().Key + "|" + adapterObj.returnSelected().No, out error);
                        openOrder = toSave;
                        InUseObjects.Set("OpenOrder", toSave);
                        var openIdent = Services.GetObject("id", openOrder.GetString("Ident"), out error);
                        InUseObjects.Set("OpenIdent", openIdent);
                    }

                }
                else
                {
                    RunOnUiThread(() =>
                    {
                        Toast.MakeText(this, "Izdelek ni izbran?.", ToastLength.Long);
                    });

                    return;
                }


                if (SaveMoveHead())
                {
                    if (trails.Count() - 1 == 0)
                    {
                        var lastItem = new NameValueObject("LastItem");
                        lastItem.SetBool("IsLastItem", true);
                        InUseObjects.Set("LastItem", lastItem);
                        var obj = adapterObj.returnSelected();
                        var ident = obj.Ident;
                        var qty = obj.Qty;
                        Intent i = new Intent(Application.Context, typeof(IssuedGoodsSerialOrSSCCEntry));
                        i.PutExtra("ident", ident);
                        i.PutExtra("qty", qty);
                        i.PutExtra("selected", Trail.Serialize(obj));
                        StartActivity(i);
                        this.Finish();

                    }
                    else
                    {
                        var obj = adapterObj.returnSelected();
                        var ident = obj.Ident;
                        var qty = obj.Qty;
                        Intent i = new Intent(Application.Context, typeof(IssuedGoodsSerialOrSSCCEntry));
                        i.PutExtra("ident", ident);
                        i.PutExtra("qty", qty);
                        i.PutExtra("selected", Trail.Serialize(obj));
                        StartActivity(i);
                        this.Finish();
                    }
                }
            } catch(Exception err)
            {
                Crashes.TrackError(err);
                StartActivity(typeof(MainMenu));
            }
        }

  

        private void IvTrail_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            adapterObj.setSelected(e.Position);
            chosen = adapterObj.returnSelected();
        }

        private bool SaveMoveHeadObjectMode(Trail trail)
        {   if(trail == null) { return false; }
            var obj = trail;
            var ident = obj.Ident;
            var location = obj.Location;
            var qty = Convert.ToDouble(obj.Qty);
            var extraData = new NameValueObject("ExtraData");
            extraData.SetString("Location", location);
            extraData.SetDouble("Qty", qty);
            InUseObjects.Set("ExtraData", extraData);
            string error;
            try
            {
                var openIdent = Services.GetObject("id", ident, out error);
                if (openIdent == null)
                {
                    string WebError = string.Format("Napaka pri preverjanju identa." + error);
                    Toast.MakeText(this, WebError, ToastLength.Long).Show();
                    return false;
                }
                InUseObjects.Set("OpenIdent", openIdent);
            } catch (Exception err)
            {
                Crashes.TrackError(err);
            }
            if (!moveHead.GetBool("Saved"))
            {

                try
                {
                    var test = openOrder.GetString("No");
                    moveHead.SetInt("Clerk", Services.UserID());
                    moveHead.SetString("Type", "P");
                    moveHead.SetString("LinkKey", openOrder.GetString("Key"));
                    moveHead.SetString("LinkNo", openOrder.GetString("No"));
                    moveHead.SetString("Document1", openOrder.GetString("Document1"));
                    moveHead.SetDateTime("Document1Date", openOrder.GetDateTime("Document1Date"));
                    moveHead.SetString("Note", openOrder.GetString("Note"));
                    string testDocument1 = openOrder.GetString("Document1");
                    if (moveHead.GetBool("ByOrder"))
                    {
                        moveHead.SetString("Receiver", openOrder.GetString("Receiver"));
                    }
                    var savedMoveHead = Services.SetObject("mh", moveHead, out error);
                    if (savedMoveHead == null)
                    {
                        string WebError = string.Format("Napaka pri dostopu do web aplikacije." + error);
                        Toast.MakeText(this, WebError, ToastLength.Long).Show();
                        return false;
                    }
                    else
                    {
                        moveHead.SetInt("HeadID", savedMoveHead.GetInt("HeadID"));
                        moveHead.SetBool("Saved", true);
                        return true;
                    }
                }
                catch (Exception err)
                {

                    Crashes.TrackError(err);
                    return false;

                }
            }
            else
            {
                return true;
            }
        }

        /* Save move head method. */
        private bool SaveMoveHead()
        {

            var obj = adapterObj.returnSelected();
            var ident = obj.Ident;
            var location = obj.Location;
            var qty = Convert.ToDouble(obj.Qty);
            var extraData = new NameValueObject("ExtraData");
            extraData.SetString("Location", location);
            extraData.SetDouble("Qty", qty);
            InUseObjects.Set("ExtraData", extraData);
            string error;
      
            if (!moveHead.GetBool("Saved"))
            {
                
                try
                {
                    var test = openOrder.GetString("No");
                    moveHead.SetInt("Clerk", Services.UserID());
                    moveHead.SetString("Type", "P");
                    moveHead.SetString("LinkKey", openOrder.GetString("Key"));
                    moveHead.SetString("LinkNo", openOrder.GetString("No"));
                    moveHead.SetString("Document1", openOrder.GetString("Document1"));
                    moveHead.SetDateTime("Document1Date", openOrder.GetDateTime("Document1Date"));
                    moveHead.SetString("Note", openOrder.GetString("Note"));
                    string testDocument1 = openOrder.GetString("Document1");
                    if (moveHead.GetBool("ByOrder"))
                    {
                        moveHead.SetString("Receiver", openOrder.GetString("Receiver"));
                    }
                    var savedMoveHead = Services.SetObject("mh", moveHead, out error);
                    if (savedMoveHead == null)
                    {
                        string WebError = string.Format("Napaka pri dostopu do web aplikacije." + error);
                        Toast.MakeText(this, WebError, ToastLength.Long).Show();
                        return false;
                    }
                    else
                    {
                        moveHead.SetInt("HeadID", savedMoveHead.GetInt("HeadID"));
                        moveHead.SetBool("Saved", true);
                        return true;
                    }
                }
                catch (Exception err)
                {
                    Crashes.TrackError(err);
                    return false;
                }
            }
            else
            {
                return true;
            }
        }
    

        private void Sound()
        {
            soundPool.Play(soundPoolId, 1, 1, 0, 0, 1);
        }

      

    }
}