using Android.Content;
using Android.Content.PM;
using Android.Media;
using Android.Net;
using Android.Views;
using BarCode2D_Receiver;
using TrendNET.WMS.Core.Data;
using TrendNET.WMS.Device.App;
using TrendNET.WMS.Device.Services;
using WMS.App;
using WMS.ExceptionStore;
using AlertDialog = Android.App.AlertDialog;
using WebApp = TrendNET.WMS.Device.Services.WebApp;
namespace WMS
{
    [Activity(Label = "WMS")]
    public class RapidTakeover : CustomBaseActivity, IBarcodeResult
    {
        private EditText tbSSCC;
        private Spinner cbWarehouses;
        private EditText tbLocation;
        private Button btConfirm;
        private Button btLogout;
        private EditText tbIdent;
        private List<ComboBoxItem> data = new List<ComboBoxItem>();
        private EditText tbReceiveLocation;
        private EditText tbRealStock;
        public static NameValueObject dataItem;
        private NameValueObject moveHead = (NameValueObject)InUseObjects.Get("MoveHead");
        private NameValueObject moveItem = (NameValueObject)InUseObjects.Get("MoveItem");
        private NameValueObject moveItemFinal;
        private ListView listData;
        private int selected;
        private int tempLocation;
        SoundPool soundPool;
        int soundPoolId;
        private Barcode2D barcode2D;
        private List<TakeOverSerialOrSSCCEntryList> dataX = new List<TakeOverSerialOrSSCCEntryList>();

        public void GetBarcode(string barcode)
        {
            try
            {
                if (tbSSCC.HasFocus)
                {

                    tbSSCC.Text = barcode;
                    ProcessSSCC();
                }
                else if (tbLocation.HasFocus)
                {
                    tbLocation.Text = barcode;
                }
                else if (tbIdent.HasFocus)
                {

                    tbIdent.Text = barcode;
                }
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }




        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            try
            {
                switch (keyCode)
                {
                    // In smartphone.  
                    case Keycode.F2:
                        BtConfirm_Click(this, null);
                        break;
                    // Return true;

                    case Keycode.F8:
                        BtLogout_Click(this, null);
                        break;
                }
                return base.OnKeyDown(keyCode, e);
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
                return false;
            }
        }


