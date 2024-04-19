using Stream = Android.Media.Stream;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.PM;
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
using AndroidX.AppCompat.App;
using AlertDialog = Android.App.AlertDialog;
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

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetTheme(Resource.Style.AppTheme_NoActionBar);
            // Create your application here
            SetContentView(Resource.Layout.TakeOverBusinessEventSetup);
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
            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerItem);
            cbWarehouse.Adapter = adapter;
            UpdateForm();   
            adapterDoc = new CustomAutoCompleteAdapter<ComboBoxItem>(this,
            Android.Resource.Layout.SimpleSpinnerItem, objectcbDocType);

            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerItem);
            cbDocType.Adapter = adapterDoc;

            var dw = CommonData.GetSetting("DefaultWarehouse");
            if (!string.IsNullOrEmpty(dw))
            {
                temporaryPositionWarehouse = cbWarehouse.SetItemByString(dw);
            }

            var _broadcastReceiver = new NetworkStatusBroadcastReceiver();
            _broadcastReceiver.ConnectionStatusChanged += OnNetworkStatusChanged;

            Application.Context.RegisterReceiver(_broadcastReceiver,
            new IntentFilter(ConnectivityManager.ConnectivityAction));

            cbDocType.ItemClick += CbDocType_ItemClick;
            cbSubject.ItemClick += CbSubject_ItemClick;
            cbWarehouse.ItemClick += CbWarehouse_ItemClick;

            InitializeAutocompleteControls();
        }

        private async void InitializeAutocompleteControls()
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
            } catch 
            {
                return;
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


        private async void CbDocType_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
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
        private void Logout_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(MainMenu));
            HelpfulMethods.clearTheStack(this);
        }

        private void BtnOrderMode_Click(object sender, EventArgs e)
        {
     
            byOrder = !byOrder;
            Base.Store.byOrder = byOrder;
            UpdateForm();
            
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
                    lbSubject.Visibility = ViewStates.Invisible;
                    cbSubject.Visibility = ViewStates.Invisible;

                    docTypes = CommonData.ListDocTypes("I|N");
                    btnOrderMode.Text = Resources.GetString(Resource.String.s30);
                }
                else
                {
                    lbSubject.Visibility = ViewStates.Visible;
                    cbSubject.Visibility = ViewStates.Visible;

                    if ( cbSubject.Adapter == null || cbSubject.Count() == 0 )
                    {
                        var subjects = CommonData.ListSubjects();
                        subjects.Items.ForEach(s =>
                        {
                            objectcbSubject.Add(new ComboBoxItem { ID = s.GetString("ID"), Text = s.GetString("ID") });
                    
                        });

                        adapterSubject = new CustomAutoCompleteAdapter<ComboBoxItem>(this,
                        Android.Resource.Layout.SimpleSpinnerItem, objectcbSubject);
                        adapterSubject.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerItem);
                        cbSubject.Adapter = adapterSubject;
                    }

                    docTypes = CommonData.ListDocTypes("P|F");

                    btnOrderMode.Text = Resources.GetString(Resource.String.s32);
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
                   HelpfulMethods.clearTheStack(this);

                }
            }
         
        }

      
    }
}
