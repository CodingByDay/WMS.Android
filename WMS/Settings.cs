using Android.Content;
using Android.Net;
using Android.Views;
using TrendNET.WMS.Device.Services;
using WMS.App;
using WMS.ExceptionStore;
using Xamarin.Essentials;
using static BluetoothService;
using AlertDialog = Android.App.AlertDialog;


namespace WMS
{
    [Activity(Label = "Settings", WindowSoftInputMode = SoftInput.AdjustResize)]
    public class Settings : CustomBaseActivity
    {
        private EditText ID;
        private EditText rootURL;
        private TextView version;
        private Button ok;
        private ImageView bin;
        public static string deviceInfo;
        private Spinner cbDevice;
        public static string IDinfo;
        private List<string> arrayData = new List<string>();
        private string item;
        private int position;
        private string dev;
        public static bool flag;
        public MyBinder binder;
        public bool isBound = false;
        public MyServiceConnection serviceConnection;
        private EventBluetooth send;
        private Button bluetooth;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                SetTheme(Resource.Style.AppTheme_NoActionBar);
                SetContentView(Resource.Layout.Settings);
                arrayData.Add($"{Resources.GetString(Resource.String.s319)}");
                arrayData.Add($"{Resources.GetString(Resource.String.s320)}");
                arrayData.Add($"{Resources.GetString(Resource.String.s321)}");
                bluetooth = FindViewById<Button>(Resource.Id.bluetooth);
                bluetooth.Click += Bluetooth_Click;
                ID = FindViewById<EditText>(Resource.Id.IDdevice);
                rootURL = FindViewById<EditText>(Resource.Id.rootURL);
                cbDevice = FindViewById<Spinner>(Resource.Id.cbDevice);
                ok = FindViewById<Button>(Resource.Id.ok);
                ok.Click += Ok_Click;
                ID.Text = App.Settings.ID;
                version = FindViewById<TextView>(Resource.Id.version);
                rootURL.Text = App.Settings.RootURL;
                var adapter = new ArrayAdapter<String>(this,
                Android.Resource.Layout.SimpleSpinnerItem, arrayData);
                bin = FindViewById<ImageView>(Resource.Id.bin);
                adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
                cbDevice.Adapter = adapter;
                bin.Click += Bin_Click;
                cbDevice.ItemSelected += CbDevice_ItemSelected;
                maintainSelection();
                // Create your application here
                var _broadcastReceiver = new NetworkStatusBroadcastReceiver();
                _broadcastReceiver.ConnectionStatusChanged += OnNetworkStatusChanged;
                Application.Context.RegisterReceiver(_broadcastReceiver,
                new IntentFilter(ConnectivityManager.ConnectivityAction), ReceiverFlags.NotExported);

                /*                 
                 try
                  {
                      if (CommonData.GetSetting("Bluetooth") != "1")
                      {
                          bluetooth.Visibility = ViewStates.Invisible;
                      }

                  } catch {

                      bluetooth.Visibility = ViewStates.Invisible;


                  } * // For now hide it  */

                bluetooth.Visibility = ViewStates.Invisible;
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private void Bluetooth_Click(object sender, EventArgs e)
        {
            try
            {
                // Start the BluetoothService with the selected device
                Intent serviceIntent = new Intent(this, typeof(BluetoothService));
                StartService(serviceIntent);
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private void Bin_Click(object sender, EventArgs e)
        {
            try
            {
                AlertDialog.Builder builder = new AlertDialog.Builder(this);
                builder.SetMessage($"{Resources.GetString(Resource.String.s322)}");
                builder.SetPositiveButton($"{Resources.GetString(Resource.String.s201)}", async (senderDialog, args) =>
                {
                    var (success, result) = await WebApp.GetAsync("mode=clearCache", this);
                });
                AlertDialog alertDialog = builder.Create();
                alertDialog.Show();
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

        public void maintainSelection()
        {
            try
            {
                if (App.Settings.tablet == true)
                {
                    cbDevice.SetSelection(1);
                }
                else
                {
                    cbDevice.SetSelection(2);
                }
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private void CbDevice_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            try
            {
                position = e.Position;

                if (position == 1)
                {
                    App.Settings.tablet = true;
                }
                else
                {
                    App.Settings.tablet = false;
                }
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private void Ok_Click(object sender, EventArgs e)
        {
            try
            {
                App.Settings.device = dev;
                App.Settings.RootURL = rootURL.Text;
                App.Settings.ID = ID.Text;
                ID.Text = App.Settings.ID;
                StartActivity(typeof(MainActivity));
                Finish();
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        internal void OnServiceBindingComplete(BluetoothService bluetoothService)
        {
            try { 

            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
            // Toast.MakeText(this, "Povezava z napravo je bila uspešna", ToastLength.Long);
        }
    }
}