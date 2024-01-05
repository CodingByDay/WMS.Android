using System;
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
using Scanner.App;
using TrendNET.WMS.Core.Data;
using TrendNET.WMS.Device.App;
using TrendNET.WMS.Device.Services;
using static Android.App.ActionBar;
using WebApp = TrendNET.WMS.Device.Services.WebApp;

namespace WMS
{
    [Activity(Label = "UnfinishedIssuedGoodsView", ScreenOrientation = ScreenOrientation.Landscape)]
    public class IssuedGoodsBusinessEventSetupClientPickingTablet : Activity
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

            protected override void OnCreate(Bundle savedInstanceState)
            {
                base.OnCreate(savedInstanceState);
                // Create your application here
                SetContentView(Resource.Layout.IssuedGoodsBusinessEventSetupClientPickingTablet);
                AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
                var _customToolbar = new CustomToolbar(this, toolbar, Resource.Id.navIcon);
                _customToolbar.SetNavigationIcon(settings.RootURL + "/Services/Logo");
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
                var adapterWarehouse = new CustomAutoCompleteAdapter<ComboBoxItem>(this,
                Android.Resource.Layout.SimpleSpinnerItem, objectWarehouse);
                adapterWarehouse.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerItem);
                cbWarehouse.Adapter = adapterWarehouse;
                UpdateForm();
                FillOpenOrders();
                var adapterDocType = new CustomAutoCompleteAdapter<ComboBoxItem>(this,
                Android.Resource.Layout.SimpleSpinnerItem, objectDocType);
                adapterDocType.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerItem);
                cbDocType.Adapter = adapterDocType;
                cbWarehouse.Enabled = true;
                BottomSheetActions bottomSheetActions = new BottomSheetActions();
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


        private void InitializeAutocompleteControls()
        {
            cbDocType.SelectAtPosition(0);
            cbExtra.SelectAtPosition(0);
            string dw = CommonData.GetSetting("DefaultWarehouse");
            temporaryPositionWarehouse = cbWarehouse.SetItemByString(dw);
            FillOpenOrders();
        }


        private void CbWarehouse_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            try
            {
                temporaryPositionWarehouse = e.Position;
                var wh = objectWarehouse.ElementAt(temporaryPositionWarehouse);
                currentWarehouse = wh.ID;
                FillOpenOrders();
            }
            catch
            {
                return;
            }
        }

        private void CbExtra_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            currentClient = objectExtra.ElementAt(e.Position);
        }

        private void CbDocType_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            try
            {
                temporaryPositionDoc = e.Position;
                string dw = CommonData.GetSetting("DefaultWarehouse");
                cbWarehouse.SetText(dw, false);
                FillOpenOrders();
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
            try
            {

                string toast = string.Format("Pridobivam seznam odprtih naročila");

                try
                {
                    objectExtra.Clear();
                    var dt = objectDocType.ElementAt(temporaryPositionDoc);

                    if (dt != null)
                    {
                        var wh = objectWarehouse.ElementAt(temporaryPositionWarehouse);
                        if (wh != null && !string.IsNullOrEmpty(wh.ID))
                        {
                            string error;

                            var subjects = Services.GetObjectListBySql($"SELECT * FROM uWMSOrderSubjectByTypeWarehouseOut WHERE acDocType = '{dt.ID}' AND acWarehouse = '{wh.ID}'");

                            if (!subjects.Success)
                            {
                                string toasted = string.Format("Napaka pri pridobivanju odprtih naročil: " + subjects.Error);
                                Toast.MakeText(this, toasted, ToastLength.Long).Show();
                                return;
                            }



                            foreach (var subject in subjects.Rows)
                            {
                                if (!string.IsNullOrEmpty(subject.StringValue("acSubject")))
                                {
                                    objectExtra.Add(subject.StringValue("acSubject"));
                                }
                            }

                            objectExtra = objectExtra.Distinct().ToList();

                            adapterExtra = new CustomAutoCompleteAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, objectExtra);

                            cbExtra.Adapter = null;
                            cbExtra.Adapter = adapterExtra;
                            adapterExtra.NotifyDataSetChanged();

                        }
                    }
                }
                catch (Exception err)
                {

                    Crashes.TrackError(err);
                    return;

                }

            }
            catch
            {

            }
        }

        private void BtnLogout_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(MainMenuTablet));
            HelpfulMethods.clearTheStack(this);

        }


        private void BtnOrder_Click(object sender, EventArgs e)
        {

            NextStep();

        }

        private void NextStep()
        {
            NameValueObject moveHead = new NameValueObject("MoveHead");
            moveHead.SetString("CurrentFlow", "3");
            moveHead.SetString("DocumentType", currentDocType);
            moveHead.SetString("Wharehouse", currentWarehouse);
            moveHead.SetString("Receiver", currentClient);
            InUseObjects.Set("MoveHead", moveHead);
            // Redirect //
            StartActivity(typeof(ClientPickingTablet));
        }

        private void CbWarehouse_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            try
            {
                temporaryPositionWarehouse = e.Position;
                var wh = objectWarehouse.ElementAt(temporaryPositionWarehouse);
                currentWarehouse = wh.ID;
                FillOpenOrders();
            }
            catch
            {
                return;
            }
        }

        private async void CbExtra_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            currentClient = objectExtra.ElementAt(e.Position);
        }

        private void CbDocType_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            try
            {
                temporaryPositionDoc = e.Position;
                string dw = CommonData.GetSetting("DefaultWarehouse");
                cbWarehouse.SetText(dw, false);
                FillOpenOrders();
            }
            catch (Exception ex)
            {
                string toast = string.Format("Napaka" + ex.ToString());
                Toast.MakeText(this, toast, ToastLength.Long).Show();
            }
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

    }

  

      
     

    }
