using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Widget;
using Android.Views;
using Android.Net;
using System.Net;
using Stream = Android.Media.Stream;
using Android.Util;
using Android.Content;
using Plugin.Settings.Abstractions;
using System.Linq;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using static Android.App.ActionBar;
using Microsoft.AppCenter.Distribute;
using Uri = System.Uri;
using System.Threading.Tasks;
using AlertDialog = Android.App.AlertDialog;
using Square.Picasso;
using Aspose.Words.Tables;
using System.Diagnostics;
using AndroidX.AppCompat.App;
using Android;
using TrendNET.WMS.Device.Services;
using WMS.App;
using WMS.Background;
using AndroidX.AppCompat.App;
using FFImageLoading;
using Android.Graphics.Drawables;
using Android.Graphics;
using Android.Preferences;
using Newtonsoft.Json;
using Xamarin.Essentials;
using AndroidX.Core.Content;
namespace WMS
{

    [Activity(Label = "WMS", Theme = "@style/AppTheme", MainLauncher = true, Icon = "@drawable/barcode", NoHistory = true)]
    public class MainActivity : CustomBaseActivity
    {
        private Dialog popupDialog;
        public static bool isValid;
        private EditText Password;
        public static ProgressBar progressBar1;
        private Button ok;
        private EditText rootURL;
        private EditText ID;
        private ImageView img;
        private TextView? txtVersion;
        private LinearLayout? chSlovenian;
        private LinearLayout? chEnglish;
        private ImageView? imgSlovenian;
        private ImageView? imgEnglish;
        private TextView deviceURL;
        private bool tablet = settings.tablet;
        private Button btnOkRestart;
        private ListView? cbLanguage;
        private List<LanguageItem> mLanguageItems;
        private LanguageAdapter mAdapter;
        private ColorMatrixColorFilter highlightFilter;

        public object MenuInflaterFinal { get; private set; }

        public bool IsOnline()
        {
            var cm = (ConnectivityManager)GetSystemService(ConnectivityService);
            return cm.ActiveNetworkInfo == null ? false : cm.ActiveNetworkInfo.IsConnected;
        }


        private void ProcessRegistration()
        {
     

            var id = settings.ID.ToString();
            string result;


            if (WebApp.Get("mode=deviceActive", out result))
            {
                if (result != "Active!")
                {
                    Toast.MakeText(this, $"{Resources.GetString(Resource.String.s294)}", ToastLength.Long).Show();
                    return;
                }

                var inactivity = new Intent(this, typeof(Inactivity));
                StartService(inactivity);
                if (IsOnline())
                {
                    if (string.IsNullOrEmpty(Password.Text.Trim())) { return; }

                    Services.ClearUserInfo();
                    string error;
                    bool valid = false;

                    try
                    {
                        valid = Services.IsValidUser(Password.Text.Trim(), out error);
                    }
                    catch (Exception err)
                    {

                        SentrySdk.CaptureException(err);
                        return;

                    }
                    if (valid)
                    {
                        Analytics.TrackEvent("Valid login");
                        if (Services.HasPermission("TNET_WMS", "R"))
                        {

                            StartActivity(typeof(MainMenu));
                            HelpfulMethods.clearTheStack(this);
                            
                            Password.Text = "";
                            isValid = true;
                            this.Finish();
                        }
                        else
                        {
                            Analytics.TrackEvent("Invalid permissions");
                            Password.Text = "";
                            isValid = false;
                            string toast = new string($"{Resources.GetString(Resource.String.s295)}");
                            Toast.MakeText(this, toast, ToastLength.Long).Show();
                            progressBar1.Visibility = ViewStates.Invisible;
                        }
                    }
                    else
                    {
                        Password.Text = "";
                        isValid = false;
                        string toast = new string($"{Resources.GetString(Resource.String.s296)}");
                        Toast.MakeText(this, toast, ToastLength.Long).Show();
                        progressBar1.Visibility = ViewStates.Invisible;
                    }
                }
                else
                {
                    // Is connected 
                    string toast = new string($"{Resources.GetString(Resource.String.s297)}");
                    Toast.MakeText(this, toast, ToastLength.Long).Show();
                    progressBar1.Visibility = ViewStates.Invisible;

                }
            }
        }




