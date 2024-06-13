using Android.Content;
using Android.Content.PM;
using Android.Net;
using Android.Views;
using TrendNET.WMS.Core.Data;
using TrendNET.WMS.Device.App;
using TrendNET.WMS.Device.Services;
using WMS.App;
namespace WMS
{
    [Activity(Label = "IssuedGoodsBusinessEventSetup", ScreenOrientation = ScreenOrientation.Portrait)]
    public class IssuedGoodsBusinessEventSetup : CustomBaseActivity, IDialogInterfaceOnClickListener
    {
        private int initial = 0;
        private CustomAutoCompleteTextView cbDocType;
        public NameValueObjectList docTypes = null;
        private CustomAutoCompleteTextView cbWarehouse;
        private CustomAutoCompleteTextView cbExtra;
        private List<ComboBoxItem> objectDocType = new List<ComboBoxItem>();
        private List<ComboBoxItem> objectWarehouse = new List<ComboBoxItem>();
        private List<ComboBoxItem> objectExtra = new List<ComboBoxItem>();
        private int temporaryPositionDoc = 0;
        private int temporaryPositionWarehouse = 0;
        private int temporaryPositionExtra = 0;
        public static bool success = false;
        public static string objectTest;
        private bool byOrder = true;
        private static string byClient = "";
        private TextView lbExtra;
        private Button btnOrder;
        private Button btnOrderMode;
        private Button btnLogout;
        private Button hidden;
        private TextView focus;
        private NameValueObjectList positions = null;

