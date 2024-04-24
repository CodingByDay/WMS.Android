using Android.App;
using Android.Content;
using Android.Net;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Microsoft.AppCenter.Crashes;
using WMS.App;
using Stream = Android.Media.Stream;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using AndroidX.AppCompat.App;
using AlertDialog = Android.App.AlertDialog;
using Android.Content.PM;
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
            if (settings.tablet)
            {
                RequestedOrientation = ScreenOrientation.Landscape;
                SetContentView(Resource.Layout.MenuPalletsTablet);
            }
            else
            {
                RequestedOrientation = ScreenOrientation.Portrait;
                SetContentView(Resource.Layout.MenuPallets);
            }
            AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
            var _customToolbar = new CustomToolbar(this, toolbar, Resource.Id.navIcon);
            _customToolbar.SetNavigationIcon(settings.RootURL + "/Services/Logo");
            SetSupportActionBar(_customToolbar._toolbar);
            SupportActionBar.SetDisplayShowTitleEnabled(false);
            shipped = FindViewById<Button>(Resource.Id.shipped);
            wrapped = FindViewById<Button>(Resource.Id.wrapped);
            shipped.Click += Shipped_Click;
            wrapped.Click += Wrapped_Click;
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

        private void Wrapped_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(WrappingPallet)); // Wrapping pallet new functionality.
            HelpfulMethods.clearTheStack(this);
        }
        public override void OnBackPressed()
        {

            HelpfulMethods.releaseLock();

            base.OnBackPressed();
        }
        private void Shipped_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(ShippingPallet)); // Shipping pallet new functionality.
            HelpfulMethods.clearTheStack(this);
        }
    }
}