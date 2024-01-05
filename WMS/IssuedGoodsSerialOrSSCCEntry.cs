using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.Media;
using Android.Net;
using Android.Nfc;
using Android.OS;
using Android.Runtime;
using Android.Text.Util;
using Android.Views;
using Android.Widget;
using BarCode2D_Receiver;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Newtonsoft.Json;
using Scanner.App;
using TrendNET.WMS.Core.Data;
using TrendNET.WMS.Device.App;
using TrendNET.WMS.Device.Services;
using static Android.App.ActionBar;
using static Android.Graphics.Paint;
using static Android.Icu.Text.Transliterator;
using WebApp = TrendNET.WMS.Device.Services.WebApp;

namespace Scanner
{
    [Activity(Label = "IssuedGoodsSerialOrSSCCEntry", ScreenOrientation = ScreenOrientation.Portrait)]
    public class IssuedGoodsSerialOrSSCCEntry : Activity, IBarcodeResult
    {
      
        private EditText tbIdent;
        private EditText tbSSCC;
        private EditText tbSerialNum;
        private EditText tbLocation;
        private EditText tbPacking;
        private EditText tbUnits;
        private EditText tbPalette;
        private Button button1;
        private Button btSaveOrUpdate;
        private Button button4;
        private Button button6;
        private Button button5;
        private Button button7;
        private NameValueObject openIdent = (NameValueObject)InUseObjects.Get("OpenIdent");
        private NameValueObject openOrder = (NameValueObject)InUseObjects.Get("OpenOrder");
        private ApiResultSet OpenOrderItem = (ApiResultSet)InUseObjects.Get("OpenOrderItem");
        private bool isSkipable = false;
        private NameValueObject moveHead = (NameValueObject)InUseObjects.Get("MoveHead");
        private NameValueObject moveItem = (NameValueObject)InUseObjects.Get("MoveItem");
        private NameValueObject CurrentFlow = (NameValueObject)InUseObjects.Get("CurrentClientFlow");
        private NameValueObject extraData = (NameValueObject)InUseObjects.Get("ExtraData");
        private NameValueObject lastItem = (NameValueObject)InUseObjects.Get("LastItem");     
        private List<MorePallets> data = new List<MorePallets>();
        private bool enabledSerial;
        private NameValueObjectList docTypes = null;
        private NameValueObject stock = null;
        private TextView lbQty;
        private bool editMode = false;
        private bool isPackaging = false;
        private TextView lbUnits;
        private TextView lbPalette; 
        SoundPool soundPool;
        int soundPoolId;
        private Button btMorePallets;
        private bool isOpened = false;
        private Trail receivedTrail;
        private List<string> locations = new List<string>();
        double qtyCheck = 0;

        protected async override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            LoaderManifest.LoaderManifestLoopResources(this);
            // Create your application here
            SetContentView(Resource.Layout.IssuedGoodsSerialOrSSCCEntry);
            btMorePallets = FindViewById<Button>(Resource.Id.btMorePallets);
            btMorePallets.Click += BtMorePallets_Click;
            btSaveOrUpdate = FindViewById<Button>(Resource.Id.btSaveOrUpdate);
            AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
            var _customToolbar = new CustomToolbar(this, toolbar, Resource.Id.navIcon);
            _customToolbar.SetNavigationIcon(settings.RootURL + "/Services/Logo");
            if (CurrentFlow != null)
            {
                if (String.IsNullOrEmpty(CurrentFlow.GetString("CurrentFlow")))
                {
                    AlertDialog.Builder alert = new AlertDialog.Builder(this);
                    alert.SetTitle("Napaka na aplikaciji");
                    alert.SetMessage("Prišlo je do napake.");
                    alert.SetPositiveButton("Ok", (senderAlert, args) =>
                    {
                        StartActivity(typeof(MainMenu));
                    });
                    Dialog dialog = alert.Create();
                    dialog.Show();
                }

                var oop = openOrder;
                var oii = openIdent;
                var mhh = moveHead;

            } else
            {
                AlertDialog.Builder alert = new AlertDialog.Builder(this);
                alert.SetTitle("Napaka na aplikaciji");
                alert.SetMessage("Prišlo je do napake.");
                alert.SetPositiveButton("Ok", (senderAlert, args) =>
                {
                    StartActivity(typeof(MainMenu));
                });
                Dialog dialog = alert.Create();
                dialog.Show();
            }

            tbIdent = FindViewById<EditText>(Resource.Id.tbIdent);
            tbSSCC = FindViewById<EditText>(Resource.Id.tbSSCC);
            tbSerialNum = FindViewById<EditText>(Resource.Id.tbSerialNum);
            tbLocation = FindViewById<EditText>(Resource.Id.tbLocation);
            tbPacking = FindViewById<EditText>(Resource.Id.tbPacking);
            tbUnits = FindViewById<EditText>(Resource.Id.tbUnits);
            tbPalette = FindViewById<EditText>(Resource.Id.tbPalette);
            tbIdent.InputType = Android.Text.InputTypes.ClassNumber;
            tbSSCC.InputType = Android.Text.InputTypes.ClassNumber;
            tbLocation.InputType = Android.Text.InputTypes.ClassText;
            tbUnits.InputType = Android.Text.InputTypes.ClassNumber;
            tbPalette.InputType = Android.Text.InputTypes.ClassNumber;
            button1 = FindViewById<Button>(Resource.Id.button1);
            button4 = FindViewById<Button>(Resource.Id.button4);
            button6 = FindViewById<Button>(Resource.Id.button6);
            button5 = FindViewById<Button>(Resource.Id.button5);
            button7 = FindViewById<Button>(Resource.Id.button7);
            lbQty = FindViewById<TextView>(Resource.Id.lbQty);
            lbUnits = FindViewById<TextView>(Resource.Id.lbUnits);
            lbPalette = FindViewById<TextView>(Resource.Id.lbPalette);
            await GetLocationsForGivenWarehouse(moveHead.GetString("Wharehouse"));
            adapterReceive = new CustomAutoCompleteAdapter<String>(this, Android.Resource.Layout.SimpleSpinnerItem, locations);
            adapterReceive.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);         
            soundPool = new SoundPool(10, Stream.Music, 0);
            soundPoolId = soundPool.Load(this, Resource.Drawable.beep, 1);
            Barcode2D barcode2D = new Barcode2D();
            barcode2D.open(this, this);
            button1.Click += Button1_Click;
            btSaveOrUpdate.Click += BtSaveOrUpdate_Click;
            button4.Click += Button4_Click;
            button6.Click += Button6_Click;
            tbSSCC.KeyPress += TbSSCC_KeyPress;
            button7.Click += Button7_Click;
            btMorePallets = FindViewById<Button>(Resource.Id.btMorePallets);
            button5.Click += Button5_Click;
            colorFields();
            tbPacking.FocusChange += TbPacking_FocusChange;  
            if (moveHead == null) {
                AlertDialog.Builder alert = new AlertDialog.Builder(this);
                alert.SetTitle("Napaka na aplikaciji");
                alert.SetMessage("Prišlo je do napake in aplikacija se bo zaprla.");
                alert.SetPositiveButton("Ok", (senderAlert, args) =>
                {
                    alert.Dispose();
                    System.Threading.Thread.Sleep(500);
                    throw new ApplicationException("Error. moveHead.");
                });
                Dialog dialog = alert.Create();
                dialog.Show();
            }
            if (openIdent == null) {

                AlertDialog.Builder alert = new AlertDialog.Builder(this);
                alert.SetTitle("Napaka na aplikaciji");
                alert.SetMessage("Prišlo je do napake in aplikacija se bo zaprla.");
                alert.SetPositiveButton("Ok", (senderAlert, args) =>
                {
                    alert.Dispose();
                    System.Threading.Thread.Sleep(500);
                    throw new ApplicationException("Error. openIdent.");
                });
                Dialog dialog = alert.Create();
                dialog.Show();
            }
            docTypes = CommonData.ListDocTypes("P|N");
            
