using Stream = Android.Media.Stream;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Net;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Microsoft.AppCenter.Crashes;
using WMS.App;

using TrendNET.WMS.Core.Data;

namespace WMS
{
    [Activity(Label = "PrintingSSCCCodesTablet", ScreenOrientation = Android.Content.PM.ScreenOrientation.Landscape)]
    public class PrintingSSCCCodesTablet : Activity
    {
        private EditText tbNum;
        private Button button1;
        private Button button2;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Create your application here
            SetContentView(Resource.Layout.PrintingSSCCCodesTablet);
            AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
            var _customToolbar = new CustomToolbar(this, toolbar, Resource.Id.navIcon);
            _customToolbar.SetNavigationIcon(settings.RootURL + "/Services/Logo");
            tbNum = FindViewById<EditText>(Resource.Id.tbNum);
            button1 = FindViewById<Button>(Resource.Id.button1);
            button2 = FindViewById<Button>(Resource.Id.button2);
            button1.Click += Button1_Click;
            button2.Click += Button2_Click;
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
        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            switch (keyCode)
            {
                // in smartphone
                case Keycode.F3:
                    if (button1.Enabled == true)
                    {
                        Button1_Click(this, null);
                    }
                    break;
                //return true;


                case Keycode.F8:
                    if (button2.Enabled == true)
                    {
                        Button2_Click(this, null);
                    }
                    break;


            }
            return base.OnKeyDown(keyCode, e);
        }
        private void Button2_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(MainMenuTablet));
            HelpfulMethods.clearTheStack(this);
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            var num = -1;
            try
            {
                num = Convert.ToInt32(tbNum.Text);
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, "Št. nalepk ni pravilno! (" + ex.Message + ")", ToastLength.Long).Show();

                return;
            }

            if (num < 1)
            {
                Toast.MakeText(this, "Št. nalepk mora biti pozitivno!", ToastLength.Long).Show();

                return;
            }


            try
            {

                var nvo = new NameValueObject("PrintSSCC");
                PrintingCommon.SetNVOCommonData(ref nvo);
                nvo.SetInt("Copies", num);
                PrintingCommon.SendToServer(nvo);
                Toast.MakeText(this, "Uspešno poslani podatki...", ToastLength.Long).Show();
            }
            catch (Exception err)
            {

                Crashes.TrackError(err);
                return;

            }
        }
    }
}