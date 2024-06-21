using Android;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.InputMethodServices;
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
using WMS.ExceptionStore;
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

        private async Task ProcessRegistrationAsync()
        {
            try
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
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }




        protected async override void OnCreate(Bundle savedInstanceState)
        {
            try
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
                new IntentFilter(ConnectivityManager.ConnectivityAction), ReceiverFlags.NotExported);
                Button btnRegistrationEvent = FindViewById<Button>(Resource.Id.btnRegistrationClick);
                btnRegistrationEvent.Clickable = true;
                btnRegistrationEvent.Enabled = true;
                btnRegistrationEvent.Click += BtnRegistrationEvent_Click;
                Password.KeyPress += Password_KeyPress;
                App.Settings.login = false;

                InitializeSentryAsync();

                // Check and request necessary permissions at startup because of Google Play policies. 29.05.2024 Janko Jovièiæ
                // RequestNecessaryPermissions(); // For now not needed. 31.05.2024 Janko Jovièiæ
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private void Password_KeyPress(object? sender, View.KeyEventArgs e)
        {
            try
            {
                if (e.Event.Action == KeyEventActions.Down && e.KeyCode == Android.Views.Keycode.Enter)
                {
                    BtnRegistrationEvent_Click(this, null);
                    e.Handled = true;
                }
                else
                {
                    e.Handled = false;
                }
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }


        void RequestNecessaryPermissions()
        {
            try
            {
                string[] requiredPermissions = new string[]
                {
                Manifest.Permission.Bluetooth
                };

                CheckAndRequestPermissions(requiredPermissions);
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        void CheckAndRequestPermissions(string[] permissions)
        {
            try
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
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            try
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
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        void OnAllPermissionsGranted()
        {
            try
            {
                // All necessary permissions are granted, proceed with the app's functionality
                permissionsGranted = true;
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        void OnPermissionsDenied()
        {
            try
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
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        public void InitializeSentryAsync()
        {
            try
            {
                SentrySdk.Init(o =>
                {
                    // Tells which project in Sentry to send events to:
                    o.Dsn = "https://4da007db4594a10f53ab292097e612f8@o4507304617836544.ingest.de.sentry.io/4507304993751120";
                });
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }



        public string GetAppVersion()
        {
            try
            {
                return AppInfo.VersionString;
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
                return string.Empty;
            }
        }

        private void SetUpLanguages()
        {
            try
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
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private void GetLogo()
        {
            try
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
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
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





        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            try
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
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
                return false;
            }
        }
        private void Listener_Click(object sender, EventArgs e)
        {
            try
            {
                StartActivity(typeof(Settings));
                Finish();
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            try
            {
                MenuInflater.Inflate(Resource.Layout.popup_action, menu);
                return base.OnCreateOptionsMenu(menu);
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
                return false;
            }
        }

        private async void BtnRegistrationEvent_Click(object sender, System.EventArgs e)
        {
            try
            {
                if (permissionsGranted)
                {
                    LoaderManifest.LoaderManifestLoopResources(this);

                    await ProcessRegistrationAsync();

                    LoaderManifest.LoaderManifestLoopStop(this);

                }
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

    

    }
}