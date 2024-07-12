using Android.Content;
using Android.Content.PM;
using Android.Media;
using Android.Net;
using TrendNET.WMS.Device.Services;
using WMS.App;
using WMS.ExceptionStore;
using AlertDialog = Android.App.AlertDialog;
namespace WMS
{
    [Activity(Label = "WMS")]
    public class WrappingPallet : CustomBaseActivity, IBarcodeResult
    {
        private EditText pallet;
        private Button btConfirm;
        SoundPool soundPool;
        int soundPoolId;
        private ProgressDialogClass progress;
        private BarCode2D_Receiver.Barcode2D barcode2D;


        public void GetBarcode(string barcode)
        {
            try
            {
                RunOnUiThread(() =>
                {
                    if (pallet.HasFocus)
                    {
                        if (barcode != "Scan fail")
                        {

                            pallet.Text = barcode;
                        }
                        else
                        {
                            pallet.Text = "";
                        }
                    }
                });
            } catch (Exception ex) {
            
                GlobalExceptions.ReportGlobalException(ex);
            
            }
       
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try       
            { 

                base.OnCreate(savedInstanceState);
                SetTheme(Resource.Style.AppTheme_NoActionBar);
                if (App.Settings.tablet)
                {
                    base.RequestedOrientation = ScreenOrientation.Landscape;
                    base.SetContentView(Resource.Layout.WrappingPalletTablet);
                }
                else
                {
                    base.RequestedOrientation = ScreenOrientation.Portrait;
                    base.SetContentView(Resource.Layout.WrappingPallet);
                }
                AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
                var _customToolbar = new CustomToolbar(this, toolbar, Resource.Id.navIcon);
                _customToolbar.SetNavigationIcon(App.Settings.RootURL + "/Services/Logo");
                SetSupportActionBar(_customToolbar._toolbar);
                SupportActionBar.SetDisplayShowTitleEnabled(false);
                // Create your application here
                pallet = FindViewById<EditText>(Resource.Id.pallet);
                btConfirm = FindViewById<Button>(Resource.Id.btConfirm);

                btConfirm.Click += BtConfirm_Click;
                color();
                barcode2D = new BarCode2D_Receiver.Barcode2D(this, this);
                pallet.RequestFocus();
                var _broadcastReceiver = new NetworkStatusBroadcastReceiver();
                _broadcastReceiver.ConnectionStatusChanged += OnNetworkStatusChanged;
                Application.Context.RegisterReceiver(_broadcastReceiver,
                new IntentFilter(ConnectivityManager.ConnectivityAction), ReceiverFlags.NotExported);

            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }

        }
        public bool IsOnline()
        {
            var cm = (ConnectivityManager)GetSystemService(ConnectivityService);
            return cm.ActiveNetworkInfo == null ? false : cm.ActiveNetworkInfo.IsConnected;

        }

        private void OnNetworkStatusChanged(object sender, EventArgs e)
        {
            try
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
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private void color()
        {
            try
            {
                RunOnUiThread(() =>
                {
                    pallet.SetBackgroundColor(Android.Graphics.Color.Aqua);

                });
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }



        private async Task FinishMethod()
        {
            try { 
            await Task.Run(async () =>
            {
                string TextPallet = string.Empty;

                RunOnUiThread(() =>
                {
                    TextPallet = pallet.Text;

                });

                

                var (success, result) = await WebApp.GetAsync("mode=palPck&pal=" + TextPallet, this);

                if (success)
                {
                    if (result == "OK")
                    {
                        RunOnUiThread(() =>
                        {


                            AlertDialog.Builder alert = new AlertDialog.Builder(this);
                            alert.SetTitle($"{Resources.GetString(Resource.String.s323)}");
                            alert.SetMessage($"{Resources.GetString(Resource.String.s332)}");

                            alert.SetPositiveButton("Ok", (senderAlert, args) =>
                            {
                                alert.Dispose();
                                StartActivity(typeof(MainMenu));
                                Finish();
                            });



                            Dialog dialog = alert.Create();
                            dialog.Show();
                        });
                    }
                    else
                    {

                        RunOnUiThread(() =>
                        {


                            AlertDialog.Builder alert = new AlertDialog.Builder(this);
                            alert.SetTitle($"{Resources.GetString(Resource.String.s265)}");
                            alert.SetMessage($"{Resources.GetString(Resource.String.s216)}" + result);

                            alert.SetPositiveButton("Ok", (senderAlert, args) =>
                            {
                                alert.Dispose();
                                StartActivity(typeof(MainMenu));
                                Finish();
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


                        AlertDialog.Builder alert = new AlertDialog.Builder(this);
                        alert.SetTitle($"{Resources.GetString(Resource.String.s265)}");
                        alert.SetMessage($"{Resources.GetString(Resource.String.s216)}" + result);

                        alert.SetPositiveButton("Ok", (senderAlert, args) =>
                        {
                            alert.Dispose();
                            StartActivity(typeof(MainMenu));
                            Finish();
                        });



                        Dialog dialog = alert.Create();
                        dialog.Show();
                    });

                }
            });
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }
        private async void BtConfirm_Click(object sender, EventArgs e)
        {
            try { 
            await FinishMethod();
                //string TextPallet = pallet.Text;
                //string result;
                //if (WebApp.Get("mode=palPck&pal=" + TextPallet, out result))
                //{
                //    if (result == "OK")
                //    {
                //        Toast.MakeText(this, $"{Resources.GetString(Resource.String.s332)}", ToastLength.Long).Show();
                //    } else
                //    {
                //        Toast.MakeText(this, $"Napaka pri zavijanju palete. {result}", ToastLength.Long).Show();
                //    }
                //} else
                //{
                //    Toast.MakeText(this, $"Napaka pri dostopu do web aplikacije. {result}", ToastLength.Long).Show();
                //}
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }
    }
}