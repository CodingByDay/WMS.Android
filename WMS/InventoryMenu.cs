using System;
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
using Scanner.App;
using TrendNET.WMS.Device.Services;

namespace Scanner
{
    [Activity(Label = "InventoryMenu")]
    public class InventoryMenu : Activity
    {


        public static string target = App.settings.device;
     
        public bool result = Services.isTablet(target); /* Is the device tablet. */
        private Button button1;
        private Button button2;
        private Button button3;
        private Button button4;
        private Button button7;
        private Button logout;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            ChangeTheOrientation();
            // Create your application here
            SetContentView(Resource.Layout.InventoryMenu);
            button1 = FindViewById<Button>(Resource.Id.button1);
            button2 = FindViewById<Button>(Resource.Id.button2);
            button3 = FindViewById<Button>(Resource.Id.button3);
            button4 = FindViewById<Button>(Resource.Id.button4);
            button7 = FindViewById<Button>(Resource.Id.button7);
            logout = FindViewById<Button>(Resource.Id.logout);
            button1.Click += Button1_Click;
            button2.Click += Button2_Click;
            button3.Click += Button3_Click;
            button4.Click += Button4_Click;
            button7.Click += Button7_Click;
            logout.Click += Logout_Click;

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
                // in smartphone
                case Keycode.F2:
                    if (button1.Enabled == true)
                    {
                        Button1_Click(this, null);
                    }
                    break;
                //return true;


                case Keycode.F3:
                    if (button2.Enabled == true)
                    {
                        Button2_Click(this, null);
                    }
                    break;


                case Keycode.F4:
                    if (button3.Enabled == true)
                    {
                        Button3_Click(this, null);
                    }
                        break;
                    

                case Keycode.F5:
                    if (button4.Enabled == true)
                    {
                        Button4_Click(this, null);
                    }
                    break;

                case Keycode.F6:
                    if (button7.Enabled == true)
                    {
                        Button7_Click(this, null);
                    }
                    break;

                case Keycode.F8:
                    
                    Logout_Click(this, null);
                    break;

            }
            return base.OnKeyDown(keyCode, e);
        }
        private void Logout_Click(object sender, EventArgs e)
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

        private void Button7_Click(object sender, EventArgs e)
        {
            if (result == true)
            {
                StartActivity(typeof(InventoryPrintTablet));
                HelpfulMethods.clearTheStack(this);
            } else
            {
                StartActivity(typeof(InventoryPrint));
                HelpfulMethods.clearTheStack(this);
            }
        }

        private void Button4_Click(object sender, EventArgs e)
        {
            if (result == true)
            {
                StartActivity(typeof(InventoryOpenTablet));
                HelpfulMethods.clearTheStack(this);
            } else
            {
                StartActivity(typeof(InventoryOpen));
                HelpfulMethods.clearTheStack(this);
            }
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            if (result)
            {
                StartActivity(typeof(InventoryConfirmTablet));
                HelpfulMethods.clearTheStack(this);
            } else
            {
                StartActivity(typeof(InventoryConfirm));
                HelpfulMethods.clearTheStack(this);
            }
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            if (result == true)
            {
                StartActivity(typeof(InventoryProcessTablet));
                HelpfulMethods.clearTheStack(this);
            } else
            {
                StartActivity(typeof(InventoryProcess));
                HelpfulMethods.clearTheStack(this);
            }
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            if (result == true)
            {
                StartActivity(typeof(InventoryOpenDocumentTablet));
                HelpfulMethods.clearTheStack(this);
            } else
            {
                StartActivity(typeof(InventoryOpenDocument));
                HelpfulMethods.clearTheStack(this);
            }
        }
    }
}