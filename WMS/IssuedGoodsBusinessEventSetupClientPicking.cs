using Android.Content;
using Android.Content.PM;
using Android.Net;
using Android.Views;
using TrendNET.WMS.Core.Data;
using TrendNET.WMS.Device.App;
using TrendNET.WMS.Device.Services;
using WMS.App;
using WMS.ExceptionStore;
using AlertDialog = Android.App.AlertDialog;
namespace WMS
{
    [Activity(Label = "WMS")]
    public class IssuedGoodsBusinessEventSetupClientPicking : CustomBaseActivity
    {
        private int initial = 0;
        private CustomAutoCompleteTextView cbDocType;
        public NameValueObjectList docTypes = null;
        private CustomAutoCompleteTextView cbWarehouse;
        private CustomAutoCompleteTextView cbExtra;
        private List<ComboBoxItem> objectDocType = new List<ComboBoxItem>();
        private List<ComboBoxItem> objectWarehouse = new List<ComboBoxItem>();
        private List<string> objectExtra = new List<string>();
        private int temporaryPositionDoc = 0;
        private int temporaryPositionWarehouse = 0;
        private int temporaryPositionExtra = 0;
        public static bool success = false;
        public static string objectTest;

        private static string byClient = "";
        private TextView lbExtra;
        private Button btnOrder;
        private Button btnOrderMode;
        private Button btnLogout;
        private Button hidden;
        private TextView focus;
        private NameValueObjectList positions = null;
        private Button btnHidden;
        private bool initialLoad;

        protected async override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                SetTheme(Resource.Style.AppTheme_NoActionBar);
                // Create your application here
                if (App.Settings.tablet)
                {
                    base.RequestedOrientation = ScreenOrientation.Landscape;
                    base.SetContentView(Resource.Layout.IssuedGoodsBusinessEventSetupClientPickingTablet);
                }
                else
                {
                    base.RequestedOrientation = ScreenOrientation.Portrait;
                    base.SetContentView(Resource.Layout.IssuedGoodsBusinessEventSetupClientPicking);
                }

                AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
                var _customToolbar = new CustomToolbar(this, toolbar, Resource.Id.navIcon);
                _customToolbar.SetNavigationIcon(App.Settings.RootURL + "/Services/Logo");
                SetSupportActionBar(_customToolbar._toolbar);
                SupportActionBar.SetDisplayShowTitleEnabled(false);
                cbDocType = FindViewById<CustomAutoCompleteTextView>(Resource.Id.cbDocType);
                cbWarehouse = FindViewById<CustomAutoCompleteTextView>(Resource.Id.cbWarehouse);
                cbExtra = FindViewById<CustomAutoCompleteTextView>(Resource.Id.cbExtra);
                btnOrder = FindViewById<Button>(Resource.Id.btnOrder);
                btnLogout = FindViewById<Button>(Resource.Id.btnLogout);
                btnHidden = FindViewById<Button>(Resource.Id.btnHidden);

                btnLogout.Click += BtnLogout_Click;
                btnOrder.Click += BtnOrder_Click;


                var warehouses = await AsyncServices.AsyncServices.GetObjectListBySqlAsync($"SELECT acWarehouse, acName FROM uWMSWarehouse", null, this);

                if (warehouses.Success)
                {
                    foreach (var war in warehouses.Rows)
                    {
                        objectWarehouse.Add(new ComboBoxItem { ID = war.StringValue("acWarehouse"), Text = war.StringValue("acWarehouse") });
                    }
                }
                else if (!warehouses.Success)
                {
                    AlertDialog.Builder alert = new AlertDialog.Builder(this);
                    alert.SetTitle($"{Resources.GetString(Resource.String.s265)}");
                    alert.SetMessage($"{warehouses.Error}");
                    alert.SetPositiveButton("Ok", (senderAlert, args) =>
                    {
                        alert.Dispose();
                    });
                    Dialog dialog = alert.Create();
                    dialog.Show();
                }

