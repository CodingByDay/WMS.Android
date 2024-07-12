using Android.Content;
using Android.Content.PM;
using Android.Net;
using Android.Views;
using TrendNET.WMS.Core.Data;
using WMS.App;
using WMS.ExceptionStore;
using WMS.Printing;
namespace WMS
{
    [Activity(Label = "WMS")]
    public class PrintingSSCCCodes : CustomBaseActivity
    {
        private EditText tbNum;
        private Button button1;
        private Button button2;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                SetTheme(Resource.Style.AppTheme_NoActionBar);
                if (App.Settings.tablet)
                {
                    base.RequestedOrientation = ScreenOrientation.Landscape;
                    base.SetContentView(Resource.Layout.PrintingSSCCCodesTablet);
                }
                else
                {
                    base.RequestedOrientation = ScreenOrientation.Portrait;
                    base.SetContentView(Resource.Layout.PrintingSSCCCodes);
                }
                AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
                var _customToolbar = new CustomToolbar(this, toolbar, Resource.Id.navIcon);
                _customToolbar.SetNavigationIcon(App.Settings.RootURL + "/Services/Logo");
                SetSupportActionBar(_customToolbar._toolbar);
                SupportActionBar.SetDisplayShowTitleEnabled(false);
                tbNum = FindViewById<EditText>(Resource.Id.tbNum);
                button1 = FindViewById<Button>(Resource.Id.button1);
                button2 = FindViewById<Button>(Resource.Id.button2);
                button1.Click += Button1_Click;
                button2.Click += Button2_Click;
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
        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            try
            {
                switch (keyCode)
                {
                    // in smartphone
                    case Keycode.F3:
                        if (button1.Enabled == true)
                        {
                            Button1_Click(this, null);
                        }
                        break;
                    // return true;


                    case Keycode.F8:
                        if (button2.Enabled == true)
                        {
                            Button2_Click(this, null);
                        }
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
        private void Button2_Click(object sender, EventArgs e)
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

        private void Button1_Click(object sender, EventArgs e)
        {
            try
            {
                var num = -1;
                try
                {
                    num = Convert.ToInt32(tbNum.Text);
                }
                catch (Exception ex)
                {
                    Toast.MakeText(this, $"{Resources.GetString(Resource.String.s301)} (" + ex.Message + ")", ToastLength.Long).Show();

                    return;
                }
                if (num < 1)
                {
                    Toast.MakeText(this, $"{Resources.GetString(Resource.String.s302)}", ToastLength.Long).Show();

                    return;
                }
                try
                {

                    var nvo = new NameValueObject("PrintSSCC");
                    PrintingCommon.SetNVOCommonData(ref nvo);
                    nvo.SetInt("Copies", num);
                    PrintingCommon.SendToServer(nvo);
                    Toast.MakeText(this, $"{Resources.GetString(Resource.String.s299)}", ToastLength.Long).Show();
                }
                catch (Exception err)
                {
                    SentrySdk.CaptureException(err);
                    return;
                }
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }
    }
}