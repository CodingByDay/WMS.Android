using Android.Content;
using Android.Content.PM;
using Android.Net;
using TrendNET.WMS.Core.Data;
using TrendNET.WMS.Device.App;
using TrendNET.WMS.Device.Services;
using WMS.App;
using WMS.ExceptionStore;
using WebApp = TrendNET.WMS.Device.Services.WebApp;
namespace WMS
{
    [Activity(Label = "WMS")]
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
        private NameValueObjectList warehouses;
        private TextView labelWarehouse;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            try
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
                labelWarehouse = FindViewById<TextView>(Resource.Id.labelWarehouse);
                select.Click += Select_Click;
                confirm.Click += Confirm_Click;
                logout.Click += Logout_Click;
                cbWarehouse.ItemSelected += CbWarehouse_ItemSelected;
                dtDate.Text = DateTime.Now.ToShortDateString();

                warehouses = await CommonData.ListWarehousesAsync();
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

        private void Logout_Click(object sender, EventArgs e)
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
        private async void Confirm_Click(object sender, EventArgs e)
        {
            try
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
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }
        private void Select_Click(object sender, EventArgs e)
        {
            try
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
                long minDate = (long) (tomorrow - new DateTime(1970, 1, 1)).TotalMilliseconds;
                datePicker.MinDate = minDate;
                dialog.Show();
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
                Spinner spinner = (Spinner)sender;
                temporaryPositionWarehouse = e.Position;
                string warehouse = warehouses.Items.ElementAt(temporaryPositionWarehouse).GetString("Name");

                // Improvment in the layout. 30.12.2024 Janko Jovičić
                var splitted = labelWarehouse.Text.Split(":");
                if (splitted.Length <= 0)
                {
                    labelWarehouse.Text = labelWarehouse.Text + ": " + warehouse;
                } else
                {
                    labelWarehouse.Text = splitted[0] + ": " + warehouse;
                }

            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }
    }
}