                adapterWarehouse = new CustomAutoCompleteAdapter<ComboBoxItem>(this,
                Android.Resource.Layout.SimpleSpinnerItem, objectWarehouse);
                adapterWarehouse.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerItem);
                cbWarehouse.Adapter = adapterWarehouse;
                await UpdateForm();
                adapterDocType = new CustomAutoCompleteAdapter<ComboBoxItem>(this,
                Android.Resource.Layout.SimpleSpinnerItem, objectDocType);
                adapterDocType.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerItem);
                cbDocType.Adapter = adapterDocType;
                cbWarehouse.Enabled = true;
                initialLoad = true;
                var _broadcastReceiver = new NetworkStatusBroadcastReceiver();
                _broadcastReceiver.ConnectionStatusChanged += OnNetworkStatusChanged;
                Application.Context.RegisterReceiver(_broadcastReceiver,
                new IntentFilter(ConnectivityManager.ConnectivityAction), ReceiverFlags.NotExported);
                cbDocType.ItemClick += CbDocType_ItemClick;
                cbExtra.ItemClick += CbExtra_ItemClick;
                cbWarehouse.ItemClick += CbWarehouse_ItemClick;
                await InitializeAutocompleteControls();
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private async Task InitializeAutocompleteControls()
        {
            try
            {
                cbDocType.SelectAtPosition(0);
                cbExtra.SelectAtPosition(0);

                var dws = await Queries.DefaultIssueWarehouse(adapterDocType.GetItem(0).ID);

                temporaryPositionWarehouse = cbWarehouse.SetItemByString(dws.warehouse);
                if (dws.main)
                {
                    cbWarehouse.Enabled = false;
                }
                await FillOpenOrdersAsync();
                currentWarehouse = cbWarehouse.Text;
                currentClient = cbExtra.Text;
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }


        private async Task FillOpenOrdersAsync()
        {
            try
            {
                await Task.Run(async () =>
                {
                    try
                    {

                        try
                        {
                            objectExtra.Clear();
                            var dt = adapterDocType.GetItem(temporaryPositionDoc);
                            if (dt != null)
                            {
                                var wh = adapterWarehouse.GetItem(temporaryPositionWarehouse);
                                if (wh != null)
                                {
                                    string error;
                                    var parameters = new List<Services.Parameter>();

                                    parameters.Add(new Services.Parameter { Name = "acDocType", Type = "String", Value = dt.ID });
                                    parameters.Add(new Services.Parameter { Name = "acWarehouse", Type = "String", Value = wh.ID });

                                    var subjects = await AsyncServices.AsyncServices.GetObjectListBySqlAsync($"SELECT * FROM uWMSOrderSubjectByTypeWarehouseOut WHERE acDocType = @acDocType AND acWarehouse = @acWarehouse", parameters, this);


                                    if (!subjects.Success)
                                    {
                                        RunOnUiThread(() =>
                                        {
                                            AlertDialog.Builder alert = new AlertDialog.Builder(this);
                                            alert.SetTitle($"{Resources.GetString(Resource.String.s265)}");
                                            alert.SetMessage($"{subjects.Error}");
                                            alert.SetPositiveButton("Ok", (senderAlert, args) =>
                                            {
                                                alert.Dispose();
                                            });
                                            Dialog dialog = alert.Create();
                                            dialog.Show();
                                            SentrySdk.CaptureMessage(subjects.Error);
                                            return;
                                        });
                                    }
                                    foreach (var subject in subjects.Rows)
                                    {
                                        if (!string.IsNullOrEmpty(subject.StringValue("acSubject")))
                                        {
                                            objectExtra.Add(subject.StringValue("acSubject"));
                                        }
                                    }
                                    RunOnUiThread(() =>
                                    {
                                        cbExtra.Text = string.Empty;
                                        objectExtra = objectExtra.Distinct().ToList();
                                        adapterExtra = new CustomAutoCompleteAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, objectExtra);
                                        cbExtra.Adapter = null;
                                        cbExtra.Adapter = adapterExtra;
                                        adapterExtra.NotifyDataSetChanged();
                                    });
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            SentrySdk.CaptureException(ex);
                        }
                    }
                    catch (Exception err)
                    {
                        SentrySdk.CaptureException(err);
                    }

                });
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }


        private async void CbWarehouse_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            try
            {
                try
                {
                    temporaryPositionWarehouse = e.Position;
                    var wh = adapterWarehouse.GetItem(temporaryPositionWarehouse);
                    currentWarehouse = wh.ID;
                    await FillOpenOrdersAsync();
                }
                catch (Exception ex)
                {
                    SentrySdk.CaptureException(ex);
                    return;
                }
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private void CbExtra_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            try
            {
                temporaryPositionExtra = e.Position;
                currentClient = adapterExtra.GetItem(e.Position);
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private async void CbDocType_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            try
            {
                try
                {
                    temporaryPositionDoc = e.Position;
                    var dws = await Queries.DefaultIssueWarehouse(adapterDocType.GetItem(temporaryPositionDoc).ID);
                    temporaryPositionWarehouse = cbWarehouse.SetItemByString(dws.warehouse);
                    if (dws.main)
                    {
                        cbWarehouse.Enabled = false;
                    }
                    await FillOpenOrdersAsync();
                }
                catch (Exception ex)
                {
                    string toast = string.Format($"{Resources.GetString(Resource.String.s265)}" + ex.ToString());
                    Toast.MakeText(this, toast, ToastLength.Long).Show();
                }
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

                    case Keycode.F3:
                        if (btnOrder.Enabled == true)
                        {
                            BtnOrder_Click(this, null);
                        }
                        break;

                    case Keycode.F8://
                        if (btnLogout.Enabled == true)
                        {
                            BtnLogout_Click(this, null);
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




        private void BtnLogout_Click(object sender, EventArgs e)
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


        private void BtnOrder_Click(object sender, EventArgs e)
        {
            try
            {
                NextStep();
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private void NextStep()
        {
            try
            {
                if (String.IsNullOrEmpty(currentClient))
                {
                    Toast.MakeText(this, $"{Resources.GetString(Resource.String.s288)}", ToastLength.Long).Show();
                    return;
                }
                NameValueObject moveHead = new NameValueObject("MoveHead");
                moveHead.SetString("CurrentFlow", "3");
                moveHead.SetString("DocumentType", adapterDocType.GetItem(temporaryPositionDoc).ID);
                moveHead.SetString("Wharehouse", currentWarehouse);
                moveHead.SetString("Receiver", currentClient);
                InUseObjects.Set("MoveHead", moveHead);
                StartActivity(typeof(ClientPickingWithTrail));
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }





        private async Task UpdateForm()
        {
            try
            {
                objectExtra.Clear();
                docTypes = await CommonData.ListDocTypesAsync("P|N");
                initial += 1;
                var result = await AsyncServices.AsyncServices.GetObjectListBySqlAsync("SELECT acDocType, acName FROM uWMSOrderDocTypeOut;");
                if (!result.Success)
                {
                    RunOnUiThread(() =>
                    {
                        AlertDialog.Builder alert = new AlertDialog.Builder(this);
                        alert.SetTitle($"{Resources.GetString(Resource.String.s265)}");
                        alert.SetMessage($"{result.Error}");
                        alert.SetPositiveButton("Ok", (senderAlert, args) =>
                        {
                            alert.Dispose();
                        });
                        Dialog dialog = alert.Create();
                        dialog.Show();
                    });
                }
                else
                {
                    foreach (Row row in result.Rows)
                    {
                        objectDocType.Add(new ComboBoxItem { ID = row.StringValue("acDocType"), Text = row.StringValue("acDocType") + " - " + row.StringValue("acName") });
                    }
                }
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private string targetWord = string.Empty;
        private string currentWord = string.Empty;
        private System.Timers.Timer aTimer = new System.Timers.Timer();
        private CustomAutoCompleteAdapter<string> adapterExtra;
        private string currentClient;
        private string currentDocType;
        private string currentWarehouse;
        private CustomAutoCompleteAdapter<ComboBoxItem> adapterDocType;
        private CustomAutoCompleteAdapter<ComboBoxItem> adapterWarehouse;
    }
}