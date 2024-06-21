using Android.Content;
using Android.Content.PM;
using Android.Net;
using Android.Views;
using TrendNET.WMS.Core.Data;
using TrendNET.WMS.Device.App;
using TrendNET.WMS.Device.Services;
using WMS.App;
using WMS.ExceptionStore;
using WMS.Printing;
namespace WMS
{
    [Activity(Label = "InventoryPrint", ScreenOrientation = ScreenOrientation.Portrait)]
    public class InventoryPrint : CustomBaseActivity
    {
        private Spinner cbWarehouse;
        private EditText tbLocation;
        private Button btPrint;
        private Button button2;
        private List<ComboBoxItem> objectsAdapter = new List<ComboBoxItem>();
        private int temporaryPositionWarehouse;

        public void GetBarcode(string barcode)
        {
            try
            {
                if (tbLocation.HasFocus)
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
                    base.SetContentView(Resource.Layout.InventoryPrintTablet);
                }
                else
                {
                    base.RequestedOrientation = ScreenOrientation.Portrait;
                    base.SetContentView(Resource.Layout.InventoryPrint);
                }
                AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
                var _customToolbar = new CustomToolbar(this, toolbar, Resource.Id.navIcon);
                _customToolbar.SetNavigationIcon(App.Settings.RootURL + "/Services/Logo");
                SetSupportActionBar(_customToolbar._toolbar);
                SupportActionBar.SetDisplayShowTitleEnabled(false);
                cbWarehouse = FindViewById<Spinner>(Resource.Id.cbWarehouse);
                tbLocation = FindViewById<EditText>(Resource.Id.tbLocation);
                btPrint = FindViewById<Button>(Resource.Id.btPrint);
                button2 = FindViewById<Button>(Resource.Id.button2);
                cbWarehouse.ItemSelected += CbWarehouse_ItemSelected;
                btPrint.Click += BtPrint_Click;
                button2.Click += Button2_Click;

                var warehouses = await CommonData.ListWarehousesAsync();
                warehouses.Items.ForEach(wh =>
                {
                    objectsAdapter.Add(new ComboBoxItem { ID = wh.GetString("Subject"), Text = wh.GetString("Name") });
                });



                var adapterWarehouse = new ArrayAdapter<ComboBoxItem>(this,
                Android.Resource.Layout.SimpleSpinnerItem, objectsAdapter);

                adapterWarehouse.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
                cbWarehouse.Adapter = adapterWarehouse;

                var _broadcastReceiver = new NetworkStatusBroadcastReceiver();
                _broadcastReceiver.ConnectionStatusChanged += OnNetworkStatusChanged;
                Application.Context.RegisterReceiver(_broadcastReceiver,
                new IntentFilter(ConnectivityManager.ConnectivityAction), ReceiverFlags.NotExported);
                tbLocation.SetBackgroundColor(Android.Graphics.Color.Aqua);
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
        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            try
            {
                switch (keyCode)
                {
                    // Setting F2 to method ProccesStock()
                    case Keycode.F2:
                        if (btPrint.Enabled == true)
                        {
                            BtPrint_Click(this, null);
                        }
                        break;

                    case Keycode.F8:
                        if (button2.Enabled == true)
                        {
                            Button2_Click(this, null);
                        }
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

        private void Button2_Click(object sender, EventArgs e)
        {   try
            {
                StartActivity(typeof(MainActivity));
                Finish();
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private async void BtPrint_Click(object sender, EventArgs e)
        {
            try
            {
                var wh = objectsAdapter.ElementAt(temporaryPositionWarehouse);
                if (wh == null)
                {
                    Toast.MakeText(this, $"{Resources.GetString(Resource.String.s245)}", ToastLength.Long).Show();
                    return;
                }
                if (!await CommonData.IsValidLocationAsync(wh.ID, tbLocation.Text.Trim(), this))
                {
                    Toast.MakeText(this, $"{Resources.GetString(Resource.String.s258)} '" + tbLocation.Text.Trim() + $"' {Resources.GetString(Resource.String.s272)} '" + wh.ID + "'!", ToastLength.Long).Show();
                    return;
                }
                try
                {
                    var nvo = new NameValueObject("PrintInventory");
                    PrintingCommon.SetNVOCommonData(ref nvo);
                    nvo.SetString("Warehouse", wh.ID);
                    nvo.SetString("Location", tbLocation.Text.Trim());
                    PrintingCommon.SendToServer(nvo);
                }
                finally
                {
                    Toast.MakeText(this, $"{Resources.GetString(Resource.String.s250)}", ToastLength.Long).Show();
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
                temporaryPositionWarehouse = e.Position;
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }
    }
}