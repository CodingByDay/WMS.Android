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
namespace WMS
{
    [Activity(Label = "WMS")]
    public class PackagingSetContext : CustomBaseActivity, IBarcodeResult
    {
        private CustomAutoCompleteTextView cbWarehouse;
        private EditText tbLocation;
        private EditText tbSSCC;
        private Button btConfirm;
        private Button btExit;
        private int temporaryPositionWarehouse;
        List<ComboBoxItem> objectsPackaging = new List<ComboBoxItem>();
        SoundPool soundPool;
        int soundPoolId;
        private Barcode2D barcode2D;
        private string temporaryString;
        private string toast;

        public void GetBarcode(string barcode)
        {
            try
            {
                // implements the interface.
                if (tbSSCC.HasFocus)
                {

                    tbSSCC.Text = barcode;
                }
                else if (tbLocation.HasFocus)
                {

                    tbLocation.Text = barcode;
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
                    base.SetContentView(Resource.Layout.PackagingSetContextTablet);
                }
                else
                {
                    base.RequestedOrientation = ScreenOrientation.Portrait;
                    base.SetContentView(Resource.Layout.PackagingSetContext);
                }
                AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
                var _customToolbar = new CustomToolbar(this, toolbar, Resource.Id.navIcon);
                _customToolbar.SetNavigationIcon(App.Settings.RootURL + "/Services/Logo");
                SetSupportActionBar(_customToolbar._toolbar);
                SupportActionBar.SetDisplayShowTitleEnabled(false);
                cbWarehouse = FindViewById<CustomAutoCompleteTextView>(Resource.Id.cbWarehouse);
                tbLocation = FindViewById<EditText>(Resource.Id.tbLocation);
                tbSSCC = FindViewById<EditText>(Resource.Id.tbSSCC);
                btConfirm = FindViewById<Button>(Resource.Id.btConfirm);
                btExit = FindViewById<Button>(Resource.Id.btExit);
                btConfirm.Click += BtConfirm_Click;
                btExit.Click += BtExit_Click;
                cbWarehouse.ItemSelected += CbWarehouse_ItemSelected;

                barcode2D = new Barcode2D(this, this);

                Color();


                var whs = await CommonData.ListWarehousesAsync();


                whs.Items.ForEach(wh =>
                {
                    objectsPackaging.Add(new ComboBoxItem { ID = wh.GetString("Subject"), Text = wh.GetString("Name") });
                });
                var adapterWarehouse = new CustomAutoCompleteAdapter<ComboBoxItem>(this,
                Android.Resource.Layout.SimpleSpinnerItem, objectsPackaging);

                adapterWarehouse.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
                cbWarehouse.Adapter = adapterWarehouse;


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

        private void Color()
        {
            try
            {
                tbLocation.SetBackgroundColor(Android.Graphics.Color.Aqua);
                tbSSCC.SetBackgroundColor(Android.Graphics.Color.Aqua);
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }


        private void BtExit_Click(object sender, EventArgs e)
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
                await ProcessData();
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private async Task ProcessData()
        {
            try
            {
                temporaryString = objectsPackaging.ElementAt(temporaryPositionWarehouse).ID;

                if (!await CommonData.IsValidLocationAsync(temporaryString, tbLocation.Text.Trim(), this))
                {
                    string toast = string.Format($"{Resources.GetString(Resource.String.s270)}");
                    Toast.MakeText(this, toast, ToastLength.Long).Show();
                    return;
                }

                if (string.IsNullOrEmpty(tbSSCC.Text.Trim()))
                {
                    string toast = string.Format($"{Resources.GetString(Resource.String.s270)}");
                    Toast.MakeText(this, toast, ToastLength.Long).Show();
                    return;
                }

                var head = new NameValueObject("PackagingHead");
                head.SetInt("HeadID", 0);
                head.SetString("Warehouse", objectsPackaging.ElementAt(temporaryPositionWarehouse).ID);
                head.SetString("ReceivingLocation", tbLocation.Text.Trim());
                head.SetString("SSCC", tbSSCC.Text.Trim());
                head.SetInt("Clerk", Services.UserID());

                string error;
                head = Services.SetObject("ph", head, out error);
                if (head != null)
                {
                    InUseObjects.Set("PackagingHead", head);
                    StartActivity(typeof(PackagingUnit));
                    Finish();
                }
                else
                {
                    string toast = string.Format($"{Resources.GetString(Resource.String.s213)}");
                    Toast.MakeText(this, toast, ToastLength.Long).Show();
                    return;
                }
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private void CbWarehouse_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            try
            {
                CustomAutoCompleteTextView spinner = (CustomAutoCompleteTextView)sender;
                if (e.Position != 0)
                {
                    temporaryPositionWarehouse = e.Position;
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
                    // in smartphone
                    case Keycode.F2:
                        if (btConfirm.Enabled == true)
                        {
                            BtConfirm_Click(this, null);
                        }
                        break;

                    case Keycode.F8:
                        BtExit_Click(this, null);
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


    }
}