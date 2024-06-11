using Android;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.Net;
using Android.Preferences;
using Android.Views;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using Google.Android.Material.Snackbar;
using Square.Picasso;
using System.Diagnostics;
using TrendNET.WMS.Device.Services;
using WMS.App;
using WMS.Background;
using Xamarin.Essentials;
namespace WMS
{

    [Activity(Label = "WMS", Theme = "@style/AppTheme", MainLauncher = true, Icon = "@drawable/barcode", NoHistory = true)]
    public class MainActivity : CustomBaseActivity
    {
        private Dialog popupDialog;
        public static bool isValid;
        private EditText Password;
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
        private bool tablet = App.Settings.tablet;
        private Button btnOkRestart;
        private ListView? cbLanguage;
        private List<LanguageItem> mLanguageItems;
        private LanguageAdapter mAdapter;
        private ColorMatrixColorFilter highlightFilter;
        private static readonly HttpClient httpClient = new HttpClient();
        const int RequestPermissionsId = 0;
        bool permissionsGranted = true;

        public object MenuInflaterFinal { get; private set; }

        public bool IsOnline()
        {
            var cm = (ConnectivityManager)GetSystemService(ConnectivityService);
            return cm.ActiveNetworkInfo == null ? false : cm.ActiveNetworkInfo.IsConnected;
        }

        private async Task ProcessRegistrationAsync()
        {
            var id = App.Settings.ID.ToString();

            // Using the asynchronous GetAsync method
            var (success, result) = await WebApp.GetAsync("mode=deviceActive");

            if (success)
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


            
                    var valid = await Services.IsValidUserAsync(Password.Text.Trim());
                 

                    if (valid)
                    {
                        if (await Services.HasPermission("TNET_WMS", "R", this))
                        {
                            StartActivity(typeof(MainMenu));
                            Password.Text = "";
                            isValid = true;
                            Finish();
                        }
                        else
                        {
                            Password.Text = "";
                            isValid = false;
                            string toast = $"{Resources.GetString(Resource.String.s295)}";
                            Toast.MakeText(this, toast, ToastLength.Long).Show();
                        }
                    }
                    else
                    {
                        Password.Text = "";
                        isValid = false;
                        string toast = $"{Resources.GetString(Resource.String.s296)}";
                        Toast.MakeText(this, toast, ToastLength.Long).Show();
                    }
                }
                else
                {
                    // Not connected 
                    string toast = $"{Resources.GetString(Resource.String.s297)}";
                    Toast.MakeText(this, toast, ToastLength.Long).Show();
                }
            }
            else
            {
                // Handle failure case
                string toast = $"{Resources.GetString(Resource.String.s297)}";
                Toast.MakeText(this, toast, ToastLength.Long).Show();
            }
        }




        protected async override void OnCreate(Bundle savedInstanceState)
        {
            App.Settings.restart = false;

            ChangeTheOrientation();
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.LoginActivity);

            Password = FindViewById<EditText>(Resource.Id.tbPassword);
            Password.InputType = Android.Text.InputTypes.NumberVariationPassword |
            Android.Text.InputTypes.ClassNumber;
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
            App.Settings.login = false;

            InitializeSentryAsync();

            // Check and request necessary permissions at startup because of Google Play policies. 29.05.2024 Janko Jovièiæ
            // RequestNecessaryPermissions(); // For now not needed. 31.05.2024 Janko Jovièiæ
        }

        void RequestNecessaryPermissions()
        {
            string[] requiredPermissions = new string[]
            {
                Manifest.Permission.Bluetooth
            };

            CheckAndRequestPermissions(requiredPermissions);
        }

        void CheckAndRequestPermissions(string[] permissions)
        {

            var permissionsToRequest = new List<string>();

            foreach (var permission in permissions)
            {
                if (ContextCompat.CheckSelfPermission(this, permission) != (int)Permission.Granted)
                {
                    permissionsToRequest.Add(permission);
                }
            }

            if (permissionsToRequest.Count > 0)
            {
                ActivityCompat.RequestPermissions(this, permissionsToRequest.ToArray(), RequestPermissionsId);
            }


            else
            {
                // All permissions are already granted
                OnAllPermissionsGranted();
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            if (requestCode == RequestPermissionsId)
            {
                bool allGranted = grantResults.Any(x => x == Permission.Granted);

                if (allGranted)
                {
                    OnAllPermissionsGranted();
                }
                else
                {
                    // Handle the case where permissions are not granted
                    OnPermissionsDenied();
                }
            }


            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        void OnAllPermissionsGranted()
        {
            // All necessary permissions are granted, proceed with the app's functionality
            permissionsGranted = true;
        }

        void OnPermissionsDenied()
        {
            // Inform the user that not all permissions were granted and the app might not work properly
            Snackbar.Make(FindViewById(Android.Resource.Id.Content), "Permissions denied. The app may not function correctly.", Snackbar.LengthIndefinite)
                .SetAction("Settings", v =>
                {
                    // Open app settings
                    var intent = new Android.Content.Intent(Android.Provider.Settings.ActionApplicationDetailsSettings);
                    var uri = Android.Net.Uri.FromParts("package", PackageName, null);
                    intent.SetData(uri);
                    StartActivity(intent);
                })
                .Show();
        }

        public void InitializeSentryAsync()
        {
            SentrySdk.Init(o =>
            {
                // Tells which project in Sentry to send events to:
                o.Dsn = "https://4da007db4594a10f53ab292097e612f8@o4507304617836544.ingest.de.sentry.io/4507304993751120";
            });
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

            if (language == "SI")
            {
                imgSlovenian.SetColorFilter(highlightFilter);
                Base.Store.language = "sl";
            }
            else if (language == "US")
            {
                imgEnglish.SetColorFilter(highlightFilter);
                Base.Store.language = "en";
            }
        }

        private void GetLogo()
        {
            try
            {
                var url = App.Settings.RootURL + "/Services/Logo";
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
            if (App.Settings.tablet == true)
            {
                base.RequestedOrientation = Android.Content.PM.ScreenOrientation.Landscape;
            }
            else
            {
                base.RequestedOrientation = Android.Content.PM.ScreenOrientation.Portrait;
            }
        }





        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.action_setting1:
                    {
                        Finish();
                        StartActivity(typeof(Settings));
                        Finish();
                        return true;
                    }

            }

            return base.OnOptionsItemSelected(item);
        }
        private void Listener_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(Settings));
            Finish();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Layout.popup_action, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        private async void BtnRegistrationEvent_Click(object sender, System.EventArgs e)
        {
            if (permissionsGranted)
            {
                await ProcessRegistrationAsync();
            }
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


    }
}