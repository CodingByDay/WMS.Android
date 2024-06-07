using Android.Content;
using Android.Content.PM;
using Android.Media;
using Android.Net;
using Android.Preferences;
using Android.Views;
using BarCode2D_Receiver;
using Newtonsoft.Json;
using TrendNET.WMS.Device.Services;
using WMS.App;
using AlertDialog = Android.App.AlertDialog;
namespace WMS
{
    [Activity(Label = "RecalculateInventory")]
    public class RecalculateInventory : CustomBaseActivity, IBarcodeResult
    {

        private EditText ident;
        private Button btCalculate;
        SoundPool soundPool;
        int soundPoolId;
        private Barcode2D barcode2D;
        private ProgressDialogClass progress;

        private List<string> identData = new List<string>();
        private List<string> returnList;
        private List<string> savedIdents;
        private CustomAutoCompleteTextView tbIdent;
        private CustomAutoCompleteAdapter<string> tbIdentAdapter;

        public void GetBarcode(string barcode)
        {
            if (ident.HasFocus)
            {

                ident.Text = barcode;

            }
        }
        public override void OnBackPressed()
        {

            HelpfulMethods.releaseLock();

            base.OnBackPressed();
        }


        protected async override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetTheme(Resource.Style.AppTheme_NoActionBar);
            // Create your application here
            if (App.Settings.tablet)
            {
                base.RequestedOrientation = ScreenOrientation.Landscape;
                base.SetContentView(Resource.Layout.RecalculateInventoryTablet);
            }
            else
            {
                base.RequestedOrientation = ScreenOrientation.Portrait;
                base.SetContentView(Resource.Layout.RecalculateInventory);
            }
            AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
            var _customToolbar = new CustomToolbar(this, toolbar, Resource.Id.navIcon);
            _customToolbar.SetNavigationIcon(App.Settings.RootURL + "/Services/Logo");
            SetSupportActionBar(_customToolbar._toolbar);
            SupportActionBar.SetDisplayShowTitleEnabled(false);
            ident = FindViewById<EditText>(Resource.Id.ident);

            btCalculate = FindViewById<Button>(Resource.Id.btCalculate);

            barcode2D = new Barcode2D(this, this);

            color();



            btCalculate.Click += BtCalculate_Click;
            tbIdent = FindViewById<CustomAutoCompleteTextView>(Resource.Id.ident);

            ISharedPreferences sharedPreferences = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
            ISharedPreferencesEditor editor = sharedPreferences.Edit();
            string savedIdentsJson = sharedPreferences.GetString("idents", "");
            if (!string.IsNullOrEmpty(savedIdentsJson))
            {
                savedIdents = JsonConvert.DeserializeObject<List<string>>(savedIdentsJson);
            }
            tbIdent.LongClick += ClearTheFields;
            tbIdentAdapter = new CustomAutoCompleteAdapter<string>(this, Android.Resource.Layout.SimpleDropDownItem1Line, new List<string>());
            tbIdent.Adapter = tbIdentAdapter;
            tbIdent.TextChanged += (sender, e) =>
            {
                string userInput = e.Text.ToString();
                UpdateSuggestions(userInput);
            };

            tbIdent.LongClick += ClearTheFields;
            var DataAdapter = new CustomAutoCompleteAdapter<string>(this,
            Android.Resource.Layout.SimpleSpinnerItem, identData);
            DataAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            ident.LongClick += ClearTheFields;
            var _broadcastReceiver = new NetworkStatusBroadcastReceiver();
            _broadcastReceiver.ConnectionStatusChanged += OnNetworkStatusChanged;
            Application.Context.RegisterReceiver(_broadcastReceiver,
            new IntentFilter(ConnectivityManager.ConnectivityAction));
        }

        private void UpdateSuggestions(string userInput)
        {
            List<string> suggestions = GetCustomSuggestions(userInput);
            tbIdentAdapter.Clear();
            tbIdentAdapter.AddAll(suggestions);
            tbIdentAdapter.NotifyDataSetChanged();
        }

        private List<string> GetCustomSuggestions(string userInput)
        {

            return savedIdents
                .Where(suggestion => suggestion.ToLower().Contains(userInput.ToLower())).Take(10000)
                .ToList();
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



        private void ClearTheFields(object sender, View.LongClickEventArgs e)
        {
            ident.Text = "";
        }


        private async void BtCalculate_Click(object sender, EventArgs e)
        {
            var value = ident.Text;
            await FinishMethod(value);
        }

        private void color()
        {
            ident.SetBackgroundColor(Android.Graphics.Color.Aqua);
        }


        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            switch (keyCode)
            {
                // In smartphone.  
                case Keycode.F2:
                    if (btCalculate.Enabled == true)
                    {
                        BtCalculate_Click(this, null);
                    }
                    break;



            }
            return base.OnKeyDown(keyCode, e);
        }



        private async Task FinishMethod(string ident)
        {



            await Task.Run(() =>
            {


                try
                {


                    RunOnUiThread(() =>
                    {
                        progress = new ProgressDialogClass();

                        progress.ShowDialogSync(this, $"{Resources.GetString(Resource.String.s317)}");
                    });

                    string result;
                    if (WebApp.Get("mode=recalc&id=" + ident, out result))
                    {


                        if (result == "OK")
                        {
                            RunOnUiThread(() =>
                            {


                                progress.StopDialogSync();

                                AlertDialog.Builder alert = new AlertDialog.Builder(this);

                                alert.SetTitle($"{Resources.GetString(Resource.String.s263)}");


                                alert.SetMessage($"{Resources.GetString(Resource.String.s318)}");

                                alert.SetPositiveButton("Ok", (senderAlert, args) =>
                                {
                                    alert.Dispose();
                                    System.Threading.Thread.Sleep(500);
                                    StartActivity(typeof(MainMenu));
                                    HelpfulMethods.clearTheStack(this);

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
                                alert.SetMessage($"Napaka: {result}");

                                alert.SetPositiveButton("Ok", (senderAlert, args) =>
                                {
                                    alert.Dispose();
                                    System.Threading.Thread.Sleep(500);
                                    StartActivity(typeof(MainMenu));
                                    HelpfulMethods.clearTheStack(this);

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
                                HelpfulMethods.clearTheStack(this);

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
                        alert.SetMessage($"{Resources.GetString(Resource.String.s247)}" + ex.Message);
                        alert.SetPositiveButton("Ok", (senderAlert, args) =>
                        {
                            alert.Dispose();
                            System.Threading.Thread.Sleep(500);
                            StartActivity(typeof(MainMenu));
                            HelpfulMethods.clearTheStack(this);
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
    }
}