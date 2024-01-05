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

namespace WMS
{
    [Activity(Label = "MenuPalletsTablet", ScreenOrientation = Android.Content.PM.ScreenOrientation.Landscape)]
    public class MenuPalletsTablet : Activity
    {
        private Button shipped;
        private Button wrapped;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.MenuPalletsTablet);
            AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
            var _customToolbar = new CustomToolbar(this, toolbar, Resource.Id.navIcon);
            _customToolbar.SetNavigationIcon(settings.RootURL + "/Services/Logo");
            shipped = FindViewById<Button>(Resource.Id.shipped);
            wrapped = FindViewById<Button>(Resource.Id.wrapped);


            // Beggining of the activity.

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
        public override void OnBackPressed()
        {

            HelpfulMethods.releaseLock();

            base.OnBackPressed();
        }
        private void Wrapped_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(WrappingPalletTablet));
            HelpfulMethods.clearTheStack(this);
        }

        private void Shipped_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(ShippingPalletTablet));
            HelpfulMethods.clearTheStack(this);
        }
    }
}