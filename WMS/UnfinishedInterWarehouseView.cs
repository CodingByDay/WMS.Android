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

namespace Scanner
{
    [Activity(Label = "UnfinishedInterWarehouseView", ScreenOrientation = ScreenOrientation.Portrait)]
    public class UnfinishedInterWarehouseView : Activity, ISwipeListener

    {

 
        private TextView lbInfo;
        private EditText tbBusEvent;
        private EditText tbIssueWarehouse;
        private EditText tbReceiveWarehouse;
        private EditText tbItemCount;
        private EditText tbCreatedBy;
        private EditText tbCreatedAt;
        private Button btNext;
        private Button btFinish;
        private Button btDelete;
        private Button btnNew;
        private Button btnLogout;

/// <summary>
/// ////////////////
/// </summary>

        private int displayedPosition = 0;
        private NameValueObjectList positions = (NameValueObjectList)InUseObjects.Get("InterWarehouseHeads");
        private Dialog popupDialog;
        private Button btnYes;
        private Button btnNo;
        private GestureDetector gestureDetector;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.UnfinishedInterWarehouseView);
            AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
            var _customToolbar = new CustomToolbar(this, toolbar, Resource.Id.navIcon);
            _customToolbar.SetNavigationIcon(settings.RootURL + "/Services/Logo");
            lbInfo = FindViewById<TextView>(Resource.Id.lbInfo);
            tbBusEvent = FindViewById<EditText>(Resource.Id.tbBusEvent);
            tbIssueWarehouse = FindViewById<EditText>(Resource.Id.tbIssueWarehouse);
            tbReceiveWarehouse = FindViewById<EditText>(Resource.Id.tbReceiveWarehouse);
            tbItemCount = FindViewById<EditText>(Resource.Id.tbItemCount);
            tbCreatedBy = FindViewById<EditText>(Resource.Id.tbCreatedBy);
            tbCreatedAt = FindViewById<EditText>(Resource.Id.tbCreatedAt);
            btNext = FindViewById<Button>(Resource.Id.btNext);
            btFinish = FindViewById<Button>(Resource.Id.btFinish);
            btnNew = FindViewById<Button>(Resource.Id.btnNew);
            btnLogout = FindViewById<Button>(Resource.Id.btnLogout);
            btDelete = FindViewById<Button>(Resource.Id.btDelete);
       
            btNext.Click += BtNext_Click;
            btFinish.Click += BtFinish_Click;
            btDelete.Click += BtDelete_Click;
            btnNew.Click += BtnNew_Click;
            btnLogout.Click += BtnLogout_Click;
            InUseObjects.Clear();

            LoadPositions();

            var _broadcastReceiver = new NetworkStatusBroadcastReceiver();
            _broadcastReceiver.ConnectionStatusChanged += OnNetworkStatusChanged;
            Application.Context.RegisterReceiver(_broadcastReceiver,
            new IntentFilter(ConnectivityManager.ConnectivityAction));



            GestureListener gestureListener = new GestureListener(this);
            gestureDetector = new GestureDetector(this, new GestureListener(this));

