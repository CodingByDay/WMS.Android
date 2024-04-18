using Stream = Android.Media.Stream;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using TrendNET.WMS.Core.Data;
using TrendNET.WMS.Device.App;
using TrendNET.WMS.Device.Services;
using WebApp = TrendNET.WMS.Device.Services.WebApp;

using static Android.App.ActionBar;

using Android.Content.PM;
using Android.Graphics;
using WMS.App;
using Android.Net;
using Microsoft.AppCenter.Crashes;

using AndroidX.AppCompat.App;
using AlertDialog = Android.App.AlertDialog;
using Android.Graphics.Drawables;
namespace WMS
{
    [Activity(Label = "UnfinishedTakeoversView", ScreenOrientation = ScreenOrientation.Portrait)]
    public class UnfinishedTakeoversView : CustomBaseActivity, ISwipeListener
    {
        private EditText tbBusEvent;
        private EditText tbOrder;
        private EditText tbSupplier;
        private EditText tbItemCount;
        private EditText tbCreatedBy;
        private EditText tbCreatedAt;
        private Button btNext;
        private Button btFinish;
        private Button btDelete;
        private Button btLogout;
        private TextView lbInfo;
        private Dialog popupDialog;
        private int displayedPosition = 0;
        private Button btnYes;
        private Button btnConfirm;
        private Button btnNo;
        private Button btNew;
        private ListView dataList;
        private NameValueObjectList positions = (NameValueObjectList)InUseObjects.Get("TakeOverHeads");
        private List<UnfinishedTakeoverList> dataSource = new List<UnfinishedTakeoverList>();
        private GestureDetector gestureDetector;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetTheme(Resource.Style.AppTheme_NoActionBar);
            SetContentView(Resource.Layout.UnfinishedTakeoversView);
            AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
            var _customToolbar = new CustomToolbar(this, toolbar, Resource.Id.navIcon);
            _customToolbar.SetNavigationIcon(settings.RootURL + "/Services/Logo");
            SetSupportActionBar(_customToolbar._toolbar);
            SupportActionBar.SetDisplayShowTitleEnabled(false);
            tbBusEvent = FindViewById<EditText>(Resource.Id.tbBusEvent);
            tbOrder = FindViewById<EditText>(Resource.Id.tbOrder);
            tbSupplier = FindViewById<EditText>(Resource.Id.tbSupplier);
            tbItemCount = FindViewById<EditText>(Resource.Id.tbItemCount);
            tbCreatedBy = FindViewById<EditText>(Resource.Id.tbCreatedBy);
            tbCreatedAt = FindViewById<EditText>(Resource.Id.tbCreatedAt);

            btNext = FindViewById<Button>(Resource.Id.btNext);
            btFinish = FindViewById<Button>(Resource.Id.btFinish);
            btDelete = FindViewById<Button>(Resource.Id.btDelete);
            btNew = FindViewById<Button>(Resource.Id.btnew);
            btLogout = FindViewById<Button>(Resource.Id.logout);
            lbInfo = FindViewById<TextView>(Resource.Id.lbInfo);
            btFinish.Click += BtFinish_Click;
            btNext.Click += BtNext_Click;
            btDelete.Click += BtDelete_Click;
            btNew.Click += BtNew_Click;
            btLogout.Click += BtLogout_Click;

            InUseObjects.Clear();

            LoadPositions();


            // Try to get the bitmap
            GestureListener gestureListener = new GestureListener(this);
            gestureDetector = new GestureDetector(this, new GestureListener(this));

            LinearLayout yourLinearLayout = FindViewById<LinearLayout>(Resource.Id.fling);
            // Initialize the GestureDetector
            yourLinearLayout.SetOnTouchListener(gestureListener);

            var _broadcastReceiver = new NetworkStatusBroadcastReceiver();
            _broadcastReceiver.ConnectionStatusChanged += OnNetworkStatusChanged;
            Application.Context.RegisterReceiver(_broadcastReceiver,
            new IntentFilter(ConnectivityManager.ConnectivityAction));
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

                //return true;


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
                    if (btNew.Enabled == true)
                    {
                        BtNew_Click(this, null);
                    }
                    break;


