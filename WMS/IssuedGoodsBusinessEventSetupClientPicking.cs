using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.Net;
using Android.OS;
using Android.Service.Autofill;
using Android.Text.Method;
using Android.Views;
using Android.Widget;
using Java.Lang.Reflect;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using WMS.App;
using Stream = Android.Media.Stream;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using TrendNET.WMS.Core.Data;
using TrendNET.WMS.Device.App;
using TrendNET.WMS.Device.Services;
using static Android.Content.ClipData;
using Android.Speech.Tts;

using AndroidX.AppCompat.App;using AlertDialog = Android.App.AlertDialog;namespace WMS
{
    [Activity(Label = "IssuedGoodsBusinessEventSetupClientPicking", ScreenOrientation = ScreenOrientation.Portrait)]
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
            base.OnCreate(savedInstanceState);
            SetTheme(Resource.Style.AppTheme_NoActionBar);
            // Create your application here
            SetContentView(Resource.Layout.IssuedGoodsBusinessEventSetupClientPicking);
            AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
            var _customToolbar = new CustomToolbar(this, toolbar, Resource.Id.navIcon);
            _customToolbar.SetNavigationIcon(settings.RootURL + "/Services/Logo");
            SetSupportActionBar(_customToolbar._toolbar);
            SupportActionBar.SetDisplayShowTitleEnabled(false);
            LoaderManifest.LoaderManifestLoopResources(this);
            cbDocType = FindViewById<CustomAutoCompleteTextView>(Resource.Id.cbDocType);
            cbWarehouse = FindViewById<CustomAutoCompleteTextView>(Resource.Id.cbWarehouse);
            cbExtra = FindViewById<CustomAutoCompleteTextView>(Resource.Id.cbExtra);
            btnOrder = FindViewById<Button>(Resource.Id.btnOrder);
            btnLogout = FindViewById<Button>(Resource.Id.btnLogout);
            btnHidden = FindViewById<Button>(Resource.Id.btnHidden);
       
            btnLogout.Click += BtnLogout_Click;
            btnOrder.Click += BtnOrder_Click;


            var warehouses = Services.GetObjectListBySql($"SELECT acWarehouse, acName FROM uWMSWarehouse");

            if (warehouses.Success)
            {
                foreach (var war in warehouses.Rows)
                {
                    objectWarehouse.Add(new ComboBoxItem { ID = war.StringValue("acWarehouse"), Text = war.StringValue("acWarehouse") });
                }
            }
           
            adapterWarehouse = new CustomAutoCompleteAdapter<ComboBoxItem>(this,
            Android.Resource.Layout.SimpleSpinnerItem, objectWarehouse);
            adapterWarehouse.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerItem);
            cbWarehouse.Adapter = adapterWarehouse;
            UpdateForm();       
            adapterDocType = new CustomAutoCompleteAdapter<ComboBoxItem>(this,
            Android.Resource.Layout.SimpleSpinnerItem, objectDocType);
            adapterDocType.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerItem);
            cbDocType.Adapter = adapterDocType;
            cbWarehouse.Enabled = true;
            initialLoad = true;
            var _broadcastReceiver = new NetworkStatusBroadcastReceiver();
            _broadcastReceiver.ConnectionStatusChanged += OnNetworkStatusChanged;
            Application.Context.RegisterReceiver(_broadcastReceiver,
            new IntentFilter(ConnectivityManager.ConnectivityAction));
            cbDocType.ItemClick += CbDocType_ItemClick;
            cbExtra.ItemClick += CbExtra_ItemClick;
            cbWarehouse.ItemClick += CbWarehouse_ItemClick;
            InitializeAutocompleteControls();

        }

        private async void InitializeAutocompleteControls()
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


        private async Task FillOpenOrdersAsync()
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

                                var subjects = Services.GetObjectListBySql($"SELECT * FROM uWMSOrderSubjectByTypeWarehouseOut WHERE acDocType = @acDocType AND acWarehouse = @acWarehouse", parameters);
                                if (!subjects.Success)
                                {
                                    RunOnUiThread(() =>
                                    {
                                        Analytics.TrackEvent(subjects.Error);
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
                        Crashes.TrackError(ex);
                    }
                }
                catch (Exception err)
                {
                    Crashes.TrackError(err);
                }
            });
        }


        private async void CbWarehouse_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
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
                Crashes.TrackError(ex);
                return;
            }
        }

        private void CbExtra_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            temporaryPositionExtra = e.Position;
            currentClient = adapterExtra.GetItem(e.Position);
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
                await FillOpenOrdersAsync();
            }
            catch (Exception ex)
            {
                string toast = string.Format($"{Resources.GetString(Resource.String.s265)}" + ex.ToString());
                Toast.MakeText(this, toast, ToastLength.Long).Show();
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
                    Crashes.TrackError(err);
                }
            }
            else
            {
                LoaderManifest.LoaderManifestLoop(this);
            }
        }
     

 


        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
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

      

     
        private void BtnLogout_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(MainMenu));
            HelpfulMethods.clearTheStack(this);
            HelpfulMethods.clearTheStack(this);
        }


        private void BtnOrder_Click(object sender, EventArgs e)
        {

            NextStep();
            
        }

        private void NextStep()
        {
            if(String.IsNullOrEmpty(currentClient))
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
            StartActivity(typeof(ClientPicking));
        }

    

    

        private void UpdateForm()
        {
            objectExtra.Clear();
            docTypes = CommonData.ListDocTypes("P|N");
            initial += 1;
            var result = Services.GetObjectListBySql("SELECT * FROM uWMSOrderDocTypeOut;");
            foreach (Row row in result.Rows)
            {
                objectDocType.Add(new ComboBoxItem { ID = row.StringValue("acDocType"), Text = row.StringValue("acDocType") + " - " + row.StringValue("acName") });
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