            LinearLayout yourLinearLayout = FindViewById<LinearLayout>(Resource.Id.fling);
            // Initialize the GestureDetector
            yourLinearLayout.SetOnTouchListener(gestureListener);
        }


        public void OnSwipeLeft()
        {
            displayedPosition--;
            if (displayedPosition < 0) { displayedPosition = positions.Items.Count - 1; }
            FillDisplayedItem();
        }

        public void OnSwipeRight()
        {
            displayedPosition++;
            if (displayedPosition >= positions.Items.Count) { displayedPosition = 0; }
            FillDisplayedItem();
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


        public override void OnBackPressed()
        {

            HelpfulMethods.releaseLock();

            base.OnBackPressed();
        }
        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            switch (keyCode)
            {
                // in smartphone
                case Keycode.F1:
                    if (btNext.Enabled == true)
                    {
                        BtNext_Click(this, null);
                    }
                    break;
                // return true;
                case Keycode.F2:
                    if (btFinish.Enabled == true)
                    {
                        BtFinish_Click(this, null);
                    }
                    break;
                case Keycode.F3:
                    if (btDelete.Enabled == true)
                    {
                        BtDelete_Click(this, null);
                    }
                    break;
                case Keycode.F4:
                    if (btnNew.Enabled == true)
                    {
                        BtnNew_Click(this, null);
                    }
                    break;
                case Keycode.F5:
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
        }
        
        private void BtnNew_Click(object sender, EventArgs e)
        {
            NameValueObject moveHead = new NameValueObject("MoveHead");
            moveHead.SetBool("Saved", false);
            InUseObjects.Set("MoveHead", moveHead);

           StartActivity(typeof(InterWarehouseBusinessEventSetup));
            HelpfulMethods.clearTheStack(this);
        }

        private void BtDelete_Click(object sender, EventArgs e)
        {
            popupDialog = new Dialog(this);
            popupDialog.SetContentView(Resource.Layout.YesNoPopUp);
            popupDialog.Window.SetSoftInputMode(SoftInput.AdjustResize);
            popupDialog.Show();

            popupDialog.Window.SetLayout(LayoutParams.MatchParent, LayoutParams.WrapContent);
            popupDialog.Window.SetBackgroundDrawableResource(Android.Resource.Color.HoloBlueBright);

            btnYes = popupDialog.FindViewById<Button>(Resource.Id.btnYes);
            btnNo = popupDialog.FindViewById<Button>(Resource.Id.btnNo);

            btnYes.Click += BtnYes_Click;
            btnNo.Click += BtnNo_Click;
          
        }

        private void BtnNo_Click(object sender, EventArgs e)
        {
            popupDialog.Dismiss();
            popupDialog.Hide();
        }

        private void BtnYes_Click(object sender, EventArgs e)
        {
            var item = positions.Items[displayedPosition];
            var id = item.GetInt("HeadID");

         
            try
            {
          
                string result;
                if (WebApp.Get("mode=delMoveHead&head=" + id.ToString() + "&deleter=" + Services.UserID().ToString(), out result))
                {
                    if (result == "OK!")
                    {
                        positions = null;
                        LoadPositions();
                        popupDialog.Dismiss();
                        popupDialog.Hide();

                    }
                    else
                    {
                        string errorWebApp = string.Format("Napaka pri brisanju pozicije " + result);
                        Toast.MakeText(this, errorWebApp, ToastLength.Long).Show();
                        positions = null;
                        LoadPositions();
                        popupDialog.Dismiss();
                        popupDialog.Hide();
                        return;

                    }
                }
                else
                {
                    string errorWebApp = string.Format("Napaka pri dostopu do web aplikacije. " + result);
                    Toast.MakeText(this, errorWebApp, ToastLength.Long).Show();
                    System.Diagnostics.Process.GetCurrentProcess().Kill();
                    popupDialog.Dismiss();
                    popupDialog.Hide();
                    return;
                }
            }
            finally
            {
                popupDialog.Dismiss();
                popupDialog.Hide();
            }


        }

        private void BtFinish_Click(object sender, EventArgs e)
        {
            var moveHead = positions.Items[displayedPosition];

            string error;
            if (!Services.TryLock("MoveHead" + moveHead.GetInt("HeadID").ToString(), out error))
            {
                string toast = string.Format("Napaka:" + error);
                Toast.MakeText(this, toast, ToastLength.Long).Show();
                return;
            }

            moveHead.SetBool("Saved", true);
            InUseObjects.Set("MoveHead", moveHead);

            StartActivity(typeof(InterWarehouseEnteredPositionsView));
            HelpfulMethods.clearTheStack(this);

        }

        private void BtNext_Click(object sender, EventArgs e)
        {
            displayedPosition++;
            if (displayedPosition >= positions.Items.Count) { displayedPosition = 0; }
            FillDisplayedItem();
        }

        private void LoadPositions()
        {       
            try
            {
               
                if (positions == null)
                {
                    var error = "";
                    if (positions == null)
                    {
                        try
                        {
                            positions = Services.GetObjectList("mh", out error, "E");
                        }
                         catch (Exception) {

                            var stop = true;
                        
                        }
                        InUseObjects.Set("InterWarehouseHeads", positions);
                    }
                    if (positions == null)
                    {
                        string toast = string.Format("Napaka pri dostopu do web aplikacije" + error);
                        Toast.MakeText(this, toast, ToastLength.Long).Show();
                        return;
                    }
                }

                displayedPosition = 0;
                FillDisplayedItem();
            }
            catch (Exception err)
            {

                Crashes.TrackError(err);
                return;

            }

        }



        private void FillDisplayedItem()
        {
            if ((positions != null) && (positions.Items.Count > 0))
            {
                lbInfo.Text = "Odprte medskladiščnice na čitalcu (" + (displayedPosition + 1).ToString() + "/" + positions.Items.Count + ")";
                var item = positions.Items[displayedPosition];

                tbBusEvent.Text = item.GetString("DocumentTypeName");
                tbIssueWarehouse.Text = item.GetString("Issuer");
                tbReceiveWarehouse.Text = item.GetString("Receiver");
                tbItemCount.Text = item.GetInt("ItemCount").ToString();
                tbCreatedBy.Text = item.GetString("ClerkName");

                var created = item.GetDateTime("DateInserted");
                tbCreatedAt.Text = created == null ? "" : ((DateTime)created).ToString("dd.MM.yyyy");
                tbBusEvent.Enabled = false;
                tbIssueWarehouse.Enabled = false;
                tbReceiveWarehouse.Enabled = false;
                tbItemCount.Enabled = false;
                tbCreatedBy.Enabled = false;
                tbCreatedAt.Enabled = false;


                tbBusEvent.SetTextColor(Android.Graphics.Color.Black);
                tbIssueWarehouse.SetTextColor(Android.Graphics.Color.Black);
                tbReceiveWarehouse.SetTextColor(Android.Graphics.Color.Black);
                tbItemCount.SetTextColor(Android.Graphics.Color.Black);
                tbCreatedBy.SetTextColor(Android.Graphics.Color.Black);
                tbCreatedAt.SetTextColor(Android.Graphics.Color.Black);

                btNext.Enabled = true;
                btDelete.Enabled = true;
                btFinish.Enabled = true;
            }
            else
            {
                lbInfo.Text = "Odprte medskladiščnice na čitalcu (ni)";

                tbBusEvent.Text = "";
                tbIssueWarehouse.Text = "";
                tbReceiveWarehouse.Text = "";
                tbItemCount.Text = "";
                tbCreatedBy.Text = "";
                tbCreatedAt.Text = "";

                tbBusEvent.Enabled = false;
                tbIssueWarehouse.Enabled = false;
                tbReceiveWarehouse.Enabled = false;
                tbItemCount.Enabled = false;
                tbCreatedBy.Enabled = false;
                tbCreatedAt.Enabled = false;


                tbBusEvent.SetTextColor(Android.Graphics.Color.Black);
                tbIssueWarehouse.SetTextColor(Android.Graphics.Color.Black);
                tbReceiveWarehouse.SetTextColor(Android.Graphics.Color.Black);
                tbItemCount.SetTextColor(Android.Graphics.Color.Black);
                tbCreatedBy.SetTextColor(Android.Graphics.Color.Black);
                tbCreatedAt.SetTextColor(Android.Graphics.Color.Black);


                btNext.Enabled = false; 
                btFinish.Enabled = false;
        
                btDelete.Enabled = false;

            }
        }




    }
}