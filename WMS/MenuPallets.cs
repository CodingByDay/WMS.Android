using Android.Content;
using Android.Content.PM;
using Android.Net;
using WMS.App;
namespace WMS
{
    [Activity(Label = "MenuPallets")]
    public class MenuPallets : CustomBaseActivity
    {
        private Button shipped;
        private Button wrapped;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetTheme(Resource.Style.AppTheme_NoActionBar);
            if (App.Settings.tablet)
            {
                base.RequestedOrientation = ScreenOrientation.Landscape;
                base.SetContentView(Resource.Layout.MenuPalletsTablet);
            }
            else
            {
                base.RequestedOrientation = ScreenOrientation.Portrait;
                base.SetContentView(Resource.Layout.MenuPallets);
            }
            AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
            var _customToolbar = new CustomToolbar(this, toolbar, Resource.Id.navIcon);
            _customToolbar.SetNavigationIcon(App.Settings.RootURL + "/Services/Logo");
            SetSupportActionBar(_customToolbar._toolbar);
            SupportActionBar.SetDisplayShowTitleEnabled(false);
            shipped = FindViewById<Button>(Resource.Id.shipped);
            wrapped = FindViewById<Button>(Resource.Id.wrapped);
            shipped.Click += Shipped_Click;
            wrapped.Click += Wrapped_Click;
            var _broadcastReceiver = new NetworkStatusBroadcastReceiver();
            _broadcastReceiver.ConnectionStatusChanged += OnNetworkStatusChanged;
            Application.Context.RegisterReceiver(_broadcastReceiver,
            new IntentFilter(ConnectivityManager.ConnectivityAction), ReceiverFlags.NotExported);
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

        private void Wrapped_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(WrappingPallet)); // Wrapping pallet new functionality.
            Finish();
        }
  
        private void Shipped_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(ShippingPallet)); // Shipping pallet new functionality.
            Finish();
        }
    }
}