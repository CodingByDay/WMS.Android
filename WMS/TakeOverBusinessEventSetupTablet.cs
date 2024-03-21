using Stream = Android.Media.Stream;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Net;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using Microsoft.AppCenter.Crashes;
using WMS.App;
using TrendNET.WMS.Core.Data;
using TrendNET.WMS.Device.App;
using TrendNET.WMS.Device.Services;

using AndroidX.AppCompat.App;using AlertDialog = Android.App.AlertDialog;namespace WMS
{
    [Activity(Label = "TakeOverBusinessEventSetupTablet", ScreenOrientation = Android.Content.PM.ScreenOrientation.Landscape)]
    public class TakeOverBusinessEventSetupTablet : AppCompatActivity
    {
        private CustomAutoCompleteTextView cbDocType;
        private CustomAutoCompleteTextView cbWarehouse;
        private CustomAutoCompleteTextView cbSubject;
        private int temporaryPositionWarehouse = 0;
        private int temporaryPositionSubject = 0;
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
        private CustomAutoCompleteAdapter<ComboBoxItem> adapter;
        private CustomAutoCompleteAdapter<ComboBoxItem> adapterDoc;
        private CustomAutoCompleteAdapter<ComboBoxItem> adapterSubject;

        //

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetTheme(Resource.Style.AppTheme_NoActionBar);
            SetContentView(Resource.Layout.TakeOverBusinessEventSetupTablet);
            AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
            var _customToolbar = new CustomToolbar(this, toolbar, Resource.Id.navIcon);
            _customToolbar.SetNavigationIcon(settings.RootURL + "/Services/Logo");
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


            btnOrderMode.Enabled = Services.HasPermission("TNET_WMS_BLAG_ACQ_NORDER", "R");

            var warehouses = CommonData.ListWarehouses();


            if (warehouses != null)
            {
                warehouses.Items.ForEach(wh =>
                {
                    objectcbWarehouse.Add(new ComboBoxItem { ID = wh.GetString("Subject"), Text = wh.GetString("Name") });
                });
            }



            adapter = new CustomAutoCompleteAdapter<ComboBoxItem>(this,
            Android.Resource.Layout.SimpleSpinnerItem, objectcbWarehouse);
            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            cbWarehouse.Adapter = adapter;
            UpdateForm();         
            adapterDoc = new CustomAutoCompleteAdapter<ComboBoxItem>(this,
            Android.Resource.Layout.SimpleSpinnerItem, objectcbDocType);
            adapterDoc.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            cbDocType.Adapter = adapterDoc;
            cbDocType.ItemSelected += CbDocType_ItemSelected;
            cbSubject.ItemSelected += CbSubject_ItemSelected;
            cbWarehouse.ItemSelected += CbWarehouse_ItemSelected;        
            var _broadcastReceiver = new NetworkStatusBroadcastReceiver();
            _broadcastReceiver.ConnectionStatusChanged += OnNetworkStatusChanged;
            Application.Context.RegisterReceiver(_broadcastReceiver,
            new IntentFilter(ConnectivityManager.ConnectivityAction));
            cbDocType.ItemClick += CbDocType_ItemClick;
            cbSubject.ItemClick += CbSubject_ItemClick;
            cbWarehouse.ItemClick += CbWarehouse_ItemClick;
            InitializeAutocompleteControls();
        }


        private void InitializeAutocompleteControls()
        {
            cbDocType.SelectAtPosition(0);
            var dws = Queries.DefaultTakeoverWarehouse(objectcbDocType.ElementAt(0).ID);
            temporaryPositionWarehouse = cbWarehouse.SetItemByString(dws.warehouse);
            if (dws.main)
            {
                cbWarehouse.Enabled = false;
            }
        }

        private void CbWarehouse_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            temporaryPositionWarehouse = e.Position;
        }

        private void CbSubject_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            temporaryPositionSubject = e.Position;
        }

        private void CbDocType_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            temporaryPositioncbDoc = e.Position;
            var dws = Queries.DefaultTakeoverWarehouse(objectcbDocType.ElementAt(temporaryPositioncbDoc).ID);
            temporaryPositionWarehouse = cbWarehouse.SetItemByString(dws.warehouse);
            if (dws.main)
            {
                cbWarehouse.Enabled = false;
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
                // in smartphone
                case Keycode.F2:
                    if (btnOrder.Enabled == true)
                    {
                        BtnOrder_Click(this, null);
                    }
                    break;
                //return true;


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
        private void Logout_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(MainMenuTablet));
            HelpfulMethods.clearTheStack(this);
        }

        private void BtnOrderMode_Click(object sender, EventArgs e)
        {
            if (byOrder && (CommonData.GetSetting("UseDirectTakeOver") == "1"))
            {
                StartActivity(typeof(TakeOver2MainTablet));
                HelpfulMethods.clearTheStack(this);


            }
            else
            {
                byOrder = !byOrder;
                UpdateForm();

            }
        }


        private void BtnOrder_Click(object sender, EventArgs e)
        {
            NextStep();
        }

        private void UpdateForm()
        {


            try
            {


                objectcbDocType.Clear();

                if (byOrder)
                {
                    FindViewById<RelativeLayout>(Resource.Id.rlExtra).Visibility = ViewStates.Invisible;

                    lbSubject.Visibility = ViewStates.Invisible;
                    cbSubject.Visibility = ViewStates.Invisible;

                    docTypes = CommonData.ListDocTypes("I|N");

                    btnOrderMode.Text = $"{Resources.GetString(Resource.String.s30)}";
                }
                else
                {
                    FindViewById<RelativeLayout>(Resource.Id.rlExtra).Visibility = ViewStates.Visible;

                    lbSubject.Visibility = ViewStates.Visible;
                    cbSubject.Visibility = ViewStates.Visible;

                    if (cbSubject.Count() == 0)
                    {
                        var subjects = CommonData.ListSubjects();
                        subjects.Items.ForEach(s =>
                        {
                            objectcbSubject.Add(new ComboBoxItem { ID = s.GetString("ID"), Text = s.GetString("ID") });
                        });
                    }
                    adapterSubject = new CustomAutoCompleteAdapter<ComboBoxItem>(this,
                    Android.Resource.Layout.SimpleSpinnerItem, objectcbSubject);
                    adapterSubject.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
                    cbSubject.Adapter = adapterSubject;
                    docTypes = CommonData.ListDocTypes("P|F");

                    btnOrderMode.Text = "Z nar - F3";
                }

                docTypes.Items.ForEach(dt =>
                {
                    objectcbDocType.Add(new ComboBoxItem { ID = dt.GetString("Code"), Text = dt.GetString("Code") + " " + dt.GetString("Name") });
                });
            }
            catch (Exception err)
            {

                Crashes.TrackError(err);
                return;

            }
        }


        private void NextStep()
        {
            if(temporaryPositioncbDoc == -1 || temporaryPositionSubject == -1 || temporaryPositionWarehouse == -1)
            {
                return;
            }
            var itemDT = adapterDoc.GetItem(temporaryPositioncbDoc);
            if (itemDT == null)
            {
                string toast = string.Format($"{Resources.GetString(Resource.String.s237)}");
                Toast.MakeText(this, toast, ToastLength.Long).Show();
            }
            else
            {
                var itemWH = adapter.GetItem(temporaryPositionWarehouse);
                if (itemWH == null)
                {
                    string toast = string.Format("Skladište more biti izbrano");
                    Toast.MakeText(this, toast, ToastLength.Long).Show();

                }
                else
                {
                    ComboBoxItem itemSubj = null;
                    if (!byOrder)
                    {
                        itemSubj = adapterSubject.GetItem(temporaryPositionSubject);
                        if (itemSubj == null)
                        {
                            string toast = string.Format("Subjekt more bit izbran");
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

                    StartActivity(typeof(TakeOverIdentEntryTablet));
                    HelpfulMethods.clearTheStack(this);

                }
            }

        }

        private void CbWarehouse_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {

            Spinner spinner = (Spinner)sender;
            string toast = string.Format($"{Resources.GetString(Resource.String.s236)}: {0}", spinner.GetItemAtPosition(e.Position));
            temporaryPositionWarehouse = e.Position;
        }

        private void CbSubject_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {

            Spinner spinner = (Spinner)sender;
            string toast = string.Format($"{Resources.GetString(Resource.String.s236)}: {0}", spinner.GetItemAtPosition(e.Position));
            temporaryPositionSubject = e.Position;
        }

        private void CbDocType_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {

            Spinner spinner = (Spinner)sender;
            string toast = string.Format($"{Resources.GetString(Resource.String.s236)}: {0}", spinner.GetItemAtPosition(e.Position));
            temporaryPositioncbDoc = e.Position;
        }
    }
}
