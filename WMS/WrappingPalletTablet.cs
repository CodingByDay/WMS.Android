using Android.App;
using Android.Content;
using Android.Media;
using Android.Net;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using BarCode2D_Receiver;
using Microsoft.AppCenter.Crashes;
using WMS.App;
using Stream = Android.Media.Stream;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TrendNET.WMS.Device.Services;

using AndroidX.AppCompat.App;
using AlertDialog = Android.App.AlertDialog;

namespace WMS
{
    [Activity(Label = "WrappingPalletTablet")]
    public class WrappingPalletTablet : AppCompatActivity, IBarcodeResult
    {
        private EditText pallet;
        private Button btConfirm;
        private SoundPool soundPool;
        private int soundPoolId;

        public void GetBarcode(string barcode)
        {
            if (pallet.HasFocus)
            {
                if (barcode != "Scan fail")
                {
                    Sound();
                    pallet.Text = barcode;
                }
                else
                {
                    pallet.Text = "";
                }
            }
        }

        private void Sound()
        {
            soundPool.Play(soundPoolId, 1, 1, 0, 0, 1);
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetTheme(Resource.Style.AppTheme_NoActionBar);
            SetContentView(Resource.Layout.WrappingPalletTablet);
            AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
            var _customToolbar = new CustomToolbar(this, toolbar, Resource.Id.navIcon);
            _customToolbar.SetNavigationIcon(settings.RootURL + "/Services/Logo");
            SetSupportActionBar(_customToolbar._toolbar);
            SupportActionBar.SetDisplayShowTitleEnabled(false);
            // Create your application here
            pallet = FindViewById<EditText>(Resource.Id.pallet);
            btConfirm = FindViewById<Button>(Resource.Id.btConfirm);
            btConfirm.Click += BtConfirm_Click;
            color();
            soundPool = new SoundPool(10, Stream.Music, 0);
            soundPoolId = soundPool.Load(this, Resource.Raw.beep, 1);
            Barcode2D barcode2D = new Barcode2D();
            barcode2D.open(this, this);
            pallet.RequestFocus();
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

        private void color()
        {
            pallet.SetBackgroundColor(Android.Graphics.Color.Aqua);
        }

        private void BtConfirm_Click(object sender, EventArgs e)
        {
            string TextPallet = pallet.Text;
            string result;
            if (WebApp.Get("mode=palPck&pal=" + TextPallet, out result))
            {
                if (result == "OK")
                {
                    Toast.MakeText(this, "Paleta uspešno zavita!", ToastLength.Long).Show();
                }
                else
                {
                    Toast.MakeText(this, $"Napaka pri zavijanju palete. {result}", ToastLength.Long).Show();
                }
            }
            else
            {
                Toast.MakeText(this, $"Napaka pri dostopu do web aplikacije. {result}", ToastLength.Long).Show();
            }
        }
    }
}