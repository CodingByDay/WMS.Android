using Stream = Android.Media.Stream;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Net;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Microsoft.AppCenter.Crashes;
using WMS.App;
using TrendNET.WMS.Device.Services;

using AndroidX.AppCompat.App;using AlertDialog = Android.App.AlertDialog;namespace WMS
{
    [Activity(Label = "PrintingMenu")]
    public class PrintingMenu : CustomBaseActivity
    {
        public static string target = App.settings.device;
        public bool result = Services.isTablet(target); /* Is the device tablet. */
        private Button button1;
        private Button button2;
        private Button button6;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            ChangeTheOrientation(); 
            base.OnCreate(savedInstanceState);
            SetTheme(Resource.Style.AppTheme_NoActionBar);
            SetContentView(Resource.Layout.PrintingMenu);
            AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
            var _customToolbar = new CustomToolbar(this, toolbar, Resource.Id.navIcon);
            _customToolbar.SetNavigationIcon(settings.RootURL + "/Services/Logo");
            SetSupportActionBar(_customToolbar._toolbar);
            SupportActionBar.SetDisplayShowTitleEnabled(false);
            // button1 PrintingReprintLabels());
            button1 = FindViewById<Button>(Resource.Id.button1);
            button1.Click += Button1_Click;
            // button2 PrintingSSCCCodes());
            button2 = FindViewById<Button>(Resource.Id.button2);
            button2.Click += Button_Click;
            // button3 PrintingProcessControl());
          
            // button6 logout
            button6 = FindViewById<Button>(Resource.Id.button6);
            button6.Click += Button6_Click;

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


        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            switch (keyCode)
            {
                // In smartphone
                case Keycode.F2:

              
                        Button1_Click(this, null);

                    
                    break;
                // Return true;

                case Keycode.F3:


                    Button_Click(this, null);
                    

                    break;


                case Keycode.F4:

                    Button1_Click(this, null);
               
                    break;

            
                case Keycode.F8:

                    Button6_Click(this, null);

                    break;

                    // return true;
            }
            return base.OnKeyDown(keyCode, e);
        }
        private void Button6_Click(object sender, EventArgs e)
        {
            if (result == true)
            {
                StartActivity(typeof(MainMenuTablet));
                HelpfulMethods.clearTheStack(this);
            }
            else
            {
                StartActivity(typeof(MainMenu));
                HelpfulMethods.clearTheStack(this);
            }
        }


   

        private void Button_Click(object sender, EventArgs e)
        {
            if (result == true)
            {
                StartActivity(typeof(PrintingSSCCCodesTablet));
                HelpfulMethods.clearTheStack(this);
            } else
            {
                StartActivity(typeof(PrintingSSCCCodes));
                HelpfulMethods.clearTheStack(this);
            }
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            if (result == true)
            {

                StartActivity(typeof(PrintingReprintLabelsTablet));
                HelpfulMethods.clearTheStack(this);
            } else
            {
                StartActivity(typeof(PrintingReprintLabels));
                HelpfulMethods.clearTheStack(this);
            }
        }
    }
}