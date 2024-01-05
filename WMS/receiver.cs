using Stream = Android.Media.Stream;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Media;
using Android.Net;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using BarCode2D_Receiver;
using Java.Lang;
using Microsoft.AppCenter.Crashes;
using WMS.App;
using Exception = System.Exception;

namespace WMS
{
    [Activity(Label = "receiver")]
    public class receiver : Activity, IBarcodeResult
    {
        Barcode2D barcode2D = new Barcode2D();
        Button btn1;
        Button btn2;
        TextView tv;
        SoundPool soundPool;
        int soundPoolId;
        private ProgressDialog proDialg;
        private string barcode;
        private string target;
        private IBarcodeResult text;
        private IBarcodeResult result;
        Button btnOkay;
        //
        public void GetBarcode(string barcode)
        {
            if (!string.IsNullOrEmpty(barcode))
            {
                Sound();
                tv.Text = barcode;

            }
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Create your application here
            soundPool = new SoundPool(10, Stream.Music, 0);
            soundPoolId = soundPool.Load(this, Resource.Drawable.beep, 1);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.ReceiverLayout);
            AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
            var _customToolbar = new CustomToolbar(this, toolbar, Resource.Id.navIcon);
            _customToolbar.SetNavigationIcon(settings.RootURL + "/Services/Logo");
            tv = FindViewById<TextView>(Resource.Id.textView1);
            btn1 = FindViewById<Button>(Resource.Id.button1);
            btnOkay = FindViewById<Button>(Resource.Id.btnOkay);
            btnOkay.Click += BtnOkay_Click;
           
            btn1.Click += Btn1_Click;
            Log.Info("1111", "11111");
            new InitTask(this).Execute();// open();

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

        private void BtnOkay_Click(object sender, EventArgs e)
        {
            Intent data = new Intent();
            data.SetData(Android.Net.Uri.Parse(tv.Text.ToString()));
            SetResult(Result.Ok, data);
            Finish();
        }

        private void Btn1_Click(object sender, EventArgs e)
        {
            barcode2D.EnableTrigger(this, true);
        }

      

        private void Sound()
        {

            soundPool.Play(soundPoolId, 1, 1, 0, 0, 1);

        }

        private void start()
        {
            barcode2D.startScan(this);
        }
        private void stop()
        {
            barcode2D.stopScan(this);
        }
        private void open()
        {
            barcode2D.open(this, this);
        }
        private void close()
        {
            barcode2D.stopScan(this);
            Java.Lang.Thread.Sleep(500);
            barcode2D.close(this);
        }
        //protected override void OnDestroy()
        //{
        //    try
        //    {
        //        close();
        //        base.OnDestroy();
        //        Log.Info("2222", "2222");
        //        Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
        //    }
        //    catch (Java.Lang.Exception ex)
        //    {
        //        ex.PrintStackTrace();
        //    }


        //}



        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            //if (keyCode == Keycode.F9)
            //{
            //    if (e.RepeatCount == 0)
            //    {
            //        start();
            //    }
            //    return true;
            //}
            return base.OnKeyDown(keyCode, e);
        }
        public override bool OnKeyUp(Keycode keyCode, KeyEvent e)
        {
            //if (keyCode == Keycode.F9)
            //{
            //    if (e.RepeatCount == 0)
            //    {
            //        stop();
            //    }
            //    return true;
            //}
            return base.OnKeyUp(keyCode, e);
        }

     
        private class InitTask : AsyncTask<Java.Lang.Void, Java.Lang.Void, string[]>
        {
            private readonly receiver _receiver;
            public InitTask(receiver receiverinit)
            {
                _receiver = receiverinit;
            }

            ProgressDialog proDialg = null;



            protected override string[] RunInBackground(params Java.Lang.Void[] @params)
            {
                return null;
            }
            //后台要执行的任务
            protected override Java.Lang.Object DoInBackground(params Java.Lang.Object[] @params)
            {
                _receiver.open();
                try
                {
                    Java.Lang.Thread.Sleep(1000);
                }
                catch (InterruptedException e)
                {
                    e.PrintStackTrace();
                }
                return true;
            }
            protected override void OnPostExecute(Java.Lang.Object result)
            {
                proDialg.Cancel();
                if (result.ToString() != "OK")
                    Toast.MakeText(_receiver, "Init failure!", ToastLength.Short);
            }

            //开始执行任务
            protected override void OnPreExecute()
            {
                proDialg = new ProgressDialog(_receiver);
                proDialg.SetMessage("init.....");
                proDialg.Show();
            }
        }
    }
}
   