        protected override void OnPause()
        {
            base.OnPause();
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {

            settings.restart = false;
            Distribute.SetEnabledAsync(true);
            AppCenter.Start("ec2ca4ce-9e86-4620-9e90-6ecc5cda0e0e",
            typeof(Analytics), typeof(Crashes), typeof(Distribute));
            AppCenter.SetUserId(settings.RootURL);
            Crashes.NotifyUserConfirmation(UserConfirmation.AlwaysSend);
            ChangeTheOrientation();
            base.OnCreate(savedInstanceState);
            SentryXamarin.Init(o =>
            {
                // Tells which project in Sentry to send events to:
                o.Dsn = "https://4da007db4594a10f53ab292097e612f8@o4507304617836544.ingest.de.sentry.io/4507304993751120";
                // When configuring for the first time, to see what the SDK is doing:
                o.Debug = true;
                // Set TracesSampleRate to 1.0 to capture 100%
                // of transactions for performance monitoring.
                // We recommend adjusting this value in production
                o.TracesSampleRate = 1.0;
            });

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);
            Distribute.ReleaseAvailable = OnReleaseAvailable;
            Password = FindViewById<EditText>(Resource.Id.tbPassword);
            Password.InputType = Android.Text.InputTypes.NumberVariationPassword |
            Android.Text.InputTypes.ClassNumber;
            progressBar1 = FindViewById<ProgressBar>(Resource.Id.progressBar1);
            img = FindViewById<ImageView>(Resource.Id.imglogo);
            txtVersion = FindViewById<TextView>(Resource.Id.txtVersion);
            chSlovenian = FindViewById<LinearLayout>(Resource.Id.chSlovenian);
            chEnglish = FindViewById<LinearLayout>(Resource.Id.chSlovenian);
            imgSlovenian = FindViewById<ImageView>(Resource.Id.imgSlovenian);
            imgEnglish = FindViewById<ImageView>(Resource.Id.imgEnglish);
            SetUpLanguages();
            GetLogo();
            var _broadcastReceiver = new NetworkStatusBroadcastReceiver();
            _broadcastReceiver.ConnectionStatusChanged += OnNetworkStatusChanged;
            Application.Context.RegisterReceiver(_broadcastReceiver,
            new IntentFilter(ConnectivityManager.ConnectivityAction));
            Button btnRegistrationEvent = FindViewById<Button>(Resource.Id.btnRegistrationClick);
            btnRegistrationEvent.Clickable = true;
            btnRegistrationEvent.Enabled = true;
            btnRegistrationEvent.Click += BtnRegistrationEvent_Click;
            settings.login = false;
        }

        public string GetAppVersion()
        {
            return AppInfo.VersionString;
        }

        private void SetUpLanguages()
        {
            ISharedPreferences sharedPreferences = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
            ISharedPreferencesEditor editor = sharedPreferences.Edit();

            // Create a color matrix for the highlight effect
            float[] colorMatrixValues = {
                2, 0, 0, 0, 0, // Red
                0, 2, 0, 0, 0, // Green
                0, 0, 2, 0, 0, // Blue
                0, 0, 0, 1, 0  // Alpha
            };

            ColorMatrix colorMatrix = new ColorMatrix(colorMatrixValues);
            highlightFilter = new ColorMatrixColorFilter(colorMatrix);
            txtVersion.Text = "v." + GetAppVersion();
            var language = Resources.Configuration.Locale.Country;      
            
            if(language == "SI")
            {
                imgSlovenian.SetColorFilter(highlightFilter);
                Base.Store.language = "sl";
            }
            else if(language == "US")
            {
                imgEnglish.SetColorFilter(highlightFilter);
                Base.Store.language = "en";
            }
        }

        private void GetLogo()
        {
            try
            {
                var url = settings.RootURL + "/Services/Logo";
                // Load and set the image with Picasso
                Picasso.Get()
                    .Load(url)
                    .Into(img);
            }
            catch 
            {
              return;
            }
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
        public override bool DispatchKeyEvent(Android.Views.KeyEvent e)
        {
            if (e.KeyCode == Keycode.Enter) { BtnRegistrationEvent_Click(this, null); }
            return base.DispatchKeyEvent(e);
        }
        private void ChangeTheOrientation()
        {
            if (settings.tablet == true)
            {
                RequestedOrientation = Android.Content.PM.ScreenOrientation.Landscape;
            }
            else
            {
                RequestedOrientation = Android.Content.PM.ScreenOrientation.Portrait;
            }
        }

        private bool OnReleaseAvailable(ReleaseDetails releaseDetails)
        {
            try
            {
                string versionName = releaseDetails.ShortVersion;
                string versionCodeOrBuildNumber = releaseDetails.Version;
                string releaseNotes = releaseDetails.ReleaseNotes;
                Uri releaseNotesUrl = releaseDetails.ReleaseNotesUrl;
                var title = "Version " + versionName + " available!";
                popupDialog = new Dialog(this);
                popupDialog.SetContentView(Resource.Layout.update);
                popupDialog.Window.SetSoftInputMode(SoftInput.AdjustResize);
                popupDialog.Show();
                popupDialog.Window.SetLayout(LayoutParams.MatchParent, LayoutParams.WrapContent);
                popupDialog.Window.SetBackgroundDrawable(new ColorDrawable(Color.ParseColor("#081a45")));

                // Access Pop-up layout fields like below
                btnOkRestart = popupDialog.FindViewById<Button>(Resource.Id.btnOk);
                btnOkRestart.Click += BtnOk_Click;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            Distribute.NotifyUpdateAction(UpdateAction.Update);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.action_setting1:
                    {
                        Finish();
                        StartActivity(typeof(Settings));
                        HelpfulMethods.clearTheStack(this);
                        return true;
                    }

            }

            return base.OnOptionsItemSelected(item);
        }
        private void Listener_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(Settings));
            HelpfulMethods.clearTheStack(this);
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Layout.popup_action, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        private void BtnRegistrationEvent_Click(object sender, System.EventArgs e)
        {
            progressBar1.Visibility = ViewStates.Visible;
            ProcessRegistration();
        }

        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            switch (keyCode)
            {
                // in smart-phone
                case Keycode.Enter:
                    BtnRegistrationEvent_Click(this, null);
                    break;
                    // return true;
            }
            return base.OnKeyDown(keyCode, e);
        }


        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}