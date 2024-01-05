using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using TrendNET.WMS.Device.App;
using TrendNET.WMS.Core.Data;
using TrendNET.WMS.Device.Services;
using static Android.Widget.AdapterView;
using Android.Content.PM;
using Scanner.App;
using Android.Net;
using Microsoft.AppCenter.Crashes;
/// <summary>
/// 
/// </summary>

namespace Scanner
{
    [Activity(Label = "InterWarehouseBusinessEventSetup", ScreenOrientation = ScreenOrientation.Portrait)]
    public class InterWarehouseBusinessEventSetup : Activity
    {
        private CustomAutoCompleteTextView cbDocType;
        public NameValueObjectList docTypes = null;
        private CustomAutoCompleteTextView cbIssueWH;
        private CustomAutoCompleteTextView cbReceiveWH;
        List<ComboBoxItem> objectDocType = new List<ComboBoxItem>();
        List<ComboBoxItem> objectIssueWH = new List<ComboBoxItem>();
        List<ComboBoxItem> objectReceiveWH = new List<ComboBoxItem>();
        private int temporaryPositionDoc = 0;
        private int temporaryPositionIssue = 0;
        private int temporaryPositionReceive = 0;
        public static bool success = false;
        public static string objectTest;
        private Button confirm;
        private string documentCode;
        private ComboBoxItem def;
        private CustomAutoCompleteAdapter<ComboBoxItem> adapter;
        private CustomAutoCompleteAdapter<ComboBoxItem> adapterIssue;
        private CustomAutoCompleteAdapter<ComboBoxItem> adapterReceive;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.InterWarehouseBusinessEventSetup);
            AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
            var _customToolbar = new CustomToolbar(this, toolbar, Resource.Id.navIcon);
            _customToolbar.SetNavigationIcon(settings.RootURL + "/Services/Logo");
            // Views
            var defDocument = CommonData.GetSetting("DefaultInterWareHouseDocType");
            if (!string.IsNullOrWhiteSpace(defDocument))
            {
                documentCode = defDocument;
            }
            cbDocType = FindViewById<CustomAutoCompleteTextView>(Resource.Id.cbDocType);
            cbIssueWH = FindViewById<CustomAutoCompleteTextView>(Resource.Id.cbIssueWH);
            cbReceiveWH = FindViewById<CustomAutoCompleteTextView>(Resource.Id.cbRecceiveWH);
            objectDocType.Add(new ComboBoxItem { ID = "Default", Text = "Izberite poslovni dogodek." });
            docTypes = CommonData.ListDocTypes("E|");
            docTypes.Items.ForEach(dt =>
            { //       
                objectDocType.Add(new ComboBoxItem { ID = dt.GetString("Code"), Text = dt.GetString("Code") + " " + dt.GetString("Name") });
            });
            /*
             Aditional comment area. */
            adapter = new CustomAutoCompleteAdapter<ComboBoxItem>(this,
            Android.Resource.Layout.SimpleSpinnerItem, objectDocType);

            ///* 
            ///* Documentation for the spinner objects add method with an adapter...
            ///*---------------------------------------------------

            cbDocType.Adapter = adapter;

            // Next thing... var warehouses = CommonData.ListWarehouses();
            // cbIssueWH

            var warehouses = CommonData.ListWarehouses();
            if (warehouses != null)
            {
                warehouses.Items.ForEach(dt =>
                {
                    objectIssueWH.Add(new ComboBoxItem { ID = dt.GetString("Subject"), Text = dt.GetString("Name") });

                    objectReceiveWH.Add(new ComboBoxItem { ID = dt.GetString("Subject"), Text = dt.GetString("Name") });

                });
            }

            adapterIssue = new CustomAutoCompleteAdapter<ComboBoxItem>(this,
            Android.Resource.Layout.SimpleSpinnerItem, objectIssueWH);
            adapterReceive = new CustomAutoCompleteAdapter<ComboBoxItem>(this,
            Android.Resource.Layout.SimpleSpinnerItem, objectReceiveWH);
            cbIssueWH.Adapter = adapterIssue;
            cbReceiveWH.Adapter = adapterReceive;

            Button logout = FindViewById<Button>(Resource.Id.logout);
            logout.Click += Logout_Click;
            confirm = FindViewById<Button>(Resource.Id.btnConfirm);
            confirm.Click += Confirm_Click;


            for (int i = 0; i < objectDocType.Count; i++)
            {
                var current = objectDocType[i];

                if (current.ID == documentCode)
                {
                    temporaryPositionDoc = cbDocType.SetItemByString(documentCode);
                }
            }


            var _broadcastReceiver = new NetworkStatusBroadcastReceiver();
            _broadcastReceiver.ConnectionStatusChanged += OnNetworkStatusChanged;
            Application.Context.RegisterReceiver(_broadcastReceiver,
            new IntentFilter(ConnectivityManager.ConnectivityAction));

