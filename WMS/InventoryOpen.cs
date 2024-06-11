using Android.Content;
using Android.Content.PM;
using Android.Net;
using Android.Views;
using TrendNET.WMS.Core.Data;
using TrendNET.WMS.Device.App;
using TrendNET.WMS.Device.Services;
using WMS.App;
using WebApp = TrendNET.WMS.Device.Services.WebApp;
namespace WMS
{
    [Activity(Label = "InventoryOpen", ScreenOrientation = ScreenOrientation.Portrait)]
    public class InventoryOpen : CustomBaseActivity
    {
        private Spinner cbWarehouse;
        private EditText dtInventory;
        private Button btChoose;
        private EditText tbItems;
        private EditText tbConfirmedBy;
        private EditText tbConfirmationDate;
        private List<ComboBoxItem> warehousesObjectsAdapter = new List<ComboBoxItem>();
        private Button btOpen;
        private Button button2;
        private int temporaryPositionWarehouse;
        private NameValueObject moveHead = null;
        private string lastError = null;
        private DateTime dateX;
        private TextView warehouseLabel;
        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetTheme(Resource.Style.AppTheme_NoActionBar);
            if (App.Settings.tablet)
            {
                base.RequestedOrientation = ScreenOrientation.Landscape;
                base.SetContentView(Resource.Layout.InventoryOpenTablet);
            }
            else
            {
                base.RequestedOrientation = ScreenOrientation.Portrait;
                base.SetContentView(Resource.Layout.InventoryOpen);
            }
            AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
            var _customToolbar = new CustomToolbar(this, toolbar, Resource.Id.navIcon);
            _customToolbar.SetNavigationIcon(App.Settings.RootURL + "/Services/Logo");
            SetSupportActionBar(_customToolbar._toolbar);
            SupportActionBar.SetDisplayShowTitleEnabled(false);
            cbWarehouse = FindViewById<Spinner>(Resource.Id.cbWarehouse);
            btChoose = FindViewById<Button>(Resource.Id.btChoose);
            tbItems = FindViewById<EditText>(Resource.Id.tbItems);
            tbConfirmedBy = FindViewById<EditText>(Resource.Id.tbConfirmedBy);
            tbConfirmationDate = FindViewById<EditText>(Resource.Id.tbConfirmationDate);
            dtInventory = FindViewById<EditText>(Resource.Id.dtInventory);
            btOpen = FindViewById<Button>(Resource.Id.btOpen);
            button2 = FindViewById<Button>(Resource.Id.button2);
            button2.Click += Button2_Click;
            cbWarehouse.ItemSelected += CbWarehouse_ItemSelected;
            btChoose.Click += BtChoose_Click;
            btOpen.Click += BtOpen_Click;
            dtInventory.Text = DateTime.Today.ToShortDateString();
            warehouseLabel = FindViewById<TextView>(Resource.Id.warehouseLabel);

            var warehouses = await CommonData.ListWarehousesAsync();
            if (warehouses != null)
            {
                warehouses.Items.ForEach(wh =>
                {
                    warehousesObjectsAdapter.Add(new ComboBoxItem { ID = wh.GetString("Subject"), Text = wh.GetString("Name") });
                });
            }
            var adapterWarehouse = new CustomAutoCompleteAdapter<ComboBoxItem>(this,
             Android.Resource.Layout.SimpleSpinnerItem, warehousesObjectsAdapter);
            adapterWarehouse.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            cbWarehouse.Adapter = adapterWarehouse;
            var _broadcastReceiver = new NetworkStatusBroadcastReceiver();
            _broadcastReceiver.ConnectionStatusChanged += OnNetworkStatusChanged;
            Application.Context.RegisterReceiver(_broadcastReceiver,
            new IntentFilter(ConnectivityManager.ConnectivityAction));



