using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Net;
using Android.Views;
using TrendNET.WMS.Device.Services;
using WMS.App;
using WMS.Caching;
using Xamarin.Essentials;
using static Android.App.ActionBar;
namespace WMS
{
    [Activity(Label = "MainMenu", ScreenOrientation = ScreenOrientation.Portrait)]
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

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetTheme(Resource.Style.AppTheme_NoActionBar);
            HelpfulMethods.releaseLock();

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
            buttonInterWarehouse.Enabled = Services.HasPermission("TNET_WMS_BLAG_TRN", "R");
            buttonIssued.Enabled = Services.HasPermission("TNET_WMS_BLAG_SND", "R");
            buttonUnfinished.Enabled = Services.HasPermission("TNET_WMS_BLAG_PROD", "R");
            button.Enabled = Services.HasPermission("TNET_WMS_BLAG_ACQ", "R");
            btnPackaging.Enabled = Services.HasPermission("TNET_WMS_BLAG_PKG", "R");
            buttonPrint.Enabled = Services.HasPermission("TNET_WMS_OTHR_PRINT", "R");
            btnInventory.Enabled = Services.HasPermission("TNET_WMS_OTHR_INV", "R");
            btRecalculate = FindViewById<Button>(Resource.Id.btRecalculate);
            btRecalculate.Click += BtRecalculate_Click;
            PalletsMenu.Enabled = Services.HasPermission("TNET_WMS_BLAG_PAL", "R");
            PalletsMenu.Click += PalletsMenu_Click;
            HideDisabled(buttons);
            var _broadcastReceiver = new NetworkStatusBroadcastReceiver();
            _broadcastReceiver.ConnectionStatusChanged += OnNetworkStatusChanged;
            Application.Context.RegisterReceiver(_broadcastReceiver,
            new IntentFilter(ConnectivityManager.ConnectivityAction));
            Caching.Caching.SavedList = new List<string>();
            DownloadResources();
            SentrySdk.CaptureMessage($"Login from the id-{App.Settings.ID}, url-{App.Settings.RootURL}, version-0.{GetAppVersion()}");
            // Reseting the global update variable.
            Base.Store.isUpdate = false;
            Base.Store.OpenOrder = null;
            Base.Store.byOrder = true;
            Base.Store.code2D = null;

        }


        private async Task<List<CleanupLocation>> FillTheCleanupList()
        {
            var location = CommonData.GetSetting("DefaultProductionLocation");
            List<CleanupLocation> data = new List<CleanupLocation>();
            await Task.Run(async () =>
            {

                string error;
                var stock = Services.GetObjectList("strl", out error, location);
                if (stock == null)
                {
                    string WebError = string.Format($"{Resources.GetString(Resource.String.s216)}" + error);
                    RunOnUiThread(() =>
                    {
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


        private void ButtonRapidTakeover_Click(object? sender, EventArgs e)
        {
            StartActivity(typeof(RapidTakeoverPhone));
        }

        public string GetAppVersion()
        {
            return AppInfo.BuildString;
        }
        private void DownloadResources()
        {

            if (!App.Settings.login)
            {
                var intent = new Intent(this, typeof(CachingService));
                base.StartService(intent);
                App.Settings.login = true;
            }
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

        protected override void OnResume()
        {
            var restartNeeded = App.Settings.restart;

            if (restartNeeded)
            {
                popupDialog = new Dialog(this);
                popupDialog.SetContentView(Resource.Layout.restart);
                popupDialog.Window.SetSoftInputMode(SoftInput.AdjustResize);
                popupDialog.Show();

                popupDialog.Window.SetLayout(LayoutParams.MatchParent, LayoutParams.WrapContent);
                popupDialog.Window.SetBackgroundDrawable(new ColorDrawable(Color.ParseColor("#081a45")));

                // Access Pop-up layout fields like below
                btnOkRestart = popupDialog.FindViewById<Button>(Resource.Id.btnOk);
                btnOkRestart.Click += BtnOkRestart_Click;

            }

            base.OnResume();
        }

        private void BtnOkRestart_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }

        private void BtRecalculate_Click(object sender, EventArgs e)
        {

            StartActivity(typeof(RecalculateInventory));
        }

        private void HideDisabled(List<Button> buttons)
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

        private void PalletsMenu_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(MenuPallets));
        }



        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
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

        private void BtnLogout_Click(object sender, EventArgs e)
        {


            Intent intent = new Intent(this, typeof(MainActivity));
            intent.SetFlags(ActivityFlags.ClearTop | ActivityFlags.ClearTask | ActivityFlags.NewTask);
            StartActivity(intent);
            Finish();
        }

        private void BtnPackaging_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(PackagingEnteredPositionsView));
        }

        private void BtnCheckStock_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(CheckStock));
        }

        private void BtnInventory_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(InventoryMenu));
        }

        private void ButtonPrint_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(PrintingMenu));

        }

        private void ButtonIssued_Click(object sender, EventArgs e)
        {

            StartActivity(typeof(UnfinishedIssuedGoodsView));

        }


        private void Button_Click_Without_Inner_Menu(object sender, EventArgs e)
        {

            StartActivity(typeof(UnfinishedTakeoversView));

        }



        private void ButtonUnfinished_Click(object sender, EventArgs e)
        {

            StartActivity(typeof(UnfinishedProductionView));
        }

        private void ButtonInterWarehouse_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(UnfinishedInterWarehouseView));
        }

        private void Button_Click(object sender, EventArgs e)
        {

            var isShown = CommonData.GetSetting("UseFastTakeOver");
            if (isShown == "1")
            {
                StartActivity(typeof(Choice));
            }
            else
            {
                StartActivity(typeof(UnfinishedTakeoversView));
            }

        }
    }
}