using Android.Content;
using Android.Content.PM;
using Android.Net;
using Android.Views;
using TrendNET.WMS.Core.Data;
using TrendNET.WMS.Device.App;
using TrendNET.WMS.Device.Services;
using WMS.App;
using WMS.ExceptionStore;
namespace WMS
{
    [Activity(Label = "TakeOverBusinessEventSetup", ScreenOrientation = ScreenOrientation.Portrait)]
    public class TakeOverBusinessEventSetup : CustomBaseActivity
    {
        private CustomAutoCompleteTextView cbDocType;
        private CustomAutoCompleteTextView cbWarehouse;
        private CustomAutoCompleteTextView cbSubject;
        private int temporaryPositionWarehouse = -1;
        private int temporaryPositionSubject = -1;
        private int temporaryPositioncbDoc = 0;
        private Button btnOrder;
        private Button btnOrderMode;
        private Button logout;
        private NameValueObjectList docTypes = null;
        private bool byOrder = true;
        List<ComboBoxItem> objectcbDocType = new List<ComboBoxItem>();
        List<ComboBoxItem> objectcbWarehouse = new List<ComboBoxItem>();
        List<ComboBoxItem> objectcbSubject = new List<ComboBoxItem>();
        private TextView label1;
        private TextView label2;
        private TextView lbSubject;
        private CustomAutoCompleteAdapter<ComboBoxItem> adapterSubject;
        private CustomAutoCompleteAdapter<ComboBoxItem> adapter;
        private CustomAutoCompleteAdapter<ComboBoxItem> adapterDoc;
        private RelativeLayout rlExtra;
        protected override async void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                SetTheme(Resource.Style.AppTheme_NoActionBar);
                // Create your application here
                if (App.Settings.tablet)
                {
                    base.RequestedOrientation = ScreenOrientation.Landscape;
                    base.SetContentView(Resource.Layout.TakeOverBusinessEventSetupTablet);
                    rlExtra = FindViewById<RelativeLayout>(Resource.Id.rlExtra);
                }
                else
                {
                    base.RequestedOrientation = ScreenOrientation.Portrait;
                    base.SetContentView(Resource.Layout.TakeOverBusinessEventSetup);
                }

                LoaderManifest.LoaderManifestLoopResources(this);


                AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
                var _customToolbar = new CustomToolbar(this, toolbar, Resource.Id.navIcon);
                _customToolbar.SetNavigationIcon(App.Settings.RootURL + "/Services/Logo");
                SetSupportActionBar(_customToolbar._toolbar);
                SupportActionBar.SetDisplayShowTitleEnabled(false);
                // Declarations
                cbDocType = FindViewById<CustomAutoCompleteTextView>(Resource.Id.cbDocType);


                cbWarehouse = FindViewById<CustomAutoCompleteTextView>(Resource.Id.cbWarehouse);
                cbSubject = FindViewById<CustomAutoCompleteTextView>(Resource.Id.cbSubject);
                btnOrder = FindViewById<Button>(Resource.Id.btnOrder);
                btnOrderMode = FindViewById<Button>(Resource.Id.btnOrderMode);
                logout = FindViewById<Button>(Resource.Id.btnLogout);
                label1 = FindViewById<TextView>(Resource.Id.label1);
                label2 = FindViewById<TextView>(Resource.Id.label2);
                lbSubject = FindViewById<TextView>(Resource.Id.lbSubject);
                btnOrder.Click += BtnOrder_Click;
                btnOrderMode.Click += BtnOrderMode_Click;
                logout.Click += Logout_Click;
                btnOrderMode.Enabled = await Services.HasPermission("TNET_WMS_BLAG_ACQ_NORDER", "R", this);
                var warehouses = await CommonData.ListWarehousesAsync();
                if (warehouses != null)
                {
                    warehouses.Items.ForEach(wh =>
                    {
                        objectcbWarehouse.Add(new ComboBoxItem { ID = wh.GetString("Subject"), Text = wh.GetString("Name") });
                    });
                }
                adapter = new CustomAutoCompleteAdapter<ComboBoxItem>(this,
                Android.Resource.Layout.SimpleSpinnerItem, objectcbWarehouse);
                adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerItem);
                cbWarehouse.Adapter = adapter;

