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
using System.Threading.Tasks;
using TrendNET.WMS.Device.Services;

using AndroidX.AppCompat.App;using AlertDialog = Android.App.AlertDialog;namespace WMS
{
    [Activity(Label = "ShippingPalletTablet")]
    public class ShippingPalletTablet : CustomBaseActivity, IBarcodeResult
    {
        private EditText pallet;
        private EditText machine;
        private Button btConfirm;
        SoundPool soundPool;
        int soundPoolId;
        private ProgressDialogClass progress;

        public string ETpallet { get; private set; }
        public string ETmachine { get; private set; }

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
            else if (machine.HasFocus)
            {
                if (barcode != "Scan fail")
                {
                    Sound();
                    machine.Text = barcode;
                }
                else
                {
                    machine.Text = "";
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
            SetContentView(Resource.Layout.ShippingPalletTablet);
            AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
            var _customToolbar = new CustomToolbar(this, toolbar, Resource.Id.navIcon);
            _customToolbar.SetNavigationIcon(settings.RootURL + "/Services/Logo");
            SetSupportActionBar(_customToolbar._toolbar);
            SupportActionBar.SetDisplayShowTitleEnabled(false);
            // Create your application here
            pallet = FindViewById<EditText>(Resource.Id.pallet);
            machine = FindViewById<EditText>(Resource.Id.machine);
            btConfirm = FindViewById<Button>(Resource.Id.btConfirm);

            btConfirm.Click += BtConfirm_Click;

            color();

            soundPool = new SoundPool(10, Stream.Music, 0);
            soundPoolId = soundPool.Load(this, Resource.Raw.beep, 1);
            Barcode2D barcode2D = new Barcode2D();
            barcode2D.open(this, this);

            machine.RequestFocus();
            machine.FocusChange += Machine_FocusChange;

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

        private void Machine_FocusChange(object sender, View.FocusChangeEventArgs e)
        {
            pallet.RequestFocus();
        }

        private async Task FinishMethod()
        {
            await Task.Run(() =>
            {
       
          

                try
                {


                    RunOnUiThread(() =>
                    {
                         ETpallet = pallet.Text;
                        
                         ETmachine = machine.Text;

                        progress = new ProgressDialogClass();

                        progress.ShowDialogSync(this, $"{Resources.GetString(Resource.String.s308)}");
                    });

                    string result;
                    if (WebApp.Get("mode=palMac&pal=" + ETpallet + "&mac=" + ETmachine, out result))
                    {
                        if (result == "OK")
                        {
                            RunOnUiThread(() =>
                            {


                                progress.StopDialogSync();
                                AlertDialog.Builder alert = new AlertDialog.Builder(this);
                                alert.SetTitle($"{Resources.GetString(Resource.String.s323)}");
                                alert.SetMessage($"{Resources.GetString(Resource.String.s324)}");

                                alert.SetPositiveButton("Ok", (senderAlert, args) =>
                                {
                                    alert.Dispose();
                                    System.Threading.Thread.Sleep(500);
                                    StartActivity(typeof(MainMenu));

                                });



                                Dialog dialog = alert.Create();
                                dialog.Show();
                            });

                        }
                        else
                        {
                            RunOnUiThread(() =>
                            {


                                progress.StopDialogSync();
                                AlertDialog.Builder alert = new AlertDialog.Builder(this);
                                alert.SetTitle($"{Resources.GetString(Resource.String.s265)}");
                                alert.SetMessage($"{Resources.GetString(Resource.String.s216)}" + result);

                                alert.SetPositiveButton("Ok", (senderAlert, args) =>
                                {
                                    alert.Dispose();
                                    System.Threading.Thread.Sleep(500);
                                    StartActivity(typeof(MainMenu));

                                });



                                Dialog dialog = alert.Create();
                                dialog.Show();
                            });
                        }
                    }
                    else
                    {
                        RunOnUiThread(() =>
                        {


                            progress.StopDialogSync();
                            AlertDialog.Builder alert = new AlertDialog.Builder(this);
                            alert.SetTitle($"{Resources.GetString(Resource.String.s265)}");
                            alert.SetMessage($"{Resources.GetString(Resource.String.s213)}");

                            alert.SetPositiveButton("Ok", (senderAlert, args) =>
                            {
                                alert.Dispose();
                                System.Threading.Thread.Sleep(500);
                                StartActivity(typeof(MainMenu));

                            });



                            Dialog dialog = alert.Create();
                            dialog.Show();
                        });



                    }
                }
                catch (Exception ex)
                {


                    RunOnUiThread(() =>
                    {


                        progress.StopDialogSync();
                        AlertDialog.Builder alert = new AlertDialog.Builder(this);
                        alert.SetTitle($"{Resources.GetString(Resource.String.s265)}");
                        alert.SetMessage($"{Resources.GetString(Resource.String.s216)}" + ex.Message);

                        alert.SetPositiveButton("Ok", (senderAlert, args) =>
                        {
                            alert.Dispose();
                            System.Threading.Thread.Sleep(500);
                            StartActivity(typeof(MainMenu));

                        });



                        Dialog dialog = alert.Create();
                        dialog.Show();
                    });
                }
                finally
                {
                    progress.StopDialogSync();
                }


            });
        }

        private void color()
        {
            pallet.SetBackgroundColor(Android.Graphics.Color.Aqua);
            machine.SetBackgroundColor(Android.Graphics.Color.Aqua);
        }

        private async void BtConfirm_Click(object sender, EventArgs e)
        {
            await FinishMethod();



        }
    }
}