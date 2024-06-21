using Android.Content;
using Android.Net;
using Android.Views;
using TrendNET.WMS.Device.Services;
using WMS.App;
using WMS.ExceptionStore;
namespace WMS
{
    [Activity(Label = "InventoryMenu")]
    public class InventoryMenu : CustomBaseActivity
    {


        public static string target = App.Settings.device;

        public bool result = Services.isTablet(target); /* Is the device tablet. */
        private Button button1;
        private Button button2;
        private Button button3;
        private Button button4;
        private Button button7;
        private Button logout;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                SetTheme(Resource.Style.AppTheme_NoActionBar);
                ChangeTheOrientation();
                // Create your application here
                SetContentView(Resource.Layout.InventoryMenu);
                button1 = FindViewById<Button>(Resource.Id.button1);
                button2 = FindViewById<Button>(Resource.Id.button2);
                button3 = FindViewById<Button>(Resource.Id.button3);
                button4 = FindViewById<Button>(Resource.Id.button4);
                button7 = FindViewById<Button>(Resource.Id.button7);
                logout = FindViewById<Button>(Resource.Id.logout);
                button1.Click += Button1_Click;
                button2.Click += Button2_Click;
                button3.Click += Button3_Click;
                button4.Click += Button4_Click;
                button7.Click += Button7_Click;
                logout.Click += Logout_Click;

                var _broadcastReceiver = new NetworkStatusBroadcastReceiver();
                _broadcastReceiver.ConnectionStatusChanged += OnNetworkStatusChanged;
                Application.Context.RegisterReceiver(_broadcastReceiver,
                new IntentFilter(ConnectivityManager.ConnectivityAction), ReceiverFlags.NotExported);
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }
        public bool IsOnline()
        {
            try
            {
                var cm = (ConnectivityManager)GetSystemService(ConnectivityService);
                return cm.ActiveNetworkInfo == null ? false : cm.ActiveNetworkInfo.IsConnected;
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
                return false;
            }
        }
        private void OnNetworkStatusChanged(object sender, EventArgs e)
        {
            try
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
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }
        public override void OnBackPressed()
        {
            try
            {
                base.OnBackPressed();
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }



        private void ChangeTheOrientation()
        {
            try
            {
                if (App.Settings.tablet == true)
                {
                    base.RequestedOrientation = Android.Content.PM.ScreenOrientation.Landscape;
                }
                else
                {
                    base.RequestedOrientation = Android.Content.PM.ScreenOrientation.Portrait;

                }
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }
        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            try
            {
                switch (keyCode)
                {
                    // in smartphone
                    case Keycode.F2:
                        if (button1.Enabled == true)
                        {
                            Button1_Click(this, null);
                        }
                        break;
                    //return true;


                    case Keycode.F3:
                        if (button2.Enabled == true)
                        {
                            Button2_Click(this, null);
                        }
                        break;


                    case Keycode.F4:
                        if (button3.Enabled == true)
                        {
                            Button3_Click(this, null);
                        }
                        break;


                    case Keycode.F5:
                        if (button4.Enabled == true)
                        {
                            Button4_Click(this, null);
                        }
                        break;

                    case Keycode.F6:
                        if (button7.Enabled == true)
                        {
                            Button7_Click(this, null);
                        }
                        break;

                    case Keycode.F8:

                        Logout_Click(this, null);
                        break;

                }
                return base.OnKeyDown(keyCode, e);
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
                return false;
            }
        }
        private void Logout_Click(object sender, EventArgs e)
        {
            try
            {
                StartActivity(typeof(MainMenu));
                Finish();
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private void Button7_Click(object sender, EventArgs e)
        {
            try
            {
                StartActivity(typeof(InventoryPrint));
                Finish();
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private void Button4_Click(object sender, EventArgs e)
        {
            try
            {
                StartActivity(typeof(InventoryOpen));
                Finish();
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            try
            {
                StartActivity(typeof(InventoryConfirm));
                Finish();
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            try
            {
                StartActivity(typeof(InventoryProcess));
                Finish();
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            try
            {
                StartActivity(typeof(InventoryOpenDocument));
                Finish();
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }
    }
}