        private void ListData_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            try
            {
                selected = e.Position;
                var item = dataX.ElementAt(selected);
                tbLocation.Text = item.Location;
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private void fillItems()
        {
            try
            {
                string error;
                var stock = Services.GetObjectList("str", out error, data.ElementAt(tempLocation).ID + "||" + tbIdent.Text); /* Defined at the beggining of the activity. */
                var number = stock.Items.Count();


                if (stock != null)
                {
                    stock.Items.ForEach(x =>
                    {
                        dataX.Add(new TakeOverSerialOrSSCCEntryList // Reusing this type
                        {
                            Ident = x.GetString("Ident"),
                            Location = x.GetString("Location"),
                            Qty = x.GetDouble("RealStock").ToString(),
                            SerialNumber = x.GetString("SerialNo")

                        });
                    });

                }
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }
        protected override async void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                SetTheme(Resource.Style.AppTheme_NoActionBar);
                if (App.Settings.tablet)
                {
                    base.RequestedOrientation = ScreenOrientation.Landscape;
                    base.SetContentView(Resource.Layout.RapidTakeover);
                }
                else
                {
                    base.RequestedOrientation = ScreenOrientation.Portrait;
                    base.SetContentView(Resource.Layout.RapidTakeoverPhone);
                }
                AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
                var _customToolbar = new CustomToolbar(this, toolbar, Resource.Id.navIcon);
                _customToolbar.SetNavigationIcon(App.Settings.RootURL + "/Services/Logo");
                SetSupportActionBar(_customToolbar._toolbar);
                SupportActionBar.SetDisplayShowTitleEnabled(false);
                barcode2D = new Barcode2D(this, this);
                tbIdent = FindViewById<EditText>(Resource.Id.tbIdent);
                // Create your application here
                tbSSCC = FindViewById<EditText>(Resource.Id.tbSSCC);
                cbWarehouses = FindViewById<Spinner>(Resource.Id.cbWarehouses);
                tbLocation = FindViewById<EditText>(Resource.Id.tbLocation);
                btConfirm = FindViewById<Button>(Resource.Id.btConfirm);
                btLogout = FindViewById<Button>(Resource.Id.btLogout);
                tbReceiveLocation = FindViewById<EditText>(Resource.Id.tbReceiveLocation);
                tbRealStock = FindViewById<EditText>(Resource.Id.tbRealStock);
                btConfirm.Click += BtConfirm_Click;
                tbLocation.FocusChange += TbLocation_FocusChange;
                btLogout.Click += BtLogout_Click;
                listData = FindViewById<ListView>(Resource.Id.listData);
                color();


                var whs = await CommonData.ListWarehousesAsync();

                whs.Items.ForEach(wh =>
                {
                    data.Add(new ComboBoxItem { ID = wh.GetString("Subject"), Text = wh.GetString("Name") });
                });

                var adapterWarehouse = new ArrayAdapter<ComboBoxItem>(this,
                Android.Resource.Layout.SimpleSpinnerItem, data);
                adapterWarehouse.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
                cbWarehouses.Adapter = adapterWarehouse;
                cbWarehouses.ItemSelected += CbWarehouses_ItemSelected;
                tbIdent.RequestFocus();
                var _broadcastReceiver = new NetworkStatusBroadcastReceiver();
                _broadcastReceiver.ConnectionStatusChanged += OnNetworkStatusChanged;
                Application.Context.RegisterReceiver(_broadcastReceiver,
                new IntentFilter(ConnectivityManager.ConnectivityAction), ReceiverFlags.NotExported);
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }
        public bool IsOnline()
        {
            try
            {
                var cm = (ConnectivityManager)GetSystemService(ConnectivityService);
                return cm.ActiveNetworkInfo == null ? false : cm.ActiveNetworkInfo.IsConnected;
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
                return false;
            }
        }


        private void OnNetworkStatusChanged(object sender, EventArgs e)
        {
            try
            {
                if (IsOnline())
                {

                    try
                    {
                        LoaderManifest.LoaderManifestLoopStop(this);
                    }
                    catch (Exception err)
                    {
                        SentrySdk.CaptureException(err);
                    }
                }
                else
                {
                    LoaderManifest.LoaderManifestLoop(this);
                }
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private void BtLogout_Click(object sender, EventArgs e)
        {
            try
            {
                StartActivity(typeof(MainMenu));
                Finish();
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private async void BtConfirm_Click(object sender, EventArgs e)
        {
            try
            {
                if (await saveHead())
                {
                    try
                    {
                        var headID = moveItem.GetInt("HeadID");

                        var (success, result) = await WebApp.GetAsync("mode=finish&stock=add&print=" + Services.DeviceUser() + "&id=" + headID.ToString(), this);

                        if (success)
                        {
                            if (result.StartsWith("OK!"))
                            {
                                var id = result.Split('+')[1];
                                Toast.MakeText(this, $"{Resources.GetString(Resource.String.s264)}" + id, ToastLength.Long).Show();
                                AlertDialog.Builder alert = new AlertDialog.Builder(this);
                                alert.SetTitle($"{Resources.GetString(Resource.String.s263)}");
                                alert.SetMessage($"{Resources.GetString(Resource.String.s264)}" + id);

                                alert.SetPositiveButton("Ok", (senderAlert, args) =>
                                {
                                    alert.Dispose();
                                });



                                Dialog dialog = alert.Create();
                                dialog.Show();

                            }
                            else
                            {
                                Toast.MakeText(this, $"{Resources.GetString(Resource.String.s266)}" + result, ToastLength.Long).Show();

                            }
                        }
                        else
                        {
                            Toast.MakeText(this, $"{Resources.GetString(Resource.String.s218)}" + result, ToastLength.Long).Show();

                        }
                    }
                    catch (Exception err)
                    {

                        SentrySdk.CaptureException(err);
                        return;

                    }

                }
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }


        private async Task<bool> saveHead()
        {
            try
            {
                var warehouse = data.ElementAt(tempLocation);
                if (!await CommonData.IsValidLocationAsync(warehouse.ID, tbLocation.Text.Trim(), this))
                {
                    string WebError = string.Format($"{Resources.GetString(Resource.String.s271)}" + tbLocation.Text.Trim() + "ni veljavna za sladišče:" + " " + data.ElementAt(tempLocation).Text + "!");
                    Toast.MakeText(this, WebError, ToastLength.Long).Show();
                    tbLocation.RequestFocus();
                    return false;
                }
                else
                {

                    {
                        string ssscError;
                        var data = Services.GetObject("sscc", tbSSCC.Text, out ssscError);
                        string error;


                        if (moveItem == null) { moveItem = new NameValueObject("MoveItem"); }


                        moveItem.SetInt("HeadID", data.GetInt("HeadID"));
                        moveItem.SetString("LinkKey", data.GetString("LinkKey"));
                        moveItem.SetInt("LinkNo", data.GetInt("LinkNo"));
                        moveItem.SetString("Ident", data.GetString("Ident"));
                        moveItem.SetString("SSCC", tbSSCC.Text.Trim());
                        moveItem.SetString("SerialNo", data.GetString("SerialNo"));
                        moveItem.SetDouble("Packing", data.GetDouble("Packing"));
                        moveItem.SetDouble("Factor", data.GetDouble("Factor"));
                        moveItem.SetDouble("Qty", data.GetDouble("Qty"));
                        moveItem.SetString("Location", tbLocation.Text.Trim());
                        moveItem.SetInt("Clerk", data.GetInt("Clerk"));

                        string error2;
                        moveItemFinal = Services.SetObject("mi", moveItem, out error2); /* Save move item method */

                        Toast.MakeText(this, moveItemFinal.ToString(), ToastLength.Long).Show();

                        InUseObjects.Invalidate("MoveItem");
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
                return false;
            }
        }

        private void TbLocation_FocusChange(object sender, View.FocusChangeEventArgs e)
        {
            try
            {
                ProcessSSCC();
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private void CbWarehouses_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            try
            {
                tempLocation = e.Position;
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private void ProcessSSCC()
        {
            try
            {
                dataX.Clear();

                var sscc = tbSSCC.Text.Trim();
                if (string.IsNullOrEmpty(sscc)) { return; }

                try
                {


                    string error;
                    dataItem = Services.GetObject("sscc", tbSSCC.Text, out error);
                    if (dataItem == null)
                    {
                        Toast.MakeText(this, $"{Resources.GetString(Resource.String.s216)}" + error, ToastLength.Long).Show();

                        tbSSCC.Text = "";

                    }
                    else
                    {
                        tbIdent.Text = dataItem.GetString("Ident");
                        tbReceiveLocation.Text = dataItem.GetString("Location");
                        tbRealStock.Text = dataItem.GetDouble("RealStock").ToString();
                        colorLocation();
                        fillItems();

                    }

                }
                catch (Exception err)
                {

                    SentrySdk.CaptureException(err);
                    return;

                }
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }


        private void colorLocation()
        {
            try
            {
                tbLocation.SetBackgroundColor(Android.Graphics.Color.Aqua);
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private void color()
        {
            try
            {
                tbSSCC.SetBackgroundColor(Android.Graphics.Color.Aqua);
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }
    }
}