                case Keycode.F8:
                    if (btLogout.Enabled == true)
                    {
                        BtLogout_Click(this, null);
                    }
                    break;


        
            }
            return base.OnKeyDown(keyCode, e);
        }
        private void BtLogout_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(MainMenu));
        }

        private void BtNew_Click(object sender, EventArgs e)
        {
            NameValueObject moveHead = new NameValueObject("MoveHead");
            moveHead.SetBool("Saved", false);
            InUseObjects.Set("MoveHead", moveHead);

            StartActivity(typeof(TakeOverBusinessEventSetup));
            HelpfulMethods.clearTheStack(this);
        }

        private void BtDelete_Click(object sender, EventArgs e)
        {
            popupDialog = new Dialog(this);
            popupDialog.SetContentView(Resource.Layout.YesNoPopUp);
            popupDialog.Window.SetSoftInputMode(SoftInput.AdjustResize);
            popupDialog.Show();

            popupDialog.Window.SetLayout(LayoutParams.MatchParent, LayoutParams.WrapContent);
            popupDialog.Window.SetBackgroundDrawable(new ColorDrawable(Color.ParseColor("#081a45")));

            // Access Popup layout fields like below
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
                        string errorWebApp = string.Format($"{Resources.GetString(Resource.String.s212)}" + result);
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

                    string errorWebApp = string.Format($"{Resources.GetString(Resource.String.s216)}" + result);
                    Toast.MakeText(this, errorWebApp, ToastLength.Long).Show();
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
            moveHead.SetBool("Saved", true);
            InUseObjects.Set("MoveHead", moveHead);

            StartActivity(typeof(TakeOverEnteredPositionsView));
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
                        positions = Services.GetObjectList("mh", out error, "I");
                        InUseObjects.Set("TakeOverHeads", positions);
                    }
                    if (positions == null)
                    {
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
                lbInfo.Text = $"{Resources.GetString(Resource.String.s12)} (" + (displayedPosition + 1).ToString() + "/" + positions.Items.Count + ")";
                var item = positions.Items[displayedPosition];

                tbBusEvent.Text = item.GetString("DocumentTypeName");
                tbOrder.Text = item.GetString("LinkKey");
                tbSupplier.Text = item.GetString("Issuer");
                tbItemCount.Text = item.GetInt("ItemCount").ToString();
                tbCreatedBy.Text = item.GetString("ClerkName");

                var created = item.GetDateTime("DateInserted");
                tbCreatedAt.Text = created == null ? "" : ((DateTime)created).ToString("dd.MM.yyyy");

                tbBusEvent.Enabled = false;
                tbOrder.Enabled = false;
                tbSupplier.Enabled = false;
                tbItemCount.Enabled = false;
                tbCreatedBy.Enabled = false;
                tbCreatedAt.Enabled = false;


                tbBusEvent.SetTextColor(Android.Graphics.Color.Black);
                tbOrder.SetTextColor(Android.Graphics.Color.Black);
                tbSupplier.SetTextColor(Android.Graphics.Color.Black);
                tbItemCount.SetTextColor(Android.Graphics.Color.Black);
                tbCreatedBy.SetTextColor(Android.Graphics.Color.Black);
                tbCreatedAt.SetTextColor(Android.Graphics.Color.Black);


                btNext.Enabled = true;
                btDelete.Enabled = true;
                btFinish.Enabled = true;
            }
            else
            {
                lbInfo.Text = $"{Resources.GetString(Resource.String.s331)}";

                tbBusEvent.Text = "";
                tbOrder.Text = "";
                tbSupplier.Text = "";
                tbItemCount.Text = "";
                tbCreatedBy.Text = "";
                tbCreatedAt.Text = "";

                tbBusEvent.Enabled = false;
                tbOrder.Enabled = false;
                tbSupplier.Enabled = false;
                tbItemCount.Enabled = false;
                tbCreatedBy.Enabled = false;
                tbCreatedAt.Enabled = false;




                tbBusEvent.SetTextColor(Android.Graphics.Color.Black);
                tbOrder.SetTextColor(Android.Graphics.Color.Black);
                tbSupplier.SetTextColor(Android.Graphics.Color.Black);
                tbItemCount.SetTextColor(Android.Graphics.Color.Black);
                tbCreatedBy.SetTextColor(Android.Graphics.Color.Black);
                tbCreatedAt.SetTextColor(Android.Graphics.Color.Black);

                btNext.Enabled = false;
                btDelete.Enabled = false;
                btFinish.Enabled = false;
            }
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
    }
}