            dateX = DateTime.Today;
            UpdateFields();
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(MainMenu));
            Finish();
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
        private async void UpdateFields()
        {
            var warehouse = warehousesObjectsAdapter.ElementAt(temporaryPositionWarehouse);
            if (warehouse == null)
            {
                ClearFields();
                return;
            }
            else
            {
                var date = dateX;


                try
                {

                    var (success, result) = await WebApp.GetAsync("mode=getConfirmedInventoryHead&wh=" + warehouse + "&date=" + date.ToString("s"), this);
                    if (success)
                    {
                        var moveHeadID = Convert.ToInt32(result);
                        if (moveHeadID < 0)
                        {
                            ClearFieldsError($"{Resources.GetString(Resource.String.s282)}");
                            return;
                        }
                        else
                        {
                            moveHead = Services.GetObject("mh", moveHeadID.ToString(), out result);
                            if (moveHead == null)
                            {
                                ClearFieldsError($"{Resources.GetString(Resource.String.s282)}");
                                return;
                            }
                            else
                            {
                                tbItems.Text = moveHead.GetInt("ItemCount").ToString();
                                tbConfirmedBy.Text = "???";
                                tbConfirmationDate.Text = "???";
                                btOpen.Enabled = true;
                                lastError = null;
                            }
                        }
                    }
                    else
                    {
                        ClearFieldsError($"{Resources.GetString(Resource.String.s247)}" + result);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    SentrySdk.CaptureException(ex);
                }
            }
        }

        private void ClearFields()
        {
            tbItems.Text = "";
            tbConfirmedBy.Text = "";
            tbConfirmationDate.Text = "";
            btOpen.Enabled = false;
        }
        private void ClearFieldsError(string err)
        {
            ClearFields();
            Toast.MakeText(this, err, ToastLength.Long).Show();
        }

        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            switch (keyCode)
            {

                case Keycode.F3:
                    if (btOpen.Enabled == true)
                    {
                        BtOpen_Click(this, null);
                    }
                    break;

                case Keycode.F8:
                    if (button2.Enabled == true)
                    {
                        StartActivity(typeof(MainMenu));
                        Finish();
                    }
                    break;
            }
            return base.OnKeyDown(keyCode, e);
        }
        private void BtLogout_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(MainMenu));
            Finish();
        }

        private async void BtOpen_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(lastError))
            {
                Toast.MakeText(this, lastError, ToastLength.Long).Show();

                return;
            }

            var warehouse = warehousesObjectsAdapter.ElementAt(temporaryPositionWarehouse);
            if (warehouse == null)
            {
                Toast.MakeText(this, $"{Resources.GetString(Resource.String.s245)}", ToastLength.Long).Show();

                return;
            }


            try
            {


                var date = dateX;
                var (success, result) = await WebApp.GetAsync("mode=canInsertInventory&wh=" + warehouse.ID.ToString(), this);
                if (!success)
                {
                    DialogHelper.ShowDialogError(this, this, $"{Resources.GetString(Resource.String.s247)}" + result);

                    return;
                }
                if (result == "OK!")
                {
                    (success, result) = await WebApp.GetAsync("mode=reopenInventory&id=" + moveHead.GetInt("HeadID").ToString(), this);
                    if (!success)
                    {
                        DialogHelper.ShowDialogError(this, this, $"{Resources.GetString(Resource.String.s247)}" + result);

                        return;
                    }
                    else
                    {
                        Toast.MakeText(this, $"{Resources.GetString(Resource.String.s283)}", ToastLength.Long).Show();

                        StartActivity(typeof(InventoryConfirm));
                        Finish();
                    }
                }
                else
                {
                    DialogHelper.ShowDialogError(this, this, $"{Resources.GetString(Resource.String.s247)}" + result);
                    return;
                }
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                return;
            }
        }

        private void CbWarehouse_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {

            temporaryPositionWarehouse = e.Position;
            warehouseLabel.Text = $"{Resources.GetString(Resource.String.s28)}: " + warehousesObjectsAdapter.ElementAt(temporaryPositionWarehouse);
            UpdateFields();

        }



        private void BtChoose_Click(object sender, EventArgs e)
        {
            DatePickerFragment frag = DatePickerFragment.NewInstance(delegate (DateTime time)
            {
                dtInventory.Text = time.ToShortDateString();
                dateX = time;
                UpdateFields();
            });
            frag.Show(FragmentManager, DatePickerFragment.TAG);
        }
    }
}