            cbDocType.ItemClick += CbDocType_ItemClick;
            cbIssueWH.ItemClick += CbIssueWH_ItemClick;
            cbReceiveWH.ItemClick += CbReceiveWH_ItemClick;
            InitializeAutocompleteControls();
        }



        private void InitializeAutocompleteControls()
        {
            cbDocType.SelectAtPosition(0);

        }


        private void CbReceiveWH_ItemClick(object sender, ItemClickEventArgs e)
        {
            temporaryPositionReceive = e.Position;
        }

        private void CbIssueWH_ItemClick(object sender, ItemClickEventArgs e)
        {
            temporaryPositionIssue = e.Position;
        }

        private void CbDocType_ItemClick(object sender, ItemClickEventArgs e)
        {

            if (e.Position == 0)
            {

                cbIssueWH.Visibility = ViewStates.Invisible;
                cbReceiveWH.Visibility = ViewStates.Invisible;
                confirm.Enabled = false;
                string errorWebApp = string.Format("Poslovni dogodek mora biti izbran.");
                Toast.MakeText(this, errorWebApp, ToastLength.Long).Show();

            }
            else
            {
                cbIssueWH.Visibility = ViewStates.Visible;
                cbReceiveWH.Visibility = ViewStates.Visible;
                confirm.Enabled = true;
                cbIssueWH.Enabled = true;
                cbReceiveWH.Enabled = true;
                if (e.Position != 0)
                {
                    temporaryPositionDoc = e.Position;
                    var id = objectDocType.ElementAt(e.Position).ID;
                    PrefillWarehouses(id);
                }

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
                    if (confirm.Enabled == true)
                    {
                        Confirm_Click(this, null);
                    }
                    break;
                // return true;

                case Keycode.F8:

                    Logout_Click(this, null);
                    break;
            }
            return base.OnKeyDown(keyCode, e);
        }

        private void PrefillWarehouses(string id)
        {

            if (string.IsNullOrEmpty(id)) { return; }
            var dt = docTypes.Items.FirstOrDefault(x => x.GetString("Code") == id);
            if (dt != null)
            {
                temporaryPositionIssue = cbIssueWH.SetItemByString(dt.GetString("IssueWarehouse"));
                temporaryPositionReceive = cbReceiveWH.SetItemByString(dt.GetString("ReceiveWarehouse"));
                cbIssueWH.Enabled = dt.GetBool("CanChangeIssueWarehouse");
                cbReceiveWH.Enabled = dt.GetBool("CanChangeReceiveWarehouse");
            }
        }



        private void Confirm_Click(object sender, EventArgs e)
        {

            if (temporaryPositionDoc == 0)
            {
                Toast.MakeText(this, "Morate izbrati dokument", ToastLength.Long).Show();
                return;
            }
            if (temporaryPositionDoc == -1 || temporaryPositionIssue == -1 || temporaryPositionReceive == -1)
            {
                cbIssueWH.Enabled = true;
                cbReceiveWH.Enabled = true;
            }
            var dt = adapter.GetItem(temporaryPositionDoc);
            var iwh = adapterIssue.GetItem(temporaryPositionIssue);
            var rwh = adapterReceive.GetItem(temporaryPositionReceive);
            var doc = dt.ID;
            var issue = iwh.ID;
            var receive = rwh.ID;        
            NameValueObject moveHead = (NameValueObject)InUseObjects.Get("MoveHead");
            moveHead.SetString("DocumentType", doc);
            moveHead.SetString("Type", "E");
            moveHead.SetString("Issuer", issue);
            moveHead.SetString("Receiver", receive);
            moveHead.SetString("LinkKey", "");
            moveHead.SetInt("LinkNo", 0);
            moveHead.SetInt("Clerk", Services.UserID());             
            string error;
     
            try
            {
                
                var savedMoveHead = Services.SetObject("mh", moveHead, out error);
                if (savedMoveHead == null)
                {
                    string errorWebApp = string.Format("Napaka pri dostopu do web aplikacije:" + error );
                    DialogHelper.ShowDialogError(this, this, errorWebApp);


                }
                else
                {
                    if (!Services.TryLock("MoveHead" + savedMoveHead.GetInt("HeadID").ToString(), out error))
                    {
                        string errorWebApp = string.Format("Kritična napaka pri zaklepanju nove medskladiščnice: " + error);
                        DialogHelper.ShowDialogError(this, this, errorWebApp);

                        // Toast.MakeText(this, errorWebApp, ToastLength.Long).Show();
                        // show a message
                        // terminate
                    }

                    moveHead.SetInt("HeadID", savedMoveHead.GetInt("HeadID"));
                    moveHead.SetBool("Saved", true);
                    InUseObjects.Set("MoveHead", moveHead);
                }

                StartActivity(typeof(InterWarehouseSerialOrSSCCEntry));
                HelpfulMethods.clearTheStack(this);

            } catch(Exception errorL)
            {
                string errorWebApp = string.Format("Napaka pri dostopu do web aplikacije:" + errorL.Message);
                DialogHelper.ShowDialogError(this, this, errorWebApp);

            }
            finally
            {
                success = true;
            }   
        }
        




        private void CbReceiveWH_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            Spinner spinner = (Spinner)sender;
                temporaryPositionReceive = e.Position;           
        }

        private void CbIssueWH_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            Spinner spinner = (Spinner)sender;            
                temporaryPositionIssue = e.Position;
            
        }

        private void CbDocType_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            // avoids Default value selection.
            if(e.Position == 0)
            {

                cbIssueWH.Visibility = ViewStates.Invisible;
                cbReceiveWH.Visibility = ViewStates.Invisible;
                confirm.Enabled = false;
                string errorWebApp = string.Format("Poslovni dogodek mora biti izbran.");
                Toast.MakeText(this, errorWebApp, ToastLength.Long).Show();

            } else {
            cbIssueWH.Visibility = ViewStates.Visible;
            cbReceiveWH.Visibility = ViewStates.Visible;
            confirm.Enabled = true;
            cbIssueWH.Enabled = true;
            cbReceiveWH.Enabled = true;
            Spinner spinner = (Spinner)sender;
                if (e.Position != 0)
                {
                    string toast = string.Format("Izbrali ste: {0}", spinner.GetItemAtPosition(e.Position));
                    temporaryPositionDoc = e.Position;
                    var id = objectDocType.ElementAt(e.Position).ID;
                    PrefillWarehouses(id);
                }

            }
        }

        private void Logout_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(MainMenu));
            HelpfulMethods.clearTheStack(this);
        }
    }
    
}