            LoadRelatedOrder();
            await Update();
            SetUpForm();
            var r = openOrder;
            // tbLocation.KeyPress += TbLocation_KeyPress;
            button4.LongClick += Button4_LongClick; 
            btSaveOrUpdate.LongClick += BtSaveOrUpdate_LongClick;
            var _broadcastReceiver = new NetworkStatusBroadcastReceiver();
            _broadcastReceiver.ConnectionStatusChanged += OnNetworkStatusChanged;
            Application.Context.RegisterReceiver(_broadcastReceiver,
            new IntentFilter(ConnectivityManager.ConnectivityAction));

            if(moveHead.GetBool("ByOrder")) {
                LockChanges();
            }

            var oi = openIdent;
            var oo = openOrder;
            var isDefault = CommonData.GetSetting("DefaultPackQtyIn");


            if (Intent.Extras != null && isDefault == "1" && Intent.Extras.GetString("update") != "1")
            {

                double UMFirst = openIdent.GetDouble("UM1toUM2");
                double UMSecond = openIdent.GetDouble("UM1toUM3");
                double resultum = UMFirst * UMSecond;
                tbPacking.Text = resultum.ToString(CommonData.GetQtyPicture());
            }

            tbSerialNum.FocusChange += TbSerialNum_FocusChange;
            if (Intent.Extras != null && Intent.Extras.GetString("update") != "1" && CurrentFlow.GetString("CurrentFlow") != "1")
            {
                byte[] trailBytes = Intent.GetByteArrayExtra("selected");
                // Deserialize the Trail object
                receivedTrail = Trail.Deserialize<Trail>(trailBytes);

                tbPacking.Text = receivedTrail.Qty;
                qtyCheck = Double.Parse(receivedTrail.Qty);
                tbLocation.Text = receivedTrail.Location;
                // ali je qty vecji od resultum in ce je uporabis resultum
                var serial = Intent.Extras.GetString("serial");
                if (CommonData.GetSetting("NoSerialnoDupOut") == "1")
                {
                    if (openIdent.GetString("SerialNo") == "O")
                    {
                        var ident = openIdent.GetString("Code");
                        var key = openOrder.GetString("Key");
                        string error2;
                        var exists = Services.GetObject("miekis", key + "|" + ident + "|" + serial, out error2);
                        if (exists != null)
                        {
                            var existsSerial = exists.GetBool("Exists");
                            if (existsSerial)
                            {
                                Toast.MakeText(this, "Serijska številka je že uporabljena", ToastLength.Long).Show();
                                tbSerialNum.Text = string.Empty;
                            }
                            else
                            {
                                tbSerialNum.Text = serial;
                            }
                        }
                    }
                    else
                    {
                        tbSerialNum.Text = serial;
                    }
                    if(Intent.Extras.GetBoolean("scan"))
                    {
                        double unformated = double.Parse(Intent.Extras.GetString("qty"));
                        tbPacking.Text = unformated.ToString(CommonData.GetQtyPicture()); 
                    }
                }
            } 
            if (CommonData.GetSetting("NoSerialnoDupOut") != "1" && extraData != null)
            {
                tbPacking.Text = extraData.GetDouble("Qty").ToString(CommonData.GetQtyPicture());
            }
            if (Intent.Extras != null)
            {

                if (Intent.Extras.GetString("update") == "1")
                {
                    string error;
                    var warehouse = moveHead.GetString("Wharehouse");
                    var stock = Services.GetObjectList("str", out error, warehouse + "|" + tbLocation.Text + "|" + moveItem.GetString("Ident"));
                    lbQty.Text = "Kol ( " + stock.Items.ElementAt(0).GetDouble("RealStock").ToString(CommonData.GetQtyPicture()) + " )";
                    qtyCheck = stock.Items.ElementAt(0).GetDouble("RealStock");
                    tbPacking.Text = moveItem.GetDouble("Qty").ToString(CommonData.GetQtyPicture());
                }

            }
            var mi = moveItem;
        }



        private async Task GetLocationsForGivenWarehouse(string warehouse)
        {
            await Task.Run(() =>
            {
                List<string> result = new List<string>();
                string error;
                var issuerLocs = Services.GetObjectList("lo", out error, warehouse);

                if (issuerLocs == null)
                {
                    RunOnUiThread(() =>
                    {
                        Toast.MakeText(this, "Prišlo je do napake", ToastLength.Long).Show();

                    });

                }
                else
                {
                    issuerLocs.Items.ForEach(x =>
                    {
                        var location = x.GetString("LocationID");

                        locations.Add(location);
                    });

                }


            });

        }


        private void TbSerialNum_FocusChange(object sender, View.FocusChangeEventArgs e)
        {
          if (!String.IsNullOrEmpty(tbSerialNum.Text))
            {
                if (CommonData.GetSetting("NoSerialnoDupOut") == "1")
                {
                    if (openIdent.GetString("SerialNo") == "O")
                    {
                        var ident = openIdent.GetString("Code");
                        var key = openOrder.GetString("Key");
                        string error2;
                        var exists = Services.GetObject("miekis", key + "|" + ident + "|" + tbSerialNum.Text, out error2);
                        if (exists != null)
                        {
                            var existsSerial = exists.GetBool("Exists");
                            if (existsSerial)
                            {
                                Toast.MakeText(this, "Serijska številka je že uporabljena", ToastLength.Long).Show();
                                tbSerialNum.Text = string.Empty;
                            }
                        }
                    }        
                }        
            }
        }

        private void LockChanges()
        {
            tbIdent.Enabled = false;
        }

        private async Task Update()
        {
            try
            {      
                await Task.Run(() =>
                {
                    if (openOrder != null && CurrentFlow.GetString("CurrentFlow") != "2")
                    {
                        string error;
                        string linkkey = openOrder.GetString("Key");
                        int linkno = openOrder.GetInt("No");
                        openOrder = Services.GetObject("oobl", linkkey + "|" + linkno.ToString(), out error);
                    } else if (openOrder == null && moveItem != null)
                    {
                        string error;
                        var openOrders = Services.GetObjectList("oo", out error, moveItem.GetString("Ident") + "|" + moveHead.GetString("DocumentType"));
                        openOrder = openOrders.Items.Where(x => x.GetString("LinkNo") == moveItem.GetString("LinkNo")).FirstOrDefault();
                    }
                });
            
            } catch (Exception err)
            {
                Crashes.TrackError(err);
            } finally
            {
                RunOnUiThread(() =>
                {
                    LoaderManifest.LoaderManifestLoopStop(this);
                });
            }
        }

        public bool IsOnline()
        {
            var cm = (ConnectivityManager) GetSystemService(ConnectivityService);
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
        private void BtSaveOrUpdate_LongClick(object sender, View.LongClickEventArgs e)
        {
            isMorePalletsMode = false;
            tbSSCC.Text = "";
            tbSerialNum.Text = "";
            tbPalette.Text = "";
            tbPacking.Text = "";
            tbLocation.Text = "";
            tbIdent.Text = "";
            tbSSCC.RequestFocus();
        }

        private void Button4_LongClick(object sender, View.LongClickEventArgs e)
        {
            isMorePalletsMode = false;
            tbSSCC.Text = "";
            tbSerialNum.Text = "";
            tbPalette.Text = "";
            tbPacking.Text = "";
            tbLocation.Text = "";
            tbIdent.Text = "";
            tbSSCC.RequestFocus();
        }

        private void TbSSCC_KeyPress(object sender, View.KeyEventArgs e)
        {
            e.Handled = false;

            if (e.KeyCode == Keycode.Enter)
            {
                var dataObject = FillRelatedData(tbSSCC.Text);
                if (dataObject != null)
                {
                    string error;

                    var ident = dataObject.Ident;
                    if (ident != null)
                    {
                        var loadIdent = CommonData.LoadIdent(ident);
                        string idname = loadIdent.GetString("Name");
                        if (!String.IsNullOrEmpty(idname))
                        {
                            ProcessQty();
                            AskAboutQuantityCorrection();
                            tbSerialNum.RequestFocus();
                        }
                        else
                        {
                            tbSSCC.Text = String.Empty;
                            return;
                        }
                    }
                }
            }
        }

        private void BtMorePallets_Click(object sender, EventArgs e)
        {
            isOkayToCallBarcode = true;
            // StartActivity(typeof(MorePalletsClass));
            popupDialogMainIssueing = new Dialog(this);
            popupDialogMainIssueing.SetContentView(Resource.Layout.MorePalletsClass);
            popupDialogMainIssueing.Window.SetSoftInputMode(SoftInput.AdjustResize);
            popupDialogMainIssueing.Show();
            popupDialogMainIssueing.Window.SetLayout(LayoutParams.MatchParent, LayoutParams.WrapContent);
            btConfirm = popupDialogMainIssueing.FindViewById<Button>(Resource.Id.btConfirm);
            btExit = popupDialogMainIssueing.FindViewById<Button>(Resource.Id.btExit);
            tbSSCCpopup = popupDialogMainIssueing.FindViewById<EditText>(Resource.Id.tbSSCC);
            tbSSCCpopup.SetBackgroundColor(Android.Graphics.Color.Aqua);
            tbSSCCpopup.KeyPress += TbSSCCpopup_KeyPress;
            lvCardMore = popupDialogMainIssueing.FindViewById<ListView>(Resource.Id.lvCardMore);
            lvCardMore.ItemLongClick += LvCardMore_ItemLongClick;
            adapterNew = new MorePalletsAdapter(this, data);
            lvCardMore.Adapter = adapterNew;
            lvCardMore.ItemSelected += LvCardMore_ItemSelected;
            btConfirm.Click += BtConfirm_Click;
            btExit.Click += BtExit_Click;
            tbSSCCpopup.RequestFocus();
        }

        private void BtExit_Click(object sender, EventArgs e)
        {
            popupDialogMainIssueing.Dismiss();
            popupDialogMainIssueing.Hide();
            isOkayToCallBarcode = false;
        }

        private void BtConfirm_Click(object sender, EventArgs e)
        {
            if (data.Count != 0)
            {
                string formatedString = $"{data.Count} skeniranih SSCC koda.";
                tbSSCC.Text = formatedString;
                tbSerialNum.Text = "...";
                tbLocation.Text = "...";
                tbIdent.Text = "...";
                tbPacking.Text = "...";
                tbLocation.RequestFocus();
                isMorePalletsMode = true;
                isBatch = true;
                popupDialogMainIssueing.Dismiss();
                popupDialogMainIssueing.Hide();
            } else
            {
                popupDialogMainIssueing.Dismiss();
                popupDialogMainIssueing.Hide();
            }
        }



        private async void SavePositions()
        {
            progress = new ProgressDialogClass();
            progress.ShowDialogSync(this, "Shranjujem pozicije.");
            foreach (var x in data)
            {

                SaveMoveItemBatch(x);

            }
            progress.StopDialogSync();
        }

        private bool SaveMoveItemBatch(MorePallets obj)
        {
            try
            {
                moveItemNew = new NameValueObject("MoveItem");
                moveItemNew.SetInt("HeadID", moveHead.GetInt("HeadID"));
                moveItemNew.SetString("LinkKey", obj.key);
                moveItemNew.SetInt("LinkNo", obj.no);
                moveItemNew.SetString("Ident", obj.Ident);
                moveItemNew.SetString("SSCC", obj.SSCC.Trim());
                moveItemNew.SetString("SerialNo", obj.Serial.Trim());
                moveItemNew.SetDouble("Packing", Convert.ToDouble(obj.Quantity.Trim()));
                moveItemNew.SetDouble("Factor", Convert.ToDouble(tbUnits.Text.Trim()));
                moveItemNew.SetDouble("Qty", Convert.ToDouble(tbUnits.Text.Trim()) * Convert.ToDouble(obj.Quantity.Trim()));
                moveItemNew.SetInt("Clerk", Services.UserID());
                moveItemNew.SetString("Location", obj.Location);
                moveItemNew.SetString("Palette", "1"); 
                string error;
                moveItemNew = Services.SetObject("mi", moveItemNew, out error);
                var jsonobj = GetJSONforMoveItem(moveItemNew);
                if (moveItemNew == null)
                {
                    RunOnUiThread(() =>
                    {
                        var debug = error;
                        Toast.MakeText(this, "Napaka pri dostopu web aplikacije." + error, ToastLength.Long).Show();
                    });
                    return false;
                }
                else
                {
                    InUseObjects.Invalidate("MoveItem");
                    return true;
                }
            } catch (Exception ex)
            {
                Crashes.TrackError(ex);
                return false;
            }
        }



        private string GetJSONforMoveItem(NameValueObject moveItem)
        {
            moveItem item = new moveItem();
            item.HeadID = moveHead.GetInt("HeadID");
            item.LinkKey = moveItem.GetString("LinkKey");
            item.LinkNo = moveItem.GetInt("LinkNo");
            item.Ident = moveItem.GetString("Ident");
            item.SSCC = moveItem.GetString("SSCC");
            item.SerialNo = moveItem.GetString("SerialNo");
            item.Packing = moveItem.GetDouble("Packing");
            item.Factor = moveItem.GetDouble("Factor");
            item.Qty = moveItem.GetDouble("Qty");
            item.Clerk = moveItem.GetInt("Clerk");
            item.Location = moveItem.GetString("Location");
            item.IssueLocation = moveItem.GetString("IssueLocation");
            item.Pallete = moveItem.GetString("Pallete");
            var jsonString = JsonConvert.SerializeObject(item);
            return jsonString;
        }

      


        private void LvCardMore_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            var index = e.Position;
            var element = data.ElementAt(index);
            string formated = $"Izbrali ste {element.SSCC}.";
            Toast.MakeText(this, formated, ToastLength.Long).Show();
        }

        private void LvCardMore_ItemLongClick(object sender, AdapterView.ItemLongClickEventArgs e)
        {
            var index = e.Position;
            var item =  adapterNew.retunObjectAt(index);
            AlertDialog.Builder alert = new AlertDialog.Builder(this);
            alert.SetTitle("Podatki o kodi");
            alert.SetMessage($"Podatki:\nIdent: {item.Ident}\nNaziv: {item.Name}\nSSCC: {item.SSCC}\nKoličina: {item.Quantity}\n");
            // Close button
            alert.SetNegativeButton("Zapri", (senderAlert, args) =>
            {
                
            });
            alert.SetPositiveButton("Pobriši", (senderAlert, args) =>
            {
                DeleteFromTouch(index);
            });
            Dialog dialog = alert.Create();
            dialog.Show();
   
        }
        private void DeleteFromTouch(int index)
        {
            popupDialog = new Dialog(this);
            popupDialog.SetContentView(Resource.Layout.YesNoPopUp);
            popupDialog.Window.SetSoftInputMode(SoftInput.AdjustResize);
            popupDialog.Show();
            popupDialog.Window.SetLayout(LayoutParams.MatchParent, LayoutParams.WrapContent);
            popupDialog.Window.SetBackgroundDrawableResource(Android.Resource.Color.HoloOrangeLight);
            // Access Popup layout fields like below
            btnYes = popupDialog.FindViewById<Button>(Resource.Id.btnYes);
            btnNo = popupDialog.FindViewById<Button>(Resource.Id.btnNo);
            btnYes.Click += (e, ev) => { Yes(index); };
            btnNo.Click += (e, ev) => { No(index); };
        }

        private void No(int index)
        {
            popupDialog.Dismiss();
            popupDialog.Hide();
        }

        private void Yes(int index)
        {
            data.RemoveAt(index);
            lvCardMore.Adapter = null;
            lvCardMore.Adapter = adapterNew;
            popupDialog.Dismiss();
            popupDialog.Hide();
        }
     
        private MorePallets FillData(string barcode, bool transport)
        {
            MorePallets morePallets = new MorePallets();
            if (!String.IsNullOrEmpty(barcode) && barcode != "Scan fail")
            {
                switch(CurrentFlow.GetString("CurrentFlow"))
                {
                    case "1":

                        existsDuplicate = data.Where(x => x.SSCC == barcode).FirstOrDefault();
                        if (existsDuplicate != null)
                        {
                            Toast.MakeText(this, $"Ne morete skenirati {existsDuplicate.SSCC} še enkrat.", ToastLength.Long);
                            return morePallets;
                        }
                        if(openIdent == null)
                        {
                            return morePallets;
                        }
                        ident = openIdent.GetString("Code");
                        sscc = barcode;
                        warehouse = moveHead.GetString("Wharehouse");
                        var keyr = openOrder; 
                        query = $"SELECT * FROM uWMSItemBySSCCWarehouseItem WHERE acIdent = '{ident}' AND acSSCC = '{sscc}' AND acWarehouse = '{warehouse}' AND acKey = '{keyr.GetString("Key")}'";
                        result = Services.GetObjectListBySql(query);
                        if(result.Success && result.Rows.Count > 0)
                        {
                            MorePallets instance = new MorePallets();
                            Row row = result.Rows[0];
                            string identValue = row.StringValue("acIdent");
                            string name = row.StringValue("acName");
                            double? qty = row.DoubleValue("anQty");
                            string location = row.StringValue("aclocation");
                            string serial = row.StringValue("acSerialNo");
                            string sscc = row.StringValue("acSSCC");
                            string warehouse = row.StringValue("acWarehouse");
                            long? no = row.IntValue("anNo");
                            string keyResponse = row.StringValue("acKey");
                            instance.Ident = identValue;
                            instance.Name = name;
                            instance.Quantity = qty.ToString();
                            instance.Location = location;
                            instance.Serial = serial;
                            instance.SSCC = sscc;
                            instance.no = (int)no;
                            instance.key = keyResponse;
                            instance.friendlySSCC = sscc.Substring(sscc.Length - 5);
                            data.Add(instance);
                            if (!transport)
                            {
                                adapterNew.NotifyDataSetChanged();
                            } else
                            {
                                if (tbSerialNum.Enabled == true)
                                {                          
                                    tbSerialNum.Text = serial;
                                    tbLocation.Text = location;
                                }
                                else
                                {
                                    tbLocation.Text = location;
                                }                                           
                            }
                            return instance;
                        }                                           
                        break;
                    case "2":
                        existsDuplicate = data.Where(x => x.SSCC == barcode).FirstOrDefault();
                        if (existsDuplicate != null)
                        {
                            Toast.MakeText(this, $"Ne morete skenirati {existsDuplicate.SSCC} še enkrat.", ToastLength.Long);
                            return morePallets;
                        }
                        if (openOrder == null)
                        {
                            return morePallets;
                        }
                        sscc = barcode;
                        int noPosition = openOrder.GetInt("No");
                        string key = openOrder.GetString("Key");
                        query = $"SELECT * FROM uWMSItemBySSCCKeyNo WHERE anNo = '{noPosition}' AND acKey = '{key}' AND acSSCC = '{barcode}' ;";
                        result = Services.GetObjectListBySql(query);
                        if (result.Success && result.Rows.Count > 0)
                        {
                            MorePallets instance = new MorePallets();
                            Row row = result.Rows[0];
                            string identValue = row.StringValue("acIdent");
                            string name = row.StringValue("acName");
                            double? qty = row.DoubleValue("anQty");
                            string location = row.StringValue("aclocation");
                            string serial = row.StringValue("acSerialNo");
                            string sscc = row.StringValue("acSSCC");
                            string warehouse = row.StringValue("acWarehouse");
                            long? no = row.IntValue("anNo");
                            string keyResponse = row.StringValue("acKey");
                            instance.Ident = identValue;
                            instance.Name = name;
                            instance.Quantity = qty.ToString();
                            instance.Location = location;
                            instance.Serial = serial;
                            instance.SSCC = sscc;
                            instance.no = (int)no;
                            instance.key = keyResponse;
                            instance.friendlySSCC = sscc.Substring(sscc.Length - 5);
                            data.Add(instance);         
                            if (!transport)
                            {
                                adapterNew.NotifyDataSetChanged();
                            }
                            else
                            {
                                if (tbSerialNum.Enabled == true)
                                {
                                    tbSerialNum.Text = serial;
                                    tbLocation.Text = location;
                                }
                                else
                                {
                                    tbLocation.Text = location;
                                }                            
                            }
                            return instance;
                        }
                        break;                 
                 }           
            }

            return morePallets;
        }

        private void TbSSCCpopup_KeyPress(object sender, View.KeyEventArgs e)
        {
            e.Handled = false;
            if (e.Event.Action == KeyEventActions.Down && e.KeyCode == Keycode.Enter)
            {
                // Add your logic here 
                FillData(tbSSCCpopup.Text, false);
            }
        }
        private void TbPacking_FocusChange(object sender, View.FocusChangeEventArgs e)
        {
            ProcessQty();
        }


        private void Button5_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(IssuedGoodsEnteredPositionsView));
            Finish();
            InvalidateAndClose();
        }

        private void LoadRelatedOrder()
        {
            if (openOrder != null)
            {
                isOpened = false;           
            } else
            {
                isOpened = true;
            }
        }

        private void fillSugestedLocation(string warehouse)
        {
            var ident = openIdent.GetString("Code");
            string result;      
            if (WebApp.Get("mode=bestLoc&wh=" + warehouse + "&ident=" + HttpUtility.UrlEncode(ident) + "&locMode=outgoing", out result))
            {
               tbLocation.Text = result;            
            }  
        }


        private void Button7_Click(object sender, EventArgs e)
        {     
            StartActivity(typeof(IssuedGoodsEnteredPositionsView));
            Finish();
            InvalidateAndClose();            
        }

        
        private void InvalidateAndClose()
        {
            InUseObjects.Invalidate("ExtraData");         
        }


        private async Task FinishMethod()
        {
            await Task.Run(async () =>
            {
                var resultAsync = SaveMoveItem().Result;
                if (resultAsync)
                {
                    RunOnUiThread(() =>
                    {
                        progress = new ProgressDialogClass();
                        progress.ShowDialogSync(this, "Zaključujem");
                    });
           
                    try
                    {

                        var headID = moveHead.GetInt("HeadID");

                        string result;

                        if (WebApp.Get("mode=finish&stock=remove&print=" + Services.DeviceUser() + "&id=" + headID.ToString(), out result))
                        {
                            if (result.StartsWith("OK!"))
                            {

                                RunOnUiThread(() =>
                                {
                                    progress.StopDialogSync();

                                    var id = result.Split('+')[1];

                                    Toast.MakeText(this, "Zaključevanje uspešno! Št. izdaje:\r\n" + id, ToastLength.Long).Show();

                                    InvalidateAndClose();

                                    AlertDialog.Builder alert = new AlertDialog.Builder(this);

                                    alert.SetTitle("Zaključevanje uspešno");

                                    alert.SetMessage("Zaključevanje uspešno! Št.prevzema:\r\n" + id);

                                    alert.SetPositiveButton("Ok", (senderAlert, args) =>
                                    {
                                        alert.Dispose();
                                        System.Threading.Thread.Sleep(500);
                                        StartActivity(typeof(IssuedGoodsBusinessEventSetup));
                                    });



                                    Dialog dialog = alert.Create();
                                    dialog.Show();
                                });
                              
                            }
                            else
                            {
                                RunOnUiThread(() =>
                                {
                                    progress.StopDialogSync();
                                    AlertDialog.Builder alert = new AlertDialog.Builder(this);
                                    alert.SetTitle("Napaka");
                                    alert.SetMessage("Napaka pri zaključevanju: " + result);

                                    alert.SetPositiveButton("Ok", (senderAlert, args) =>
                                    {
                                        alert.Dispose();
                                        System.Threading.Thread.Sleep(500);
                                        StartActivity(typeof(MainMenu));

                                    });



                                    Dialog dialog = alert.Create();
                                    dialog.Show();
                                });
                            }
                        }
                        else
                        {
                            Toast.MakeText(this, "Napaka pri klicu do web aplikacije" + result, ToastLength.Long).Show();

                        }
                    }
                    finally
                    {
                        RunOnUiThread(() =>
                        {
                            progress.StopDialogSync();

                        });
                    }
                }
            });
        }

      

     

       

        private async void Button6_Click(object sender, EventArgs e)
        {
            popupDialogConfirm = new Dialog(this);
            popupDialogConfirm.SetContentView(Resource.Layout.Confirmation);
            popupDialogConfirm.Window.SetSoftInputMode(SoftInput.AdjustResize);
            popupDialogConfirm.Show();
            popupDialogConfirm.Window.SetLayout(LayoutParams.MatchParent, LayoutParams.WrapContent);
            popupDialogConfirm.Window.SetBackgroundDrawableResource(Android.Resource.Color.HoloRedLight);
            // Access Popup layout fields like below
            btnYesConfirm = popupDialogConfirm.FindViewById<Button>(Resource.Id.btnYes);
            btnNoConfirm = popupDialogConfirm.FindViewById<Button>(Resource.Id.btnNo);
            btnYesConfirm.Click += BtnYesConfirm_Click;
            btnNoConfirm.Click += BtnNoConfirm_Click;
        }

        private void BtnNoConfirm_Click(object sender, EventArgs e)
        {
            popupDialogConfirm.Dismiss();
            popupDialogConfirm.Hide();
        }

        private async void BtnYesConfirm_Click(object sender, EventArgs e)
        {
             await FinishMethod();   
        }

        private async void Button4_Click(object sender, EventArgs e)
        {
            if (!isMorePalletsMode)
            {
                var result = SaveMoveItem().Result;
                if (result)
                {
                    if (moveHead.GetBool("ByOrder") && CurrentFlow.GetString("CurrentFlow") == "2")
                    {
                        StartActivity(typeof(IssuedGoodsIdentEntryWithTrail));
                        Finish();
                    }
                    else if (CurrentFlow.GetString("CurrentFlow") == "1")
                    {
                        StartActivity(typeof(IssuedGoodsIdentEntry));
                        Finish();
                    } 
                    InvalidateAndClose();
                }
            }
            else
            {
                if (String.IsNullOrEmpty(tbLocation.Text))
                {
                    return;
                }
                SavePositions();
                if (moveHead.GetBool("ByOrder") && CurrentFlow.GetString("CurrentFlow") == "2")
                {
                    StartActivity(typeof(IssuedGoodsIdentEntryWithTrail));
                    Finish();
                }
                else if (CurrentFlow.GetString("CurrentFlow") == "1")
                {
                    StartActivity(typeof(IssuedGoodsIdentEntry));
                    Finish();
                }
                InvalidateAndClose();

            }
        }

        private async void BtSaveOrUpdate_Click(object sender, EventArgs e)
        {
            if (!isMorePalletsMode)
            {
                var result = SaveMoveItem().Result;
                if (result)
                {
                    if (editMode)
                    {
                        StartActivity(typeof(IssuedGoodsEnteredPositionsView));
                        Finish();

                    }
                    else
                    {
                        StartActivity(typeof(IssuedGoodsSerialOrSSCCEntry));
                        Finish();

                    }

                } else
                {
                    Toast.MakeText(this, "Obstaja napaka na podatkih.", ToastLength.Long).Show();
                }
            }
            else
            {
                if (String.IsNullOrEmpty(tbLocation.Text))
                {
                    return;
                }
                SavePositions();

                if (moveHead.GetBool("ByOrder") && CurrentFlow.GetString("CurrentFlow") == "2")
                {
                    StartActivity(typeof(IssuedGoodsIdentEntryWithTrail));
                    Finish();
                }
                else if (CurrentFlow.GetString("CurrentFlow") == "1")
                {
                    StartActivity(typeof(IssuedGoodsIdentEntry));
                    Finish();
                }
                InvalidateAndClose();
            }
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            var qty = tbPacking.Text;
            if (qty.Trim().StartsWith("-"))
            {
                qty = qty.Trim().Substring(1);
            }
            else
            {
                qty = "-" + qty;
            }
            tbPacking.Text = qty;
        }

        private void SetUpForm()
        {
            tbSSCC.Enabled = openIdent.GetBool("isSSCC");
            tbSerialNum.Enabled = openIdent.GetBool("HasSerialNumber");

            if (moveItem != null)
            {
                tbIdent.Text = moveItem.GetString("IdentName");
                tbSerialNum.Text = moveItem.GetString("SerialNo");
                tbSSCC.Text = moveItem.GetString("SSCC");
                tbLocation.Text = moveItem.GetString("Location");
                tbPalette.Text = moveItem.GetString("Palette");
                tbPacking.Text = moveItem.GetDouble("Packing").ToString();
                tbUnits.Text = moveItem.GetDouble("Factor").ToString();
                btSaveOrUpdate.Text = "Serij. - F2";
            }
            else
            {
                tbIdent.Text = openIdent.GetString("Code") + " " + openIdent.GetString("Name");

                if (extraData != null)
                {
                  
                    tbLocation.Text = extraData.GetString("Location");
                    tbPacking.Text = extraData.GetDouble("Qty").ToString();
                }
            }

            isPackaging = openIdent.GetBool("IsPackaging");
            if (isPackaging)
            {
                tbSSCC.Enabled = false;
                tbSerialNum.Enabled = false;
            }
            else
            {

            }

            if (CommonData.GetSetting("ShowPaletteField") == "1")
            {
                lbPalette.Visibility = ViewStates.Visible;
                tbPalette.Visibility = ViewStates.Visible;
            }

            if (string.IsNullOrEmpty(tbUnits.Text.Trim())) { tbUnits.Text = "1"; }

            if (CommonData.GetSetting("ShowNumberOfUnitsField") == "1")
            {
                lbUnits.Visibility = ViewStates.Visible;
                tbUnits.Visibility = ViewStates.Visible;
            }


            tbIdent.RequestFocus();
            var ident = openIdent.GetString("Code");
            var location = CommonData.GetSetting("DefaultProductionLocation");
            if (tbLocation.Text == String.Empty)
            {
                if (location != null)
                {
                    tbLocation.Text = location;

                }

            }
            var warehouse = moveHead.GetString("Wharehouse");
            if (moveItem != null) { }
            else
            {
                fillSugestedLocation(warehouse);
            }

            tbSSCC.RequestFocus();
        
        }
     

        private bool LoadStock(string warehouse, string location, string sscc, string serialNum, string ident)
        {
            try
            {
                string error;
                stock = Services.GetObject("str", warehouse + "|" + location + "|" + sscc + "|" + serialNum + "|" + ident, out error);
                if (stock == null)
                {
                    Toast.MakeText(this, "Napaka pri dostopu do web aplikacije" + error, ToastLength.Long).Show();
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                Crashes.TrackError(ex);
                return false;
            }
        }



        private static bool? checkIssuedOpenQty = null;
        private ProgressDialogClass progress;
        private Dialog popupDialogMain;
        private Button btConfirm;
        private Button btExit;
        private EditText tbSSCCpopup;
        private ListView lvCardMore;
        private MorePalletsAdapter adapter;
        private Dialog popupDialog;
        private Button btnYes;
        private Button btnNo;
        private bool isFirst;
        private bool isMorePalletsMode = false;
        private bool isBatch;
        private int check;
        private bool isOkayToCallBarcode;
        private MorePalletsAdapter adapterNew;
        private NameValueObject moveItemNew;
        private Dialog popupDialogConfirm;
        private Button btnYesConfirm;
        private Button btnNoConfirm;
        private CustomAutoCompleteAdapter<string> DataAdapter;
        private string qtyStock;
        private CustomAutoCompleteAdapter<string> adapterReceive;
        private MorePallets existsDuplicate;
        private string error;
        private string query;
        private ApiResultSet result;
        private NameValueObject dataObject;
        private string ident;
        private string sscc;
        private string warehouse;
        private Dialog popupDialogMainIssueing;

        private bool CheckIssuedOpenQty()
        {
            if (checkIssuedOpenQty == null)
            {                  
                try
                {
                    string error;
                    var useObj = Services.GetObject("cioqUse", "", out error);
                    checkIssuedOpenQty = useObj == null ? false : useObj.GetBool("Use");
                }
                catch(Exception ex)
                {
                    Crashes.TrackError(ex);
                }
            }
            return (bool)checkIssuedOpenQty;
        }
       

        private async Task<bool> SaveMoveItem()
        {
            // Is the user trying to issue more than the available stock
            try
            {
                double result;
                if (double.TryParse(tbPacking.Text, out result))
                {
                    if (qtyCheck < double.Parse(tbPacking.Text) && CommonData.GetSetting("CheckIssuedOpenQty") == "1")
                    {
                        RunOnUiThread(() =>
                        {
                            Toast.MakeText(this, "Ne morete izdati več kot je trenutno na zalogi!", ToastLength.Long).Show();
                        });
                        return false;
                    }
                } else
                {
                    return false;
                }
            } catch
            {
                RunOnUiThread(() =>
                {
                    Toast.MakeText(this, "Nepravilen vnos", ToastLength.Long).Show();
                });
                return false;
            }

            if (string.IsNullOrEmpty(tbPacking.Text.Trim()))
            {
                return true;
            }

            if (tbSSCC.Enabled && string.IsNullOrEmpty(tbSSCC.Text.Trim()))
            {
                RunOnUiThread(() =>
                {
                    Toast.MakeText(this, "SSCC koda je obvezen podatek", ToastLength.Long).Show();

                    tbSSCC.RequestFocus();
                });
            
                return false;
            }
            if (tbSerialNum.Enabled && string.IsNullOrEmpty(tbSerialNum.Text.Trim()))
            {
                RunOnUiThread(() =>
                {
                    Toast.MakeText(this, "Serijska številka je obvezen podatek!", ToastLength.Long).Show();

                    tbSerialNum.RequestFocus();
                });
               
                return false;
            }

            if (!CommonData.IsValidLocation(moveHead.GetString("Wharehouse"), tbLocation.Text.Trim()))
            {
                RunOnUiThread(() =>
                {
                    Toast.MakeText(this, "Lokacija '" + tbLocation.Text.Trim() + "' ni veljavna za skladišče '" + moveHead.GetString("Wharehouse") + "'!", ToastLength.Long).Show();

                    tbLocation.RequestFocus();
                });
               
                return false;
            }
            if (!LoadStock(moveHead.GetString("Wharehouse"), tbLocation.Text.Trim(), tbSSCC.Text.Trim(), tbSerialNum.Text.Trim(), openIdent.GetString("Code")))
            {
                return false;
            }

            if (string.IsNullOrEmpty(tbPacking.Text.Trim()))
            {
                RunOnUiThread(() =>
                {
                    Toast.MakeText(this, "Količina je obvezen podatek", ToastLength.Long).Show();
                    tbPacking.RequestFocus();
                });
            
                return false;
            }
            else
            {
                try
                {
                    var qty = Convert.ToDouble(tbPacking.Text.Trim());
                    if (qty == 0.0)
                    {
                        RunOnUiThread(() =>
                        {
                            Toast.MakeText(this, "Količina je obvezen podatek in mora biti različna od nič.", ToastLength.Long).Show();

                            tbPacking.RequestFocus();
                        });
                   
                        return false;
                    }

                    if (moveHead.GetBool("ByOrder") && !isPackaging && CheckIssuedOpenQty())
                    {
                        var tolerance = openIdent.GetDouble("TolerancePercent");
                        var maxVal = Math.Abs(openOrder.GetDouble("OpenQty") * (1.0 + tolerance / 100));
                        if (Math.Abs(qty) > maxVal)
                        {
                            RunOnUiThread(() =>
                            {
                                Toast.MakeText(this, "Količina presega (" + qty.ToString(CommonData.GetQtyPicture()) + ") naročilo (" + maxVal.ToString(CommonData.GetQtyPicture()) + ")!", ToastLength.Long).Show();
                                tbPacking.RequestFocus();
                            });
                         
                            return false;
                        }
                    }
                }
                catch (Exception e)
                {
                    RunOnUiThread(() =>
                    {
                        Toast.MakeText(this, "Količina mora biti število (" + e.Message + ")!", ToastLength.Long).Show();

                        tbPacking.RequestFocus();
                    });
                
                    return false;
                }
            }

            if (string.IsNullOrEmpty(tbUnits.Text.Trim()))
            {
                RunOnUiThread(() =>
                {
                    Toast.MakeText(this, "Število enota je obvezan podatek", ToastLength.Long).Show();
                    tbUnits.RequestFocus();
                });
             
                return false;
            }
            else
            {
                try
                {
                    var units = Convert.ToDouble(tbUnits.Text.Trim());
                    if (units == 0.0)
                    {
                        RunOnUiThread(() =>
                        {
                            Toast.MakeText(this, "Število enota je obvezan podatek in more biti raličit o nič", ToastLength.Long).Show();
                            tbUnits.RequestFocus();
                        });
                      
                        return false;
                    }
                }
                catch (Exception e)
                {
                    RunOnUiThread(() =>
                    {
                        Toast.MakeText(this, "Število enota mora biti število (" + e.Message, ToastLength.Long).Show();

                        tbUnits.RequestFocus();
                    });
            
                    return false;
                }
            }

            if (CommonData.GetSetting("IssuedGoodsPreventSerialDups") == "1")
            {            
                try
                {                 
                    var headID = moveHead.GetInt("HeadID");
                    var serialNo = tbSerialNum.Text.Trim();
                    var sscc = tbSSCC.Text.Trim();
                    string result;

                    if (WebApp.Get("mode=canAddSerial2&hid=" + headID.ToString() + "&sn=" + serialNo + "&sscc=" + sscc + "&ident=" + openIdent.GetString("Code"), out result))
                    {
                        if (!result.StartsWith("OK!"))
                        {
                            RunOnUiThread(() =>
                            {
                                Toast.MakeText(this, result, ToastLength.Long).Show();
                            });
                            return false;
                        }
                    }
                    else
                    {
                        RunOnUiThread(() =>
                        {
                            Toast.MakeText(this, "Napaka pri klicu web aplikacije" + result, ToastLength.Long).Show();
                        });
                        return false;
                    }
                }
                catch(Exception err) 
                {
                    Crashes.TrackError(err);
                    return false;
                }
            }
  
            try
            {
                    if (moveItem == null) { 
                        moveItem = new NameValueObject("MoveItem");
                    }
                    moveItem.SetInt("HeadID", moveHead.GetInt("HeadID"));
                    var No = moveHead.GetInt("LinkNoTrail");
                    if (openOrder != null)
                    {                  
                    moveItem.SetString("LinkKey", openOrder.GetString("Key"));
                    moveItem.SetInt("LinkNo", openOrder.GetInt("No"));
                    moveItem.SetString("Ident", openIdent.GetString("Code"));
                    moveItem.SetString("SSCC", tbSSCC.Text.Trim());
                    moveItem.SetString("SerialNo", tbSerialNum.Text.Trim());
                    moveItem.SetDouble("Packing", Convert.ToDouble(tbPacking.Text.Trim()));
                    moveItem.SetDouble("Factor", Convert.ToDouble(tbUnits.Text.Trim()));
                    moveItem.SetDouble("Qty", Convert.ToDouble(tbUnits.Text.Trim()) * Convert.ToDouble(tbPacking.Text.Trim()));
                    moveItem.SetInt("Clerk", Services.UserID());
                    moveItem.SetString("Location", tbLocation.Text.Trim());
                    moveItem.SetString("Palette", tbPalette.Text.Trim());
                    string error;
                    moveItem = Services.SetObject("mi", moveItem, out error);

                    if (moveItem == null)
                    {
                        RunOnUiThread(() =>
                        {
                            var debug = error;
                            Toast.MakeText(this, "Napaka pri dostopu web aplikacije." + error, ToastLength.Long).Show();
                        });

                        return false;
                    }
                    else
                    {

                        InUseObjects.Invalidate("MoveItem");                     
                        return true;
                    }
                } else
                {
                    moveItem.SetString("LinkKey", moveItem.GetString("LinkKey"));
                    moveItem.SetInt("LinkNo", moveItem.GetInt("LinkNo"));
                    moveItem.SetString("Ident", openIdent.GetString("Code"));
                    moveItem.SetString("SSCC", tbSSCC.Text.Trim());
                    moveItem.SetString("SerialNo", tbSerialNum.Text.Trim());
                    moveItem.SetDouble("Packing", Convert.ToDouble(tbPacking.Text.Trim()));
                    moveItem.SetDouble("Factor", Convert.ToDouble(tbUnits.Text.Trim()));
                    moveItem.SetDouble("Qty", Convert.ToDouble(tbUnits.Text.Trim()) * Convert.ToDouble(tbPacking.Text.Trim()));
                    moveItem.SetInt("Clerk", Services.UserID());
                    moveItem.SetString("Location", tbLocation.Text.Trim());
                    moveItem.SetString("Palette", tbPalette.Text.Trim());
                    string error;
                    moveItem = Services.SetObject("mi", moveItem, out error);

                    if (moveItem == null)
                    {
                        RunOnUiThread(() =>
                        {
                            var debug = error;
                            Toast.MakeText(this, "Napaka pri dostopu web aplikacije." + error, ToastLength.Long).Show();
                        });

                        return false;
                    }
                    else
                    {
                        InUseObjects.Invalidate("MoveItem");
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Crashes.TrackError(ex);
                return false;
            }
        }


        private void ProcessQty()
        {
            if (!CommonData.IsValidLocation(moveHead.GetString("Wharehouse"), tbLocation.Text.Trim()))
            {
                Toast.MakeText(this, "Lokacija '" + tbLocation.Text.Trim() + "' ni veljavna za skladišče '" + moveHead.GetString("Wharehouse") + "'!", ToastLength.Long).Show();
                tbLocation.RequestFocus();
                return;
            }

            if (!LoadStock(moveHead.GetString("Wharehouse"), tbLocation.Text.Trim(), tbSSCC.Text.Trim(), tbSerialNum.Text.Trim(), openIdent.GetString("Code")))
            {
                Toast.MakeText(this, "Zaloga za SSCC/Serijsko št. ne obstaja.", ToastLength.Long).Show();         
                return;
            }
            else
            {
                NameValueObject fulfilledOrder = new NameValueObject();
                double fulfilledQty = 0.0;
                Double qty = 0.0;
                // 22.09.2023 Testiranje za Bimex pogoj ni potreben
                if (openOrder != null /* && CommonData.GetSetting("UseSingleOrderIssueing" )  != "1"  */ )
                { 
                    qty = openOrder.GetDouble("OpenQty");
                    string error;
                    fulfilledOrder = Services.GetObject("miho", openOrder.GetString("Key") + "|" + openOrder.GetInt("No") + "|" + openIdent.GetString("Code"), out error);
                    fulfilledQty = fulfilledOrder == null ? 0.0 : fulfilledOrder.GetDouble("Qty");
                    tbPacking.Text = stock.GetDouble("RealStock").ToString(CommonData.GetQtyPicture());
                    if(CurrentFlow.GetString("CurrentFlow") == "1")
                    {
                        if (qtyStock != null)
                        {
                            lbQty.Text = "Kol. (" + (Double.Parse(qtyStock)).ToString(CommonData.GetQtyPicture()) + ")";
                            qtyCheck = Double.Parse(qtyStock);
                        }
                        else
                        {
                            lbQty.Text = "Kol. (" + (openOrder.GetDouble("OpenQty")).ToString(CommonData.GetQtyPicture()) + ")";
                            qtyCheck = openOrder.GetDouble("OpenQty");
                        }
                    } else
                    {
                        if (extraData != null)
                        {
                            lbQty.Text = "Kol. (" + (extraData.GetDouble("Qty")).ToString(CommonData.GetQtyPicture()) + ")";
                            qtyCheck = extraData.GetDouble("Qty");
                        }
                        else
                        {
                            // Če je OpenQty večji kot zaloga se kaže zaloga
                            lbQty.Text = "Kol. (" + (openOrder.GetDouble("OpenQty")).ToString(CommonData.GetQtyPicture()) + ")";
                            qtyCheck = openOrder.GetDouble("OpenQty");
                        }
                    }                
                }         
            }          
        }


        private MorePallets ProcessQtyWithParams(MorePallets obj, string location)
        {
            try
            {
                if (!CommonData.IsValidLocation(moveHead.GetString("Wharehouse"), obj.Location.Trim()))
                {
                    Toast.MakeText(this, "Lokacija '" + obj.Location.Trim() + "' ni veljavna za skladišče '" + moveHead.GetString("Wharehouse") + "'!", ToastLength.Long).Show();
                    tbLocation.RequestFocus();
                    return null;
                }
                if (!LoadStock(moveHead.GetString("Wharehouse"), obj.Location.Trim(), obj.SSCC.Trim(), obj.Serial.Trim(), openIdent.GetString("Code")))
                {
                    Toast.MakeText(this, "Zaloga za SSCC/Serijsko št. ne obstaja.", ToastLength.Long).Show();
                    return null;
                }
                else
                {
                    string error;
                    var fulfilledOrder = Services.GetObject("miho", openOrder.GetString("Key") + "|" + openOrder.GetInt("No") + "|" + openIdent.GetString("Code"), out error);
                    var fulfilledQty = fulfilledOrder == null ? 0.0 : fulfilledOrder.GetDouble("Qty");
                    obj.Quantity = stock.GetDouble("RealStock").ToString(CommonData.GetQtyPicture());
                    //  lbQty.Text = "Kol. (" + (openOrder.GetDouble("OpenQty") - fulfilledQty).ToString(CommonData.GetQtyPicture()) + ")";                  
                    return obj;
                }
            } catch
            {
                return null;
            }
        }
        private void colorFields()
        {
            tbSSCC.SetBackgroundColor(Android.Graphics.Color.Aqua);
            tbSerialNum.SetBackgroundColor(Android.Graphics.Color.Aqua);
            tbLocation.SetBackgroundColor(Android.Graphics.Color.Aqua);            
        }
        private void Button3_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(IssuedGoodsIdentEntryWithTrail));
            Finish();
        }

        public void GetBarcode(string barcode)
        {
            try
            {
                if (tbSSCC.HasFocus && isOkayToCallBarcode == false)
                {
                    if (barcode != "Scan fail")
                    {
                        tbSSCC.Text = "";
                        tbSerialNum.Text = "";
                        tbPalette.Text = "";
                        tbPacking.Text = "";
                        tbLocation.Text = "";
                        Sound();
                        tbSSCC.Text = barcode;
                        var dataObject = FillRelatedData(tbSSCC.Text);
                        if (dataObject != null && !String.IsNullOrEmpty(dataObject.Ident))
                        {
                            string error;
                            var ident = dataObject.Ident;
                            var loadIdent = CommonData.LoadIdent(ident);
                            string idname = loadIdent.GetString("Name");
                            if (!String.IsNullOrEmpty(idname))
                            {
                                ProcessQty();
                                AskAboutQuantityCorrection();
                                tbSerialNum.RequestFocus();
                            }
                            else
                            {
                                tbSSCC.Text = String.Empty;
                                return;
                            }
                        }
                    }
                }
                else if (tbSerialNum.HasFocus && isOkayToCallBarcode == false)
                {
                    if (barcode != "Scan fail")
                    {
                        Sound();

                        if (CommonData.GetSetting("NoSerialnoDupOut") == "1")
                        {

                            if (openIdent.GetString("SerialNo") == "O")
                            {

                                var ident = openIdent.GetString("Code");
                                var key = openOrder.GetString("Key");
                                var serial = barcode;
                                string error2;
                                var exists = Services.GetObject("miekis", key + "|" + ident + "|" + serial, out error2);
                                if (exists != null)
                                {
                                    var existsSerial = exists.GetBool("Exists");
                                    if (existsSerial)
                                    {
                                        Toast.MakeText(this, "Serijska številka je že uporabljena", ToastLength.Long).Show();
                                        tbSerialNum.Text = string.Empty;
                                    }
                                    else
                                    {
                                        tbSerialNum.Text = barcode;
                                    }
                                }
                            }
                            else
                            {
                                tbSerialNum.Text = barcode;
                            }
                        }
                        else
                        {
                            tbSerialNum.Text = barcode;
                        }
                        tbLocation.RequestFocus();
                    }
                }
                else if (tbLocation.HasFocus && isOkayToCallBarcode == false)
                {
                    if (barcode != "Scan fail")
                    {
                        Sound();
                        tbLocation.Text = barcode;
                        tbPacking.RequestFocus();
                    }
                }
                else if (isOkayToCallBarcode)
                {
                    if (tbSSCCpopup.HasFocus)
                    {
                        // This method fills the popup field with data
                        FillData(barcode, false);
                    }
                }
            } catch (Exception ex)
            {
                Crashes.TrackError(ex);
                Toast.MakeText(this, "Prišlo je do napake", ToastLength.Long).Show();
            }
        }

        private void AskAboutQuantityCorrection()
        {
            if (openOrder != null && stock != null) {
                // Return in the case that its not needed.
                if (openOrder.GetDouble("OpenQty") >= stock.GetDouble("RealStock"))
                {
                    return;
                }

                AlertDialog.Builder alertDialogBuilder = new AlertDialog.Builder(this);
                // Set the title and message
                alertDialogBuilder.SetTitle("Izbor količine");
                alertDialogBuilder.SetMessage("Ali želite izdati celotno količino ali samo naročeno?");
                // Add two buttons with their respective actions
                alertDialogBuilder
                   .SetPositiveButton("Celotno", (senderAlert, args) =>
                    {
                        return;
                    })
                    .SetNegativeButton("Naročeno", (senderAlert, args) =>
                    {
                        tbPacking.Text = openOrder.GetDouble("OpenQty").ToString(CommonData.GetQtyPicture());
                    });
                AlertDialog alertDialog = alertDialogBuilder.Create();
                alertDialog.Show();
            }
        }

        private MorePallets FillRelatedData(string text)
        {
            return FillData(text, true);
        }

        private void TransportOneObject(string sscc)
        {

            if (!String.IsNullOrEmpty(sscc))
            {
                string error;
                var dataObject = Services.GetObject("sscc", sscc, out error);
                if (dataObject != null)
                {
                    var ident = dataObject.GetString("Ident");
                    var loadIdent = CommonData.LoadIdent(ident);
                    var name = dataObject.GetString("IdentName");
                    var serial = dataObject.GetString("SerialNo");
                    var location = dataObject.GetString("Location");
                    MorePallets pallets = new MorePallets();
                    pallets.Ident = ident;
                    string idname = loadIdent.GetString("Name");
                    // Validation

                    if (String.IsNullOrEmpty(idname))
                    {
                        return;
                    }

                    pallets.Location = location;

                    try
                    {
                        pallets.Name = idname.Trim().Substring(0, 3);
                    } catch (Exception exception)
                    {
                        Crashes.TrackError(exception);
                    }

                    pallets.Quantity = sscc;
                    pallets.SSCC = sscc;
                    pallets.Serial = serial;

                    try
                    {
                        pallets.friendlySSCC = pallets.SSCC.Substring(pallets.SSCC.Length - 3);
                    }
                    catch (Exception)
                    {
                        pallets.Name = "Error";
                    }
                    enabledSerial = loadIdent.GetBool("HasSerialNumber");
#nullable enable        
                    MorePallets? obj = ProcessQtyWithParams(pallets, location);
#nullable disable
                    /* Adds an object to the list. */
                    if (obj is null)
                    {
                        Toast.MakeText(this, "Ne obstaja.", ToastLength.Long).Show();
                    }
                    else
                    {
                        data.Add(obj);
                    }
                }
                else
                {
                    return;
                }
            }
        }


        private void Sound()
        {
            soundPool.Play(soundPoolId, 1, 1, 0, 0, 1);
        }
    }
}