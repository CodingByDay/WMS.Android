using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Net;
using Android.Views;
using TrendNET.WMS.Core.Data;
using TrendNET.WMS.Device.App;
using TrendNET.WMS.Device.Services;
using WMS.App;
using static Android.App.ActionBar;
using AlertDialog = Android.App.AlertDialog;
using WebApp = TrendNET.WMS.Device.Services.WebApp;

namespace WMS
{
    [Activity(Label = "PackagingEnteredPositionsView", ScreenOrientation = ScreenOrientation.Portrait)]
    public class PackagingEnteredPositionsView : CustomBaseActivity

    {
        private Dialog popupDialog;
        private TextView lbInfo;
        private EditText tbPackNum;
        private EditText tbSSCC;
        private EditText tbItemCount;
        private EditText tbCreatedBy;
        private Button btNext;
        private Button btUpdate;
        private Button btCreate;
        private Button btDelete;
        private Button btClose;
        private int displayedPosition = 0;
        private NameValueObjectList positions = null;
        private Button btnYes;
        private Button btnNo;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetTheme(Resource.Style.AppTheme_NoActionBar);
            if (App.Settings.tablet)
            {
                base.RequestedOrientation = ScreenOrientation.Landscape;
                base.SetContentView(Resource.Layout.PackagingEnteredPositionsViewTablet);
            }
            else
            {
                base.RequestedOrientation = ScreenOrientation.Portrait;
                base.SetContentView(Resource.Layout.PackagingEnteredPositionsView);
            }
            AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
            var _customToolbar = new CustomToolbar(this, toolbar, Resource.Id.navIcon);
            _customToolbar.SetNavigationIcon(App.Settings.RootURL + "/Services/Logo");
            SetSupportActionBar(_customToolbar._toolbar);
            SupportActionBar.SetDisplayShowTitleEnabled(false);
            lbInfo = FindViewById<TextView>(Resource.Id.lbInfo);
            tbPackNum = FindViewById<EditText>(Resource.Id.tbPackNum);
            tbSSCC = FindViewById<EditText>(Resource.Id.tbSSCC);
            tbItemCount = FindViewById<EditText>(Resource.Id.tbItemCount);
            tbCreatedBy = FindViewById<EditText>(Resource.Id.tbCreatedBy);
            btNext = FindViewById<Button>(Resource.Id.btNext);
            btUpdate = FindViewById<Button>(Resource.Id.btUpdate);
            btCreate = FindViewById<Button>(Resource.Id.btCreate);
            btDelete = FindViewById<Button>(Resource.Id.btDelete);
            btClose = FindViewById<Button>(Resource.Id.btClose);
            btNext.Click += BtNext_Click;
            btUpdate.Click += BtUpdate_Click;
            btDelete.Click += BtDelete_Click;
            btCreate.Click += BtCreate_Click;
            btClose.Click += BtClose_Click;
            // LoadPosition()
            LoadPositions();

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
                    SentrySdk.CaptureException(err);
                }
            }
            else
            {
                LoaderManifest.LoaderManifestLoop(this);
            }
        }

        private void BtClose_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(MainMenu));
            Finish();
        }

        private void BtCreate_Click(object sender, EventArgs e)
        {
            InUseObjects.Set("PackagingHead", null);
            StartActivity(typeof(PackagingSetContext));
            Finish();
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

        private async void BtnYes_Click(object sender, EventArgs e)
        {
            var item = positions.Items[displayedPosition];
            var id = item.GetInt("HeadID");

            try
            {

                var (success, result) = await WebApp.GetAsync("mode=delPackHead&head=" + id.ToString(), this);
                if (success)
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



                        AlertDialog.Builder alert = new AlertDialog.Builder(this);
                        alert.SetTitle($"{Resources.GetString(Resource.String.s265)}");
                        alert.SetMessage($"{Resources.GetString(Resource.String.s212)}" + result);

                        alert.SetPositiveButton("Ok", (senderAlert, args) =>
                        {
                            alert.Dispose();                            
                        });

                        Dialog dialog = alert.Create();
                        dialog.Show();
                    }
                }
                else
                {
                    string toastError = string.Format($"{Resources.GetString(Resource.String.s213)}" + result);
                    Toast.MakeText(this, toastError, ToastLength.Long).Show();
                    
                    return;
                }
            }
            catch (Exception err)
            {

                SentrySdk.CaptureException(err);
                return;

            }
        }

        private void BtUpdate_Click(object sender, EventArgs e)
        {
            var item = positions.Items[displayedPosition];
            InUseObjects.Set("PackagingHead", item);
            StartActivity(typeof(PackagingUnitList));
            Finish();
        }




        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            switch (keyCode)
            {
                // In smartphone
                case Keycode.F1:

                    // next
                    BtNext_Click(this, null);
                    break;


                case Keycode.F2:

                    // edit
                    BtUpdate_Click(this, null);
                    break;


                case Keycode.F3:

                    // create
                    BtCreate_Click(this, null);
                    break;

                case Keycode.F4:

                    // delete 
                    BtDelete_Click(this, null);
                    break;


                case Keycode.F8:

                    // logout
                    BtClose_Click(this, null);
                    break;



            }
            return base.OnKeyDown(keyCode, e);
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
                        positions = Services.GetObjectList("ph", out error, "");
                        InUseObjects.Set("PackagingHeadPositions", positions);
                    }
                    if (positions == null)
                    {
                        string toast = string.Format($"{Resources.GetString(Resource.String.s213)}" + error);
                        Toast.MakeText(this, toast, ToastLength.Long).Show();
                        return;
                    }
                }

                displayedPosition = 0;
                FillDisplayedItem();
            }
            catch (Exception err)
            {

                SentrySdk.CaptureException(err);
                return;

            }

        }

        private void FillDisplayedItem()
        {
            if ((positions != null) && (displayedPosition < positions.Items.Count))
            {
                var item = positions.Items[displayedPosition];
                lbInfo.Text = $"{Resources.GetString(Resource.String.s92)} (" + (displayedPosition + 1).ToString() + "/" + positions.Items.Count + ")";

                tbPackNum.Text = item.GetInt("HeadID").ToString();
                tbSSCC.Text = item.GetString("SSCC");
                tbItemCount.Text = item.GetInt("ItemCount").ToString();

                var created = item.GetDateTime("Date");
                tbCreatedBy.Text = created == null ? "" : ((DateTime)created).ToString("dd.MM.") + " " + item.GetString("ClerkName");

                btUpdate.Enabled = true;
                btDelete.Enabled = true;

                tbPackNum.Enabled = false;
                tbSSCC.Enabled = false;
                tbItemCount.Enabled = false;
                tbCreatedBy.Enabled = false;



                tbPackNum.SetTextColor(Android.Graphics.Color.Black);
                tbSSCC.SetTextColor(Android.Graphics.Color.Black);
                tbItemCount.SetTextColor(Android.Graphics.Color.Black);
                tbCreatedBy.SetTextColor(Android.Graphics.Color.Black);
            }
            else
            {
                lbInfo.Text = $"{Resources.GetString(Resource.String.s267)}";

                tbPackNum.Text = "";
                tbSSCC.Text = "";
                tbItemCount.Text = "";
                tbCreatedBy.Text = "";

                btUpdate.Visibility = ViewStates.Gone;
                btDelete.Visibility = ViewStates.Gone;
                btNext.Visibility = ViewStates.Gone;


                tbPackNum.Enabled = false;
                tbSSCC.Enabled = false;
                tbItemCount.Enabled = false;
                tbCreatedBy.Enabled = false;



                tbPackNum.SetTextColor(Android.Graphics.Color.Black);
                tbSSCC.SetTextColor(Android.Graphics.Color.Black);
                tbItemCount.SetTextColor(Android.Graphics.Color.Black);
                tbCreatedBy.SetTextColor(Android.Graphics.Color.Black);

            }
        }


    }
}