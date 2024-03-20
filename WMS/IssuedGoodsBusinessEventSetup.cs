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


using AndroidX.AppCompat.App;using AlertDialog = Android.App.AlertDialog;namespace WMS
{
    [Activity(Label = "IssuedGoodsBusinessEventSetup", ScreenOrientation = ScreenOrientation.Portrait)]
    public class IssuedGoodsBusinessEventSetup : AppCompatActivity, IDialogInterfaceOnClickListener
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
        private Button btnHidden;
        private bool initialLoad;

     
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetTheme(Resource.Style.AppTheme_NoActionBar);
            // Create your application here
            SetContentView(Resource.Layout.IssuedGoodsBusinessEventSetup);
            AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
            var _customToolbar = new CustomToolbar(this, toolbar, Resource.Id.navIcon);
            _customToolbar.SetNavigationIcon(settings.RootURL + "/Services/Logo");
            SetSupportActionBar(_customToolbar._toolbar);
            SupportActionBar.SetDisplayShowTitleEnabled(false);
            LoaderManifest.LoaderManifestLoopResources(this);
            cbDocType = FindViewById<CustomAutoCompleteTextView>(Resource.Id.cbDocType);
            cbWarehouse = FindViewById<CustomAutoCompleteTextView>(Resource.Id.cbWarehouse);
            cbExtra = FindViewById<CustomAutoCompleteTextView>(Resource.Id.cbExtra);
            lbExtra = FindViewById<TextView>(Resource.Id.lbExtra);
            btnOrderMode = FindViewById<Button>(Resource.Id.btnOrderMode);
            btnOrder = FindViewById<Button>(Resource.Id.btnOrder);
            btnLogout = FindViewById<Button>(Resource.Id.btnLogout);
            btnHidden = FindViewById<Button>(Resource.Id.btnHidden);
            btnOrder.Click += BtnOrder_Click;
            btnOrderMode.Click += BtnOrderMode_Click;
            btnLogout.Click += BtnLogout_Click;
            hidden = FindViewById<Button>(Resource.Id.hidden);
            hidden.Click += Hidden_Click;
            focus = FindViewById<TextView>(Resource.Id.focus);
            var warehouses = CommonData.ListWarehouses();
            warehouses.Items.ForEach(wh =>
            {
                objectWarehouse.Add(new ComboBoxItem { ID = wh.GetString("Subject"), Text = wh.GetString("Name") });
            });
            adapterWarehouse = new CustomAutoCompleteAdapter<ComboBoxItem>(this,
            Android.Resource.Layout.SimpleSpinnerItem, objectWarehouse);
            adapterWarehouse.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerItem);
            cbWarehouse.Adapter = adapterWarehouse;
            string dw = CommonData.GetSetting("DefaultWarehouse");
            cbWarehouse.SetText(dw, false);
            UpdateForm();
            btnHidden.Click += BtnHidden_Click;
            adapterDocType = new CustomAutoCompleteAdapter<ComboBoxItem>(this,
            Android.Resource.Layout.SimpleSpinnerItem, objectDocType);
            adapterDocType.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerItem);
            cbDocType.Adapter = adapterDocType;
            btnOrderMode.Enabled = Services.HasPermission("TNET_WMS_BLAG_SND_NORDER", "R");
            cbWarehouse.Enabled = true;
            BottomSheetActions bottomSheetActions = new BottomSheetActions();
            initialLoad = true;
            var _broadcastReceiver = new NetworkStatusBroadcastReceiver();
            _broadcastReceiver.ConnectionStatusChanged += OnNetworkStatusChanged;
            Application.Context.RegisterReceiver(_broadcastReceiver,
            new IntentFilter(ConnectivityManager.ConnectivityAction));
            // cbExtra.ViewAttachedToWindow += CbExtra_ViewAttachedToWindow;        
            SelectState();
            // Events //
            cbDocType.ItemClick += CbDocType_ItemClick;
            cbExtra.ItemClick += CbExtra_ItemClick;
            cbWarehouse.ItemClick += CbWarehouse_ItemClick;
            InitializeAutocompleteControls();

        }

        private async void InitializeAutocompleteControls()
        {
            cbDocType.SelectAtPosition(0);
            cbExtra.SelectAtPosition(0);
            var dws = Queries.DefaultIssueWarehouse(objectDocType.ElementAt(0).ID);
            temporaryPositionWarehouse = cbWarehouse.SetItemByString(dws.warehouse);
            if(dws.main)
            {
                cbWarehouse.Enabled = false;
            }
            await FillOpenOrdersAsync();
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
                string toast = string.Format("Napaka" + ex.ToString());
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
                string toast = string.Format("Napaka" + ex.ToString());
                Toast.MakeText(this, toast, ToastLength.Long).Show();
            }
        }

        private async void CbDocType_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            try
            {
                temporaryPositionDoc = e.Position;
                var dws = Queries.DefaultIssueWarehouse(adapterDocType.GetItem(temporaryPositionDoc).ID);
                temporaryPositionWarehouse = cbWarehouse.SetItemByString(dws.warehouse);
                if(dws.main)
                {
                    cbWarehouse.Enabled = false;
                }
                cbExtra.Text = string.Empty;
                await FillOpenOrdersAsync();               
            }
            catch (Exception ex)
            {
                string toast = string.Format("Napaka" + ex.ToString());
                Toast.MakeText(this, toast, ToastLength.Long).Show();
            }
        }

        private void SelectState()
        {
            var index_doc = Intent.GetIntExtra("index_doc", 0);
            var index_war = Intent.GetIntExtra("index_war", 0);
            if(index_doc!=0||index_war!=0) {
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
                    Crashes.TrackError(err);
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

        private void BtnHidden_Click(object sender, EventArgs e)
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
                                var wh = cbWarehouse.Text;
                                if (wh != null && !string.IsNullOrEmpty(wh))
                                {
                                    string error;
                                    positions = Services.GetObjectList("oodtw", out error, dt + "|" + wh + "|" + byClient);
                              
                                    if (positions == null)
                                    {
                                        RunOnUiThread(() =>
                                        {
                                            string toasted = string.Format("Napaka pri pridobivanju odprtih naročil: " + error);
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
                        catch(Exception ex) 
                        {
                            Crashes.TrackError(ex);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Crashes.TrackError(ex);
                }
            });
        }
        private void BtnLogout_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(MainMenu));
            HelpfulMethods.clearTheStack(this);
            HelpfulMethods.clearTheStack(this);
        }

        private async void BtnOrderMode_Click(object sender, EventArgs e)
        {
            await FillOpenOrdersAsync();
            byOrder = !byOrder;
            UpdateForm();
        }

        private void BtnOrder_Click(object sender, EventArgs e)
        {
            // Fixing clicking the order without choosing an order...
            if (cbExtra.Visibility == ViewStates.Visible&&cbExtra.Text==string.Empty) 
            {
                Toast.MakeText(this, "Morate izbrati naročilo.", ToastLength.Long).Show();
            }
            else
            {
                NextStep();
            }
        }

     

        private async void UpdateForm()
        {
            objectExtra.Clear();
      
            if (byOrder)
            {
                int selectedFlow = Base.Store.modeIssuing;
                if (selectedFlow == 2)
                {
                    lbExtra.Visibility = ViewStates.Visible;
                    cbExtra.Visibility = ViewStates.Visible;
                    lbExtra.Text = "Naročilo:";
                }
                else
                {
                    lbExtra.Visibility = ViewStates.Invisible;
                    cbExtra.Visibility = ViewStates.Invisible;
                }
                if (initial > 0)
                {
                    await FillOpenOrdersAsync();
                }
                docTypes = CommonData.ListDocTypes("P|N");
                btnOrderMode.Text = Resources.GetString(Resource.String.s30);
                initial += 1;
            }
            else
            {
                lbExtra.Visibility = ViewStates.Visible;
                cbExtra.Visibility = ViewStates.Visible;
                lbExtra.Text = "Subjekt:";
                objectExtra.Clear();
                var subjects = CommonData.ListSubjects();
                subjects.Items.ForEach(s =>
                {
                    objectExtra.Add(new ComboBoxItem { ID = s.GetString("ID"), Text = s.GetString("ID") });
                });
                adapterExtra = new CustomAutoCompleteAdapter<ComboBoxItem>(this,
                             Android.Resource.Layout.SimpleSpinnerItem, objectExtra);
                cbExtra.Adapter = null;
                cbExtra.Adapter = adapterExtra;

                adapterExtra.NotifyDataSetChanged();
                docTypes = CommonData.ListDocTypes("I;M|F");
                btnOrderMode.Text = Resources.GetString(Resource.String.s32);
            }
            docTypes.Items.ForEach(dt =>
            {
                objectDocType.Add(new ComboBoxItem { ID = dt.GetString("Code"), Text = dt.GetString("Code") + " " + dt.GetString("Name") });
            });
        }


        private void NextStep()
        {
            if(temporaryPositionDoc == -1|| temporaryPositionWarehouse == -1)
            {
                return;
            }
            var itemDT = adapterDocType.GetItem(temporaryPositionDoc);
            if (itemDT == null)
            {
                string toast = string.Format("Poslovni dogodek more bit izbran");
                Toast.MakeText(this, toast, ToastLength.Long).Show();
            }
            else
            {
                var itemWH = adapterWarehouse.GetItem(temporaryPositionWarehouse);
                if (itemWH == null)
                {
                    string toast = string.Format("Sladište more biti izbrano.");
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
                            string toast = string.Format("Poslovni dogodek more bit izbran");
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
                        if (byOrder && selectedFlow == "2" )
                        {
                            itemSubj = adapterExtra.GetItem(temporaryPositionExtra);
                            if (itemSubj == null)
                            {
                                string toast = string.Format("Subjekt more biti izbran.");
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