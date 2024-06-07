using Android.Content;
using Android.Net;
using Android.Views;
using TrendNET.WMS.Device.Services;
using WMS.App;
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
            new IntentFilter(ConnectivityManager.ConnectivityAction));

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

        private void Bluetooth_Click(object sender, EventArgs e)
        {
            // Start the BluetoothService with the selected device
            Intent serviceIntent = new Intent(this, typeof(BluetoothService));
            StartService(serviceIntent);
        }

        private void Bin_Click(object sender, EventArgs e)
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            builder.SetMessage($"{Resources.GetString(Resource.String.s322)}");
            builder.SetPositiveButton($"{Resources.GetString(Resource.String.s201)}", (senderDialog, args) =>
            {
                string result;
                WebApp.Get("mode=clearCache", out result);
            });
            AlertDialog alertDialog = builder.Create();
            alertDialog.Show();
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

        public string GetAppVersion()
        {
            return AppInfo.VersionString;
        }

        public void maintainSelection()
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

        private void CbDevice_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
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

        private void Ok_Click(object sender, EventArgs e)
        {
            App.Settings.device = dev;
            App.Settings.RootURL = rootURL.Text;
            App.Settings.ID = ID.Text;
            ID.Text = App.Settings.ID;
            StartActivity(typeof(MainActivity));
            Finish();
        }

        internal void OnServiceBindingComplete(BluetoothService bluetoothService)
        {
            // Toast.MakeText(this, "Povezava z napravo je bila uspešna", ToastLength.Long);
        }
    }
}