                adapterDoc = new CustomAutoCompleteAdapter<ComboBoxItem>(this,
                Android.Resource.Layout.SimpleSpinnerItem, objectcbDocType);

                adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerItem);
                cbDocType.Adapter = adapterDoc;
                adapterDoc.SetNotifyOnChange(true);

                await UpdateForm();


                var dw = await CommonData.GetSettingAsync("DefaultWarehouse", this);
                if (!string.IsNullOrEmpty(dw))
                {
                    temporaryPositionWarehouse = cbWarehouse.SetItemByString(dw);
                }

                var _broadcastReceiver = new NetworkStatusBroadcastReceiver();
                _broadcastReceiver.ConnectionStatusChanged += OnNetworkStatusChanged;

                Application.Context.RegisterReceiver(_broadcastReceiver,
                new IntentFilter(ConnectivityManager.ConnectivityAction), ReceiverFlags.NotExported);

                cbDocType.ItemClick += CbDocType_ItemClick;
                cbSubject.ItemClick += CbSubject_ItemClick;
                cbWarehouse.ItemClick += CbWarehouse_ItemClick;

                await InitializeAutocompleteControls();


                LoaderManifest.LoaderManifestLoopStop(this);
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
                try
                {
                    cbDocType.SelectAtPosition(0);
                    var dws = await Queries.DefaultTakeoverWarehouse(objectcbDocType.ElementAt(0).ID);
                    temporaryPositionWarehouse = cbWarehouse.SetItemByString(dws.warehouse);
                    if (dws.main)
                    {
                        cbWarehouse.Enabled = false;
                    }
                }
                catch
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private void CbWarehouse_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
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

        private void CbSubject_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            try
            {
                temporaryPositionSubject = e.Position;
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
                var selected = objectcbDocType.ElementAt(e.Position);

                temporaryPositioncbDoc = e.Position;

                var dws = await Queries.DefaultTakeoverWarehouse(objectcbDocType.ElementAt(temporaryPositioncbDoc).ID);

                temporaryPositionWarehouse = cbWarehouse.SetItemByString(dws.warehouse);

                if (dws.main)
                {
                    cbWarehouse.Enabled = false;
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
                    // in smartphone
                    case Keycode.F2:
                        if (btnOrder.Enabled == true)
                        {
                            BtnOrder_Click(this, null);
                        }
                        break;
                    // return true;
                    case Keycode.F3:
                        if (btnOrderMode.Enabled == true)
                        {
                            BtnOrderMode_Click(this, null);
                        }
                        break;
                    case Keycode.F8:
                        if (logout.Enabled == true)
                        {
                            Logout_Click(this, null);
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

        private async void BtnOrderMode_Click(object sender, EventArgs e)
        {
            try
            {
                if (byOrder && (await CommonData.GetSettingAsync("UseDirectTakeOver", this) == "1"))
                {
                    // Special process for SkiSea, direct takeover. 20.05.2024 Janko Jovičić
                    StartActivity(typeof(TakeOver2Main));
                    Finish();
                }
                byOrder = !byOrder;
                Base.Store.byOrder = byOrder;
                await UpdateForm();
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

        private async Task UpdateForm()
        {
            try
            {
                try
                {

                    objectcbDocType.Clear();

                    if (byOrder)
                    {
                        lbSubject.Visibility = ViewStates.Invisible;
                        if (App.Settings.tablet)
                        {
                            rlExtra.Visibility = ViewStates.Invisible;
                        }
                        else
                        {
                            cbSubject.Visibility = ViewStates.Invisible;
                        }
                        docTypes = await CommonData.ListDocTypesAsync("I|N");
                        if (App.Settings.tablet)
                        {
                            btnOrderMode.Text = base.Resources.GetString(Resource.String.s138);

                        }
                        else
                        {
                            btnOrderMode.Text = base.Resources.GetString(Resource.String.s30);
                        }
                    }
                    else
                    {
                        lbSubject.Visibility = ViewStates.Visible;

                        if (App.Settings.tablet)
                        {
                            rlExtra.Visibility = ViewStates.Visible;
                        }
                        else
                        {
                            cbSubject.Visibility = ViewStates.Visible;
                        }
                        if (cbSubject.Adapter == null || cbSubject.Count() == 0)
                        {
                            var subjects = await CommonData.ListSubjectsAsync();
                            subjects.Items.ForEach(s =>
                            {
                                objectcbSubject.Add(new ComboBoxItem { ID = s.GetString("ID"), Text = s.GetString("ID") });

                            });

                            adapterSubject = new CustomAutoCompleteAdapter<ComboBoxItem>(this,
                            Android.Resource.Layout.SimpleSpinnerItem, objectcbSubject);
                            adapterSubject.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerItem);
                            cbSubject.Adapter = adapterSubject;
                        }

                        docTypes = await CommonData.ListDocTypesAsync("P|F");

                        if (App.Settings.tablet)
                        {
                            btnOrderMode.Text = base.Resources.GetString(Resource.String.s338);

                        }
                        else
                        {
                            btnOrderMode.Text = base.Resources.GetString(Resource.String.s32);
                        }
                    }

                    docTypes.Items.ForEach(dt =>
                    {
                        objectcbDocType.Add(new ComboBoxItem { ID = dt.GetString("Code"), Text = dt.GetString("Code") + " " + dt.GetString("Name") });
                    });


                    // Refresh the data
                    adapterDoc.Clear();
                    adapterDoc.AddAll(objectcbDocType);
                    adapterDoc.NotifyDataSetChanged();

                    if (objectcbDocType.Count == 0)
                    {
                        cbDocType.Text = string.Empty;
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


        private void NextStep()
        {
            try
            {
                var itemDT = adapterDoc.GetItem(temporaryPositioncbDoc);
                if (itemDT == null)
                {
                    string toast = string.Format($"{Resources.GetString(Resource.String.s237)}");
                    Toast.MakeText(this, toast, ToastLength.Long).Show();
                    return;
                }
                else
                {
                    if (temporaryPositionWarehouse == -1)
                    {
                        string toast = string.Format($"{Resources.GetString(Resource.String.s270)}");
                        Toast.MakeText(this, toast, ToastLength.Long).Show();
                        return;

                    }

                    var itemWH = adapter.GetItem(temporaryPositionWarehouse);
                    if (itemWH == null)
                    {
                        string toast = string.Format($"{Resources.GetString(Resource.String.s270)}");
                        Toast.MakeText(this, toast, ToastLength.Long).Show();
                        return;

                    }
                    else
                    {
                        ComboBoxItem itemSubj = null;
                        if (!byOrder)
                        {
                            if (temporaryPositionSubject == -1)
                            {
                                string toast = string.Format($"{Resources.GetString(Resource.String.s270)}");
                                Toast.MakeText(this, toast, ToastLength.Long).Show();

                                return;
                            }
                            itemSubj = adapterSubject.GetItem(temporaryPositionSubject);
                            if (itemSubj == null)
                            {
                                string toast = string.Format($"{Resources.GetString(Resource.String.s270)}");
                                Toast.MakeText(this, toast, ToastLength.Long).Show();

                                return;
                            }
                        }

                        NameValueObject moveHead = (NameValueObject)InUseObjects.Get("MoveHead");
                        moveHead.SetString("DocumentType", itemDT.ID);
                        moveHead.SetString("Wharehouse", itemWH.ID);
                        moveHead.SetBool("ByOrder", byOrder);
                        if (!byOrder)
                        {
                            moveHead.SetString("Receiver", itemSubj.ID);
                        }

                        StartActivity(typeof(TakeOverIdentEntry));
                        Finish();

                    }
                }
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }


    }
}
