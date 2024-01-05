using Stream = Android.Media.Stream;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Net;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.Lang;
using Microsoft.AppCenter.Crashes;
using WMS.App;
using TrendNET.WMS.Core.Data;
using TrendNET.WMS.Device.App;
using TrendNET.WMS.Device.Services; 

using Exception = System.Exception;

using AndroidX.AppCompat.App;using AlertDialog = Android.App.AlertDialog;namespace WMS
{
    [Activity(Label = "IssuedGoodsBusinessEventSetupTablet", ScreenOrientation = Android.Content.PM.ScreenOrientation.Landscape)]
    public class IssuedGoodsBusinessEventSetupTablet : AppCompatActivity,  IDialogInterfaceOnClickListener
    {
        private CustomAutoCompleteTextView cbDocType;
        public NameValueObjectList docTypes = null;
        private CustomAutoCompleteTextView cbWarehouse;
        private CustomAutoCompleteTextView cbExtra;
        List<ComboBoxItem> objectDocType = new List<ComboBoxItem>();
        List<ComboBoxItem> objectWarehouse = new List<ComboBoxItem>();
        List<ComboBoxItem> objectExtra = new List<ComboBoxItem>();
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
        private NameValueObjectList positions = null;
        private CustomAutoCompleteAdapter<ComboBoxItem> adapterExtra;
        private CustomAutoCompleteAdapter<ComboBoxItem> adapterWarehouse;
        private CustomAutoCompleteAdapter<ComboBoxItem> adapterDocType;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.IssuedGoodsBusinessEventSetupTablet);
            AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
            var _customToolbar = new CustomToolbar(this, toolbar, Resource.Id.navIcon);
            _customToolbar.SetNavigationIcon(settings.RootURL + "/Services/Logo");
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
            // next
            var warehouses = CommonData.ListWarehouses();
            warehouses.Items.ForEach(wh =>
            {
                objectWarehouse.Add(new ComboBoxItem { ID = wh.GetString("Subject"), Text = wh.GetString("Name") });
            });
            adapterWarehouse = new CustomAutoCompleteAdapter<ComboBoxItem>(this,
            Android.Resource.Layout.SimpleSpinnerItem, objectWarehouse);
            ///* 22.12.2020---------------------------------------------------------------
            ///* Documentation for the spinner objects add method with an adapter...
            ///*---------------------------------------------------
            ///
            adapterWarehouse.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            cbWarehouse.Adapter = adapterWarehouse;
            // Function update form...
            UpdateForm();
            adapterExtra = new CustomAutoCompleteAdapter<ComboBoxItem>(this,
            Android.Resource.Layout.SimpleSpinnerItem, objectExtra);
            adapterExtra.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            cbExtra.Adapter = adapterExtra;
            adapterDocType = new CustomAutoCompleteAdapter<ComboBoxItem>(this,
            Android.Resource.Layout.SimpleSpinnerItem, objectDocType);
            adapterDocType.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            cbDocType.Adapter = adapterDocType;
            btnOrderMode.Enabled = Services.HasPermission("TNET_WMS_BLAG_SND_NORDER", "R");
            FillOpenOrders();
            var _broadcastReceiver = new NetworkStatusBroadcastReceiver();
            _broadcastReceiver.ConnectionStatusChanged += OnNetworkStatusChanged;
            Application.Context.RegisterReceiver(_broadcastReceiver,
            new IntentFilter(ConnectivityManager.ConnectivityAction));
            cbDocType.ItemClick += CbDocType_ItemClick;
            cbExtra.ItemClick += CbExtra_ItemClick;
            cbWarehouse.ItemClick += CbWarehouse_ItemClick;
            InitializeAutocompleteControls();
        }

        private void InitializeAutocompleteControls()
        {
            cbDocType.SelectAtPosition(0);
            cbExtra.SelectAtPosition(0);
            var dws = Queries.DefaultIssueWarehouse(objectDocType.ElementAt(0).ID);
            temporaryPositionWarehouse = cbWarehouse.SetItemByString(dws.warehouse);
            if (dws.main)
            {
                cbWarehouse.Enabled = false;
            }
            FillOpenOrders();
        }

        private void CbWarehouse_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            try
            {
                temporaryPositionWarehouse = e.Position;
                FillOpenOrders();
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

        private void CbDocType_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            try
            {
                temporaryPositionDoc = e.Position;
                FillOpenOrders();
                var dws = Queries.DefaultIssueWarehouse(objectDocType.ElementAt(temporaryPositionDoc).ID);
                if (dws.main)
                {
                    cbWarehouse.Enabled = false;
                }
                temporaryPositionWarehouse = cbWarehouse.SetItemByString(dws.warehouse);

            }
            catch (Exception ex)
            {
                string toast = string.Format("Napaka" + ex.ToString());
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

        private async Task SendEvents()
        {
            await Task.Run(() =>
            {
                Instrumentation inst = new Instrumentation();            
                inst.SendKeyDownUpSync(Keycode.Back);
            });
        } 

        public void OnClick(IDialogInterface dialog, int which)
        {
       
        }
        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            switch (keyCode)
            {

                // Setting F2 to method ProccesStock()
                case Keycode.F1:

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

                case Keycode.F3://
                    if (btnLogout.Enabled == true)
                    {
                        BtnLogout_Click(this, null);
                    }

                    break;



            }
            return base.OnKeyDown(keyCode, e);
        }



        private void FillOpenOrders()
        {

            if ((byOrder && CommonData.GetSetting("UseSingleOrderIssueing") == "1"))
            {
                string toast = string.Format("Pridobivam seznam odprtih naručila");
                
                try
                {
                    objectExtra.Clear();
                    var dt = adapterDocType.GetItem(temporaryPositionDoc);
                    if (dt != null)
                    {
                        var wh = adapterWarehouse.GetItem(temporaryPositionWarehouse);
                        if (wh != null && !string.IsNullOrEmpty(wh.ID))
                        {
                            string error;
                            positions = Services.GetObjectList("oodtw", out error, dt.ID + "|" + wh.ID + "|" + byClient);
                            if (positions == null)
                            {
                                string toasted = string.Format("Napaka pri pridobivanju odprtih naročil: " + error);
                                Toast.MakeText(this, toasted, ToastLength.Long).Show();
                           
                                return;
                            }

                            positions.Items.ForEach(p =>
                            {
                                if (!string.IsNullOrEmpty(p.GetString("Key")))
                                {
                                    objectExtra.Add(new ComboBoxItem { ID = p.GetString("Key"), Text = p.GetString("ShortKey") + " " + p.GetString("FillPercStr") + " " + p.GetString("Receiver") });
                                }
                            });
                            adapterExtra.NotifyDataSetChanged();

                            cbExtra.RequestFocus();
                        }
                    }
                }
                finally
                {
                    var adapterExtra = new CustomAutoCompleteAdapter<ComboBoxItem>(this,
                                  Android.Resource.Layout.SimpleSpinnerItem, objectExtra);
                    adapterExtra.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerItem);
                    cbExtra.Adapter = null;

                    cbExtra.Adapter = adapterExtra;
                    adapterExtra.NotifyDataSetChanged();
                }
            }
        }
        private void BtnLogout_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(MainMenuTablet));
            HelpfulMethods.clearTheStack(this);
        }

        private void BtnOrderMode_Click(object sender, EventArgs e)
        {
            byOrder = !byOrder;
            UpdateForm();
        }

        private void BtnOrder_Click(object sender, EventArgs e)
        {
            NextStep();
        }


        private void UpdateForm()
        {
            objectDocType.Clear();
     
            if (byOrder)
            {

                if ((CommonData.GetSetting("UseSingleOrderIssueing") == "1"))
                {
                    lbExtra.Visibility = ViewStates.Visible;
                    FindViewById<RelativeLayout>(Resource.Id.rlExtra).Visibility = ViewStates.Visible;
                    cbExtra.Visibility = ViewStates.Visible;      
                    lbExtra.Text = "Naročilo:";                
                }
                else

                {
                    FindViewById<RelativeLayout>(Resource.Id.rlExtra).Visibility = ViewStates.Invisible;
                    lbExtra.Visibility = ViewStates.Invisible;
                    cbExtra.Visibility = ViewStates.Invisible;
                }

                docTypes = CommonData.ListDocTypes("P|N");

                btnOrderMode.Text = "Brez nar. - F3";
            }
            else
            {
                FindViewById<RelativeLayout>(Resource.Id.rlExtra).Visibility = ViewStates.Visible;


                lbExtra.Visibility = ViewStates.Visible;

                cbExtra.Visibility = ViewStates.Visible;

                lbExtra.Text = "Subjekt:";

                objectExtra.Clear();

                var subjects = CommonData.ListSubjects();

                subjects.Items.ForEach(s =>

                {

                    objectExtra.Add(new ComboBoxItem { ID = s.GetString("ID"), Text = s.GetString("ID") });

                });
                adapterExtra.NotifyDataSetChanged();

                docTypes = CommonData.ListDocTypes("I;M|F");

                btnOrderMode.Text = "Z nar. - F3";
            }

            docTypes.Items.ForEach(dt =>
            {

                objectDocType.Add(new ComboBoxItem { ID = dt.GetString("Code"), Text = dt.GetString("Code") + " " + dt.GetString("Name") });

            });
        }
        private void NextStep()
        {
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

                    moveHead.SetString("DocumentType", itemDT.ID);

                    moveHead.SetString("Wharehouse", itemWH.ID);

                    moveHead.SetBool("ByOrder", byOrder);
                    if (!byOrder)
                    {
                        moveHead.SetString("Receiver", itemSubj.ID);
                    }
                    InUseObjects.Set("MoveHead", moveHead);

                    NameValueObject order = null;
                    if ((byOrder && CommonData.GetSetting("UseSingleOrderIssueing") == "1"))
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

                    if ((byOrder && CommonData.GetSetting("UseSingleOrderIssueing") == "1"))
                    {
                        StartActivity(typeof(IssuedGoodsIdentEntryWithTrailTablet));

                        this.Finish();
                    }
                    else
                    {
                        StartActivity(typeof(IssuedGoodsIdentEntryTablet));

                        this.Finish();
                    }
                }
            }
        }

     
    }
}