        private bool initialLoad;
        private RelativeLayout rlExtra;


        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetTheme(Resource.Style.AppTheme_NoActionBar);
            // Create your application here
            if (App.Settings.tablet)
            {
                base.RequestedOrientation = ScreenOrientation.Landscape;
                base.SetContentView(Resource.Layout.IssuedGoodsBusinessEventSetupTablet);
                rlExtra = FindViewById<RelativeLayout>(Resource.Id.rlExtra);
            }
            else
            {
                base.RequestedOrientation = ScreenOrientation.Portrait;
                base.SetContentView(Resource.Layout.IssuedGoodsBusinessEventSetup);
            }
            AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
            var _customToolbar = new CustomToolbar(this, toolbar, Resource.Id.navIcon);
            _customToolbar.SetNavigationIcon(App.Settings.RootURL + "/Services/Logo");
            SetSupportActionBar(_customToolbar._toolbar);
            SupportActionBar.SetDisplayShowTitleEnabled(false);
            cbDocType = FindViewById<CustomAutoCompleteTextView>(Resource.Id.cbDocType);
            cbWarehouse = FindViewById<CustomAutoCompleteTextView>(Resource.Id.cbWarehouse);
            cbExtra = FindViewById<CustomAutoCompleteTextView>(Resource.Id.cbExtra);
            lbExtra = FindViewById<TextView>(Resource.Id.lbExtra);
            btnOrderMode = FindViewById<Button>(Resource.Id.btnOrderMode);
            btnOrder = FindViewById<Button>(Resource.Id.btnOrder);
            btnLogout = FindViewById<Button>(Resource.Id.btnLogout);
            btnOrder.Click += BtnOrder_Click;
            btnOrderMode.Click += BtnOrderMode_Click;
            btnLogout.Click += BtnLogout_Click;
            hidden = FindViewById<Button>(Resource.Id.hidden);
            focus = FindViewById<TextView>(Resource.Id.focus);
            var warehouses = await CommonData.ListWarehousesAsync();
            warehouses.Items.ForEach(wh =>
            {
                objectWarehouse.Add(new ComboBoxItem { ID = wh.GetString("Subject"), Text = wh.GetString("Name") });
            });
            adapterWarehouse = new CustomAutoCompleteAdapter<ComboBoxItem>(this,
            Android.Resource.Layout.SimpleSpinnerItem, objectWarehouse);
            adapterWarehouse.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerItem);
            cbWarehouse.Adapter = adapterWarehouse;
            string dw = await CommonData.GetSettingAsync("DefaultWarehouse", this);
            cbWarehouse.SetText(dw, false);
            adapterDocType = new CustomAutoCompleteAdapter<ComboBoxItem>(this,
            Android.Resource.Layout.SimpleSpinnerItem, objectDocType);
            adapterDocType.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerItem);
            cbDocType.Adapter = adapterDocType;
            await UpdateForm();
            btnOrderMode.Enabled = await Services.HasPermission("TNET_WMS_BLAG_SND_NORDER", "R", this);
            cbWarehouse.Enabled = true;
            BottomSheetActions bottomSheetActions = new BottomSheetActions();
            initialLoad = true;
            var _broadcastReceiver = new NetworkStatusBroadcastReceiver();
            _broadcastReceiver.ConnectionStatusChanged += OnNetworkStatusChanged;
            Application.Context.RegisterReceiver(_broadcastReceiver,
            new IntentFilter(ConnectivityManager.ConnectivityAction));
            SelectState();
            cbDocType.ItemClick += CbDocType_ItemClick;
            cbExtra.ItemClick += CbExtra_ItemClick;
            cbWarehouse.ItemClick += CbWarehouse_ItemClick;
            await InitializeAutocompleteControls();
        }

        private async Task InitializeAutocompleteControls()
        {
            try
            {
                if (objectDocType.Count > 0)
                {
                    cbDocType.SelectAtPosition(0);
                    cbExtra.SelectAtPosition(0);
                    var dws = await Queries.DefaultIssueWarehouse(objectDocType.ElementAt(0).ID);
                    temporaryPositionWarehouse = cbWarehouse.SetItemByString(dws.warehouse);
                    if (dws.main)
                    {
                        cbWarehouse.Enabled = false;
                    }
                    await FillOpenOrdersAsync();
                }
            } catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
            }
        }

        private async void CbWarehouse_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            try
            {
                temporaryPositionWarehouse = e.Position;
                await FillOpenOrdersAsync();
            }
            catch (Exception ex)
            {
                string toast = string.Format($"{Resources.GetString(Resource.String.s265)}" + ex.ToString());
                Toast.MakeText(this, toast, ToastLength.Long).Show();
            }
        }

        private void CbExtra_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            try
            {
                temporaryPositionExtra = e.Position;
            }
            catch (Exception ex)
            {
                string toast = string.Format($"{Resources.GetString(Resource.String.s265)}" + ex.ToString());
                Toast.MakeText(this, toast, ToastLength.Long).Show();
            }
        }

        private async void CbDocType_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
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
                cbExtra.Text = string.Empty;
                await FillOpenOrdersAsync();
            }
            catch (Exception ex)
            {
                string toast = string.Format($"{Resources.GetString(Resource.String.s265)}" + ex.ToString());
                Toast.MakeText(this, toast, ToastLength.Long).Show();
            }
        }

        private void SelectState()
        {
            var index_doc = Intent.GetIntExtra("index_doc", 0);
            var index_war = Intent.GetIntExtra("index_war", 0);
            if (index_doc != 0 || index_war != 0)
            {
                cbDocType.SelectAtPosition(index_doc);
            }
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
        internal class BottomSheetActions : Java.Lang.Object, IDialogInterfaceOnClickListener
        {
            public void OnClick(IDialogInterface dialog, int which)
            {
                Console.WriteLine("Hello fox");
            }


        }
        public void OnClick(IDialogInterface dialog, int which)
        {

        }


        private void Hidden_Click(object sender, EventArgs e)
        {
            focus.RequestFocus();
        }

        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            switch (keyCode)
            {
                // Setting F2 to method ProccesStock()
                case Keycode.F3:
                    if (btnOrderMode.Enabled == true)
                    {
                        BtnOrderMode_Click(this, null);
                    }
                    break;

                case Keycode.F2:
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



        private async Task FillOpenOrdersAsync()
        {
            await Task.Run(async () =>
            {
                try
                {
 
                    int selectedFlow = Base.Store.modeIssuing;
                    if (selectedFlow == 2)
                    {
                        try
                        {
                            objectExtra.Clear();
                            var dt = objectDocType.Where(x => x.Text == cbDocType.Text).FirstOrDefault().ID;

                            if (dt != null)
                            {


                                var whAdapter = cbWarehouse.Adapter as CustomAutoCompleteAdapter<ComboBoxItem>;
                                string wh = whAdapter.GetComboBoxItem(temporaryPositionWarehouse).ID ?? cbWarehouse.Text;

                                if (wh != null && !string.IsNullOrEmpty(wh))
                                {
                                    string error;
                                    positions = Services.GetObjectList("oodtw", out error, dt + "|" + wh + "|" + byClient);

                                    if (positions == null)
                                    {
                                        RunOnUiThread(() =>
                                        {
                                            string toasted = string.Format($"{Resources.GetString(Resource.String.s216)}" + error);
                                            Toast.MakeText(this, toasted, ToastLength.Long).Show();
                                            return;

                                        });
                                    }

                                    positions.Items.ForEach(p =>
                                    {
                                        if (!string.IsNullOrEmpty(p.GetString("Key")))
                                        {
                                            objectExtra.Add(new ComboBoxItem { ID = p.GetString("Key"), Text = p.GetString("ShortKey") + " " + p.GetString("FillPercStr") + " " + p.GetString("Receiver") });
                                        }

                                    });
                                    RunOnUiThread(() =>
                                    {
                                        adapterExtra = new CustomAutoCompleteAdapter<ComboBoxItem>(this, Android.Resource.Layout.SimpleSpinnerItem, objectExtra);
                                        cbExtra.Adapter = null;
                                        cbExtra.Adapter = adapterExtra;
                                        cbExtra.Threshold = 1;
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
                }
                catch (Exception ex)
                {
                    SentrySdk.CaptureException(ex);
                }

            });
        }
        private void BtnLogout_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(MainMenu));
            Finish();
            Finish();
        }

        private async void BtnOrderMode_Click(object sender, EventArgs e)
        {
            await FillOpenOrdersAsync();
            Base.Store.byOrder = byOrder;
            byOrder = !byOrder;
            await UpdateForm();
        }

        private void BtnOrder_Click(object sender, EventArgs e)
        {
            // Fixing clicking the order without choosing an order...
            if (cbExtra.Visibility == ViewStates.Visible && cbExtra.Text == string.Empty)
            {
                Toast.MakeText(this, $"{Resources.GetString(Resource.String.s286)}", ToastLength.Long).Show();
            }
            else
            {
                NextStep();
            }
        }



        private async Task UpdateForm()
        {
            objectExtra.Clear();
            adapterDocType.Clear();
            if (byOrder)
            {
                int selectedFlow = Base.Store.modeIssuing;
                if (selectedFlow == 2)
                {
                    lbExtra.Visibility = ViewStates.Visible;
                    if (App.Settings.tablet)
                    {
                        rlExtra.Visibility = ViewStates.Visible;
                    }
                    else
                    {
                        cbExtra.Visibility = ViewStates.Visible;
                    }
                    lbExtra.Text = Resources.GetString(Resource.String.s36);
                }
                else
                {
                    lbExtra.Visibility = ViewStates.Invisible;
                    if (App.Settings.tablet)
                    {
                        rlExtra.Visibility = ViewStates.Invisible;
                    }
                    else
                    {
                        cbExtra.Visibility = ViewStates.Invisible;
                    }
                }
                if (initial > 0)
                {
                    await FillOpenOrdersAsync();
                }
                docTypes = await CommonData.ListDocTypesAsync("P|N");

                if (App.Settings.tablet)
                {
                    btnOrderMode.Text = base.Resources.GetString(Resource.String.s342);

                }
                else
                {
                    btnOrderMode.Text = base.Resources.GetString(Resource.String.s30);
                }

                initial += 1;
            }
            else
            {
                lbExtra.Visibility = ViewStates.Visible;

                if (App.Settings.tablet)
                {
                    rlExtra.Visibility = ViewStates.Visible;
                }
                else
                {
                    cbExtra.Visibility = ViewStates.Visible;
                }
                lbExtra.Text = Resources.GetString(Resource.String.s33);
                objectExtra.Clear();
                var subjects = await CommonData.ListSubjectsAsync();
                subjects.Items.ForEach(s =>
                {
                    objectExtra.Add(new ComboBoxItem { ID = s.GetString("ID"), Text = s.GetString("ID") });
                });
                adapterExtra = new CustomAutoCompleteAdapter<ComboBoxItem>(this,
                             Android.Resource.Layout.SimpleSpinnerItem, objectExtra);
                cbExtra.Adapter = null;
                cbExtra.Adapter = adapterExtra;

                adapterExtra.NotifyDataSetChanged();
                docTypes = await CommonData.ListDocTypesAsync("I;M|F");

                if (App.Settings.tablet)
                {
                    btnOrderMode.Text = base.Resources.GetString(Resource.String.s340);

                }
                else
                {
                    btnOrderMode.Text = base.Resources.GetString(Resource.String.s32);
                }
            }
            docTypes.Items.ForEach(dt =>
            {
                objectDocType.Add(new ComboBoxItem { ID = dt.GetString("Code"), Text = dt.GetString("Code") + " " + dt.GetString("Name") });
            });


            // Refresh the data
            adapterDocType.Clear();
            adapterDocType.AddAll(objectDocType);
            adapterDocType.NotifyDataSetChanged();

            if (objectDocType.Count == 0)
            {
                cbDocType.Text = string.Empty;
            }

        }


        private void NextStep()
        {
            if (temporaryPositionDoc == -1 || temporaryPositionWarehouse == -1)
            {
                return;
            }
            var itemDT = adapterDocType.GetItem(temporaryPositionDoc);
            if (itemDT == null)
            {
                string toast = string.Format($"{Resources.GetString(Resource.String.s237)}");
                Toast.MakeText(this, toast, ToastLength.Long).Show();
            }
            else
            {
                var itemWH = adapterWarehouse.GetItem(temporaryPositionWarehouse);
                if (itemWH == null)
                {
                    string toast = string.Format($"{Resources.GetString(Resource.String.s287)}");
                    Toast.MakeText(this, toast, ToastLength.Long).Show();
                }
                else
                {
                    ComboBoxItem itemSubj = null;
                    if (!byOrder)
                    {
                        itemSubj = adapterExtra.GetItem(temporaryPositionExtra);
                        if (itemSubj == null)
                        {
                            string toast = string.Format($"{Resources.GetString(Resource.String.s237)}");
                            Toast.MakeText(this, toast, ToastLength.Long).Show();
                            return;
                        }
                    }
                    NameValueObject moveHead = new NameValueObject("MoveHead");
                    moveHead.SetString("CurrentFlow", Base.Store.modeIssuing.ToString());
                    moveHead.SetString("DocumentType", itemDT.ID);
                    moveHead.SetString("Wharehouse", itemWH.ID);
                    moveHead.SetBool("ByOrder", byOrder);
                    if (!byOrder)
                    {
                        moveHead.SetString("Receiver", itemSubj.ID);
                    }
                    InUseObjects.Set("MoveHead", moveHead);
                    NameValueObject order = null;

                    {
                        string selectedFlow = Base.Store.modeIssuing.ToString();
                        if (byOrder && selectedFlow == "2")
                        {
                            itemSubj = adapterExtra.GetItem(temporaryPositionExtra);
                            if (itemSubj == null)
                            {
                                string toast = string.Format($"{Resources.GetString(Resource.String.s288)}");
                                Toast.MakeText(this, toast, ToastLength.Long).Show();
                                return;
                            }
                            order = positions.Items.First(p => p.GetString("Key") == adapterExtra.GetItem(temporaryPositionExtra).ID);
                            InUseObjects.Set("OpenOrder", order);
                        }

                        if (selectedFlow == "2" && byOrder)
                        {
                            StartActivity(typeof(IssuedGoodsIdentEntryWithTrail));
                        }
                        else
                        {
                            StartActivity(typeof(IssuedGoodsIdentEntry));
                        }
                    }
                }
            }
        }








        private string targetWord = string.Empty;
        private string currentWord = string.Empty;
        private System.Timers.Timer aTimer = new System.Timers.Timer();
        private CustomAutoCompleteAdapter<ComboBoxItem> adapterExtra;
        private CustomAutoCompleteAdapter<ComboBoxItem> adapterDocType;
        private CustomAutoCompleteAdapter<ComboBoxItem> adapterWarehouse;
    }
}