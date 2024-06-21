using Android.Content;
using Android.Content.PM;
using Android.Net;
using TrendNET.WMS.Core.Data;
using TrendNET.WMS.Device.App;
using TrendNET.WMS.Device.Services;
using WMS.App;
using WebApp = TrendNET.WMS.Device.Services.WebApp;
namespace WMS
{
    [Activity(Label = "InventoryOpenDocument", ScreenOrientation = ScreenOrientation.Portrait)]
    public class InventoryOpenDocument : CustomBaseActivity
    {
        private Spinner cbWarehouse;
        private EditText dtDate;
        private Button select;
        private Button confirm;
        private Button logout;
        private List<ComboBoxItem> warehousesAdapter = new List<ComboBoxItem>();
        private int temporaryPositionWarehouse;
        private DateTime datex;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetTheme(Resource.Style.AppTheme_NoActionBar);
            if (App.Settings.tablet)
            {
                base.RequestedOrientation = ScreenOrientation.Landscape;
                base.SetContentView(Resource.Layout.InventoryOpenDocumentTablet);
            }
            else
            {
                base.RequestedOrientation = ScreenOrientation.Portrait;
                base.SetContentView(Resource.Layout.InventoryOpenDocument);
            }
            AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
            var _customToolbar = new CustomToolbar(this, toolbar, Resource.Id.navIcon);
            _customToolbar.SetNavigationIcon(App.Settings.RootURL + "/Services/Logo");
            SetSupportActionBar(_customToolbar._toolbar);
            SupportActionBar.SetDisplayShowTitleEnabled(false);
            cbWarehouse = FindViewById<Spinner>(Resource.Id.cbWarehouse);
            dtDate = FindViewById<EditText>(Resource.Id.dtDate);
            select = FindViewById<Button>(Resource.Id.select);
            confirm = FindViewById<Button>(Resource.Id.confirm);
            logout = FindViewById<Button>(Resource.Id.logout);
            select.Click += Select_Click;
            confirm.Click += Confirm_Click;
            logout.Click += Logout_Click;
            cbWarehouse.ItemSelected += CbWarehouse_ItemSelected;
            dtDate.Text = DateTime.Now.ToShortDateString();
            var warehouses = await CommonData.ListWarehousesAsync();
            warehouses.Items.ForEach(wh =>
            {
                warehousesAdapter.Add(new ComboBoxItem { ID = wh.GetString("Subject"), Text = wh.GetString("Name") });
            });
            var adapter = new CustomAutoCompleteAdapter<ComboBoxItem>(this,
             Android.Resource.Layout.SimpleSpinnerItem, warehousesAdapter);
            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            cbWarehouse.Adapter = adapter;
            var _broadcastReceiver = new NetworkStatusBroadcastReceiver();
            _broadcastReceiver.ConnectionStatusChanged += OnNetworkStatusChanged;
            Application.Context.RegisterReceiver(_broadcastReceiver,
            new IntentFilter(ConnectivityManager.ConnectivityAction), ReceiverFlags.NotExported);
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
                    SentrySdk.CaptureException(err);
                }
            }
            else
            {
                LoaderManifest.LoaderManifestLoop(this);
            }
        }

        private void Logout_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(MainMenu));
            Finish();
        }
        private async void Confirm_Click(object sender, EventArgs e)
        {
            var warehouse = warehousesAdapter.ElementAt(temporaryPositionWarehouse);
            if (warehouse == null)
            {
                Toast.MakeText(this, $"{Resources.GetString(Resource.String.s245)}", ToastLength.Long).Show();
                return;
            }
            try
            {
                var date = datex;
                var moveHead = new NameValueObject("MoveHead");
                moveHead.SetString("Wharehouse", warehouse.ID.ToString());
                moveHead.SetDateTime("Date", date);
                moveHead.SetString("Type", "N");
                moveHead.SetString("LinkKey", "");
                moveHead.SetInt("LinkNo", 0);
                moveHead.SetInt("Clerk", Services.UserID());

                var (success, result) = await WebApp.GetAsync("mode=canInsertInventory&wh=" + warehouse.ID.ToString(), this);
                if (!success)
                {
                    Toast.MakeText(this, $"{Resources.GetString(Resource.String.s216)}" + result, ToastLength.Long).Show();
                    return;
                }
                if (result == "OK!")
                {
                    var savedMoveHead = Services.SetObject("mh", moveHead, out result);
                    if (savedMoveHead == null)
                    {
                        Toast.MakeText(this, $"{Resources.GetString(Resource.String.s216)}" + result, ToastLength.Long).Show();
                        return;
                    }
                    Toast.MakeText(this, $"{Resources.GetString(Resource.String.s284)}", ToastLength.Long).Show();
                    StartActivity(typeof(InventoryMenu));
                }
                else
                {
                    Toast.MakeText(this, $"{Resources.GetString(Resource.String.s247)}" + result, ToastLength.Long).Show();
                    return;
                }
            }
            catch (Exception error)
            {
                SentrySdk.CaptureException(error);
                return;
            }
        }
        private void Select_Click(object sender, EventArgs e)
        {
            DateTime today = DateTime.Today;
            DatePickerDialog dialog = new DatePickerDialog(this, (sender, args) =>
            {
                DateTime selectedDate = args.Date;
                if (selectedDate >= today)
                {
                    dtDate.Text = selectedDate.ToShortDateString();
                    datex = selectedDate;
                }
                else
                {
                    Toast.MakeText(this, $"{Resources.GetString(Resource.String.s249)}", ToastLength.Short).Show();
                }
            }, today.Year, today.Month - 1, today.Day);
            DatePicker datePicker = dialog.DatePicker;
            DateTime tomorrow = today.AddDays(0);
            long minDate = (long)(tomorrow - new DateTime(1970, 1, 1)).TotalMilliseconds;
            datePicker.MinDate = minDate;
            dialog.Show();
        }

        private void CbWarehouse_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            Spinner spinner = (Spinner)sender;
            temporaryPositionWarehouse = e.Position;
        }
    }
}