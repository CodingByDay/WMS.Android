﻿using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Net;
using Android.Views;
using Aspose.Words;
using Java.Nio.FileNio.Attributes;
using Newtonsoft.Json;
using TrendNET.WMS.Device.Services;
using WMS.App;
using WMS.Caching;
using WMS.ExceptionStore;
using Xamarin.ANRWatchDog;
using Xamarin.Essentials;
using static Android.App.ActionBar;
using static BluetoothService;
using static EventBluetooth;
using static Xamarin.ANRWatchDog.ANRWatchDog;
namespace WMS
{
    [Activity(Label = "WMS")]
    public class MainMenu : CustomBaseActivity
    {
        private List<Button> buttons = new List<Button>();
        public static string IDdevice;
        public static string target;
        public bool result;
        private Button button;
        private Button buttonInterWarehouse;
        private Button buttonUnfinished;
        private Button buttonIssued;
        private Button buttonPrint;
        private Button btnInventory;
        private Button btnCheckStock;
        private Button btnPackaging;
        private Button btnLogout;
        private Button PalletsMenu;
        private Button btRecalculate;
        private Dialog popupDialog;
        private Button btnOkRestart;
        private bool isActive = false;
        private bool login = false;
        private Button? buttonRapidTakeover;
        private ListView rapidListview;
        private List<CleanupLocation> dataCleanup;
        private UniversalAdapter<CleanupLocation> dataAdapter;
        private GeneralServiceConnection serviceConnection;
        private BluetoothService activityBluetoothService;
        private EventBluetooth send;
        public MyBinder binder;
        public bool isBound = false;
        protected override async void OnCreate(Bundle savedInstanceState)
        {
            try
            {

                base.OnCreate(savedInstanceState);
                SetTheme(Resource.Style.AppTheme_NoActionBar);

                if (App.Settings.tablet)
                {
                    base.RequestedOrientation = ScreenOrientation.Landscape;
                    base.SetContentView(Resource.Layout.MainMenuTablet);
                    buttonRapidTakeover = FindViewById<Button>(Resource.Id.rapidTakeover);
                    buttonRapidTakeover.Click += ButtonRapidTakeover_Click;
                    rapidListview = FindViewById<ListView>(Resource.Id.rapidListview);
                    dataCleanup = await FillTheCleanupList();
                    dataAdapter = UniversalAdapterHelper.GetMainMenu(this, dataCleanup);
                    rapidListview.Adapter = dataAdapter;
                    UniversalAdapterHelper.SelectPositionProgramaticaly(rapidListview, 0);
                }
                else
                {
                    base.RequestedOrientation = ScreenOrientation.Portrait;
                    base.SetContentView(Resource.Layout.MainMenu);
                }

                if (CommonData.GetSetting("Bluetooth") == "1")
                {
                    // Binding to a service
                    serviceConnection = new GeneralServiceConnection(this);
                    Intent serviceIntent = new Intent(this, typeof(BluetoothService));
                    BindService(serviceIntent, serviceConnection, Bind.AutoCreate);
                }


                AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
                var _customToolbar = new CustomToolbar(this, toolbar, Resource.Id.navIcon);
                _customToolbar.SetNavigationIcon(App.Settings.RootURL + "/Services/Logo");
                SetSupportActionBar(_customToolbar._toolbar);
                SupportActionBar.SetDisplayShowTitleEnabled(false);
                var flag = Services.isTablet(App.Settings.device);
                IDdevice = App.Settings.ID;
                target = App.Settings.device;
                result = App.Settings.tablet;
                button = FindViewById<Button>(Resource.Id.goodsTakeOver);
                button.Click += Button_Click;
                buttons.Add(button);
                buttonInterWarehouse = FindViewById<Button>(Resource.Id.goodsInterWarehouse);
                buttonInterWarehouse.Click += ButtonInterWarehouse_Click;
                buttons.Add(buttonInterWarehouse);
                buttonUnfinished = FindViewById<Button>(Resource.Id.goodsProduction);
                buttonUnfinished.Click += ButtonUnfinished_Click;
                buttons.Add(buttonUnfinished);
                buttonIssued = FindViewById<Button>(Resource.Id.goodsIssued);
                buttonIssued.Click += ButtonIssued_Click;
                buttons.Add(buttonIssued);
                buttonPrint = FindViewById<Button>(Resource.Id.btnPrint);
                buttonPrint.Click += ButtonPrint_Click;
                buttons.Add(buttonPrint);
                btnInventory = FindViewById<Button>(Resource.Id.btnInventory);
                btnInventory.Click += BtnInventory_Click;
                buttons.Add(btnInventory);
                btnCheckStock = FindViewById<Button>(Resource.Id.btCheckStock);
                btnCheckStock.Click += BtnCheckStock_Click;
                buttons.Add(btnCheckStock);
                btnPackaging = FindViewById<Button>(Resource.Id.goodsPackaging);
                btnPackaging.Click += BtnPackaging_Click;
                buttons.Add(btnPackaging);
                btnLogout = FindViewById<Button>(Resource.Id.logout);
                btnLogout.Click += BtnLogout_Click;
                PalletsMenu = FindViewById<Button>(Resource.Id.PalletsMenu);
                buttons.Add(PalletsMenu);
                buttonInterWarehouse.Enabled = await Services.HasPermission("TNET_WMS_BLAG_TRN", "R", this);
                buttonIssued.Enabled = await Services.HasPermission("TNET_WMS_BLAG_SND", "R", this);
                buttonUnfinished.Enabled = await Services.HasPermission("TNET_WMS_BLAG_PROD", "R", this);
                button.Enabled = await Services.HasPermission("TNET_WMS_BLAG_ACQ", "R", this);
                btnPackaging.Enabled = await Services.HasPermission("TNET_WMS_BLAG_PKG", "R", this);
                buttonPrint.Enabled = await Services.HasPermission("TNET_WMS_OTHR_PRINT", "R", this);
                btnInventory.Enabled = await Services.HasPermission("TNET_WMS_OTHR_INV", "R", this);
                btRecalculate = FindViewById<Button>(Resource.Id.btRecalculate);
                btRecalculate.Click += BtRecalculate_Click;
                PalletsMenu.Enabled = await Services.HasPermission("TNET_WMS_BLAG_PAL", "R", this);
                PalletsMenu.Click += PalletsMenu_Click;
                HideDisabled(buttons);
                var _broadcastReceiver = new NetworkStatusBroadcastReceiver();
                _broadcastReceiver.ConnectionStatusChanged += OnNetworkStatusChanged;
                Application.Context.RegisterReceiver(_broadcastReceiver,
                new IntentFilter(ConnectivityManager.ConnectivityAction), ReceiverFlags.NotExported);
                Caching.Caching.SavedList = new List<string>();
                DownloadResources();
                // Reseting the global update variable.
                Base.Store.isUpdate = false;
                Base.Store.OpenOrder = null;
                Base.Store.byOrder = true;
                Base.Store.code2D = null;

                string pickingChoice = await CommonData.GetSettingAsync("IssueProcessSelectbreaking", this);


                // Get the package manager
                PackageManager packageManager = PackageManager;

                // Get the package name of your application
                string packageName = PackageName;
                int versionCode = 0;
                string versionName = string.Empty;

                try
                {
                    // Get package info for the specified package name
                    PackageInfo packageInfo = packageManager.GetPackageInfo(packageName, 0);
                    // Access version code and version name
                    versionCode = packageInfo.VersionCode; // Integer value
                    versionName = packageInfo.VersionName; // String value

                }
                catch (Exception ex)
                {
                    SentrySdk.CaptureException(ex);
                }

                // Global scope for sentry 10.06.2024 Janko Jovičić
                SentrySdk.ConfigureScope(scope =>
                {
                    var currentUser = new User
                    {
                        url = App.Settings.RootURL,
                        id = App.Settings.ID,
                        tablet = App.Settings.tablet,
                        versionName = versionName,
                        versionCode = versionCode.ToString()
                    };
                    scope.SetExtra("WMS User", currentUser);
                });




            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        public void OnServiceBindingComplete(BluetoothService service)
        {
            try
            {
                try
                {
                    activityBluetoothService = service;

                    // Needed to clean to glasses screen. 16.10.2024 Janko Jovičić
                    send = new EventBluetooth();
                    List<Position> positions = new List<Position>();
                    send.Positions = positions;
                    send.EventTypeValue = EventBluetooth.EventType.CleanUp;
                    send.IsRefreshCallback = true;
                    send.ChosenPosition = -1;
                    send.OrderNumber = string.Empty;
                    activityBluetoothService.SendObject(JsonConvert.SerializeObject(send));
                }
                catch
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        public override void OnConfigurationChanged(Configuration newConfig)
        {
            base.OnConfigurationChanged(newConfig);
            // Handle configuration changes manually if needed
            // Example: Adjust layout or UI components based on new configuration
        }

        private void InstallApk(string apkFilePath)
        {
            try
            {
                Intent intent = new Intent(Intent.ActionView);
                intent.SetDataAndType(Android.Net.Uri.FromFile(new Java.IO.File(apkFilePath)), "application/vnd.android.package-archive");
                intent.SetFlags(ActivityFlags.NewTask);
                intent.AddFlags(ActivityFlags.GrantReadUriPermission);
                Android.App.Application.Context.StartActivity(intent);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception while installing APK: {ex.Message}");
            }
        }
        private async Task<List<CleanupLocation>> FillTheCleanupList()
        {
            try
            {
                var location = await CommonData.GetSettingAsync("DefaultProductionLocation", this);
                List<CleanupLocation> data = new List<CleanupLocation>();
                await Task.Run(async () =>
                {

                    string error;
                    var stock = Services.GetObjectList("strl", out error, location);
                    if (stock == null)
                    {
                        RunOnUiThread(() =>
                        {
                            string WebError = string.Format($"{Resources.GetString(Resource.String.s216)}" + error);

                            Toast.MakeText(this, WebError, ToastLength.Long).Show();
                        });


                    }
                    else
                    {
                        stock.Items.ForEach(x =>
                        {
                            var ident = x.GetString("Ident");
                            var location = x.GetString("Location");
                            var SSCC = x.GetString("SSCC");
                            var Name = x.GetString("Name");
                            var Serial = x.GetString("SerialNo");
                            data.Add(new CleanupLocation { Name = Name, Ident = ident, Location = location, Serial = Serial, SSCC = SSCC });
                        });
                    }



                });
                return data;
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
                return new List<CleanupLocation>();
            }
        }


        private void ButtonRapidTakeover_Click(object? sender, EventArgs e)
        {
            try
            {
                StartActivity(typeof(RapidTakeover));
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        public string GetAppVersion()
        {
            try
            {
                return AppInfo.BuildString;
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
                return string.Empty;
            }
        }
        private void DownloadResources()
        {
            try
            {
                if (!App.Settings.login)
                {
                    var intent = new Intent(this, typeof(CachingService));
                    base.StartService(intent);
                    App.Settings.login = true;
                }
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        public bool IsOnline()
        {
            try
            {
                var cm = (ConnectivityManager)GetSystemService(ConnectivityService);
                return cm.ActiveNetworkInfo == null ? false : cm.ActiveNetworkInfo.IsConnected;
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
                return false;
            }
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

   

      

        private void BtRecalculate_Click(object sender, EventArgs e)
        {
            try
            {
                StartActivity(typeof(RecalculateInventory));
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private void HideDisabled(List<Button> buttons)
        {
            try
            {
                foreach (Button btn in buttons)
                {
                    if (btn.Enabled == false)
                    {
                        btn.SetBackgroundColor(Android.Graphics.Color.DarkGray);
                        btn.SetTextColor(Android.Graphics.Color.White);
                    }
                    else
                    {
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private void PalletsMenu_Click(object sender, EventArgs e)
        {
            try
            {
                StartActivity(typeof(MenuPallets));
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }



        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            try
            {
                switch (keyCode)
                {
                    // In smartphone
                    case Keycode.F1:

                        if (button.Enabled == true)
                        {
                            Button_Click(this, null);

                        }
                        break;

                    // Return true;

                    case Keycode.F2:
                        if (buttonInterWarehouse.Enabled == true)
                        {
                            ButtonInterWarehouse_Click(this, null);
                        }

                        break;


                    case Keycode.F3:
                        if (buttonUnfinished.Enabled == true)
                        {
                            ButtonUnfinished_Click(this, null);
                        }
                        break;

                    case Keycode.F4:
                        if (buttonIssued.Enabled == true)
                        {
                            ButtonIssued_Click(this, null);
                        }
                        break;


                    case Keycode.F5:
                        if (btnPackaging.Enabled == true)
                        {
                            BtnPackaging_Click(this, null);
                        }
                        break;


                    case Keycode.F6:
                        if (buttonPrint.Enabled == true)
                        {
                            ButtonPrint_Click(this, null);
                        }
                        break;

                    case Keycode.F7:
                        if (btnInventory.Enabled == true)
                        {
                            BtnInventory_Click(this, null);
                        }
                        break;
                    case Keycode.F8:
                        if (btnCheckStock.Enabled == true)
                        {
                            BtnCheckStock_Click(this, null);
                        }
                        break;
                        // return true;
                }
                return base.OnKeyDown(keyCode, e);
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
                return false;
            }
        }

        private void BtnLogout_Click(object sender, EventArgs e)
        {
            try
            {
                Intent intent = new Intent(this, typeof(MainActivity));
                intent.SetFlags(ActivityFlags.ClearTop | ActivityFlags.ClearTask | ActivityFlags.NewTask);
                StartActivity(intent);
                Finish();
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

 
        private void BtnPackaging_Click(object sender, EventArgs e)
        {
            try
            {
                StartActivity(typeof(PackagingEnteredPositionsView));
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private void BtnCheckStock_Click(object sender, EventArgs e)
        {
            try
            {
                StartActivity(typeof(CheckStock));
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private void BtnInventory_Click(object sender, EventArgs e)
        {
            try
            {
                StartActivity(typeof(InventoryMenu));
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private void ButtonPrint_Click(object sender, EventArgs e)
        {
            try
            {
                StartActivity(typeof(PrintingMenu));
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private void ButtonIssued_Click(object sender, EventArgs e)
        {
            try
            {
                StartActivity(typeof(UnfinishedIssuedGoodsView));
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }


        private void Button_Click_Without_Inner_Menu(object sender, EventArgs e)
        {
            try
            {
                StartActivity(typeof(UnfinishedTakeoversView));
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }



        private void ButtonUnfinished_Click(object sender, EventArgs e)
        {
            try
            {
                StartActivity(typeof(UnfinishedProductionView));
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private void ButtonInterWarehouse_Click(object sender, EventArgs e)
        {
            try
            {
                StartActivity(typeof(UnfinishedInterWarehouseView));
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private async void Button_Click(object sender, EventArgs e)
        {
            try
            {
                var isShown = await CommonData.GetSettingAsync("UseFastTakeOver", this);
                if (isShown == "1")
                {
                    StartActivity(typeof(Choice));
                }
                else
                {
                    StartActivity(typeof(UnfinishedTakeoversView));
                }
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }
    }
}