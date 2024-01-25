using Android.App;
using Android.Content;
using Android.Net;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using WMS.App;
using Stream = Android.Media.Stream;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrendNET.WMS.Device.Services;
using Xamarin.Essentials;
using WMS.Caching;

using AndroidX.AppCompat.App;using AlertDialog = Android.App.AlertDialog;namespace WMS
{
    [Activity(Label = "MainMenuTablet", ScreenOrientation = Android.Content.PM.ScreenOrientation.Landscape)]
    public class MainMenuTablet : AppCompatActivity
    {

        public static string IDdevice;
        public static string target;
        public bool result;
        private ListView rapidListview;
        private Button rapidTakeover;
        private List<rapidTakeoverList> data = new List<rapidTakeoverList>();
        private Button PalletsMenu;
        private List<Button> buttons = new List<Button>();
        List<CleanupLocation> dataCleanup = new List<CleanupLocation>();
        private CleanupAdapter cleanupAdapter;

        protected async override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetTheme(Resource.Style.AppTheme_NoActionBar);
            SetContentView(Resource.Layout.MainMenuTablet);
            AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
            var _customToolbar = new CustomToolbar(this, toolbar, Resource.Id.navIcon);
            _customToolbar.SetNavigationIcon(settings.RootURL + "/Services/Logo");
            SetSupportActionBar(_customToolbar._toolbar);
            var flag = Services.isTablet(App.settings.device);
            if (MainActivity.isValid == true)
            {
                string toast = new string("Uspešna prijava.");
                Toast.MakeText(this, toast, ToastLength.Long).Show();
                MainActivity.isValid = false;
                MainActivity.progressBar1.Visibility = ViewStates.Invisible;
            }
            IDdevice = settings.ID;
            target = settings.device;
            result = settings.tablet;
            Button button = FindViewById<Button>(Resource.Id.goodsTakeOver);
            button.Click += Button_Click;
            buttons.Add(button);
            Button buttonInterWarehouse = FindViewById<Button>(Resource.Id.goodsInterWarehouse);
            buttonInterWarehouse.Click += ButtonInterWarehouse_Click;
            buttons.Add(buttonInterWarehouse);
            Button buttonUnfinished = FindViewById<Button>(Resource.Id.goodsProduction);
            buttonUnfinished.Click += ButtonUnfinished_Click;
            buttons.Add(buttonUnfinished);
            Button buttonIssued = FindViewById<Button>(Resource.Id.goodsIssued);
            buttonIssued.Click += ButtonIssued_Click;
            buttons.Add(buttonIssued);
            Button buttonPrint = FindViewById<Button>(Resource.Id.btnPrint);
            buttonPrint.Click += ButtonPrint_Click;
            buttons.Add(buttonPrint);
            Button btnInventory = FindViewById<Button>(Resource.Id.btnInventory);
            btnInventory.Click += BtnInventory_Click;
            buttons.Add(btnInventory);
            Button btnCheckStock = FindViewById<Button>(Resource.Id.btCheckStock);
            btnCheckStock.Click += BtnCheckStock_Click;
            buttons.Add(btnCheckStock);
            Button btnPackaging = FindViewById<Button>(Resource.Id.goodsPackaging);
            btnPackaging.Click += BtnPackaging_Click;
            buttons.Add(btnPackaging);
            Button btnLogout = FindViewById<Button>(Resource.Id.logout);
            btnLogout.Click += BtnLogout_Click;
            Button PalletsMenu = FindViewById<Button>(Resource.Id.PalletsMenu);
            buttons.Add(PalletsMenu);
            rapidTakeover = FindViewById<Button>(Resource.Id.rapidTakeover);
            rapidTakeover.Click += RapidTakeover_Click1;
            Button btRecalculate = FindViewById<Button>(Resource.Id.btRecalculate);
            btRecalculate.Click += BtRecalculate_Click;
            PalletsMenu = FindViewById<Button>(Resource.Id.PalletsMenu);
            PalletsMenu.Click += PalletsMenu_Click;
            buttonInterWarehouse.Enabled = Services.HasPermission("TNET_WMS_BLAG_TRN", "R");
            buttonIssued.Enabled = Services.HasPermission("TNET_WMS_BLAG_SND", "R");
            buttonUnfinished.Enabled = Services.HasPermission("TNET_WMS_BLAG_PROD", "R");
            button.Enabled = Services.HasPermission("TNET_WMS_BLAG_ACQ", "R");
            btnPackaging.Enabled = Services.HasPermission("TNET_WMS_BLAG_PKG", "R");
            buttonPrint.Enabled = Services.HasPermission("TNET_WMS_OTHR_PRINT", "R");
            btnInventory.Enabled = Services.HasPermission("TNET_WMS_OTHR_INV", "R");
            PalletsMenu.Enabled = Services.HasPermission("TNET_WMS_BLAG_PAL", "R");
            HideDisabled(buttons);
            rapidListview = FindViewById<ListView>(Resource.Id.rapidListview);
            dataCleanup = await FillTheCleanupList();
            cleanupAdapter = new CleanupAdapter(this, dataCleanup);
            rapidListview.Adapter = cleanupAdapter;
            ProccessRapidTakeover();
            var _broadcastReceiver = new NetworkStatusBroadcastReceiver();
            _broadcastReceiver.ConnectionStatusChanged += OnNetworkStatusChanged;
            Application.Context.RegisterReceiver(_broadcastReceiver,
            new IntentFilter(ConnectivityManager.ConnectivityAction));
            DownloadResources();
            // Track user
            Analytics.TrackEvent($"Login from the id-{settings.ID}, url-{settings.RootURL}, version-0.{GetAppVersion()}");
        }
        public string GetAppVersion()
        {
            return AppInfo.BuildString;
        }

        private void DownloadResources()
        {

            if (!settings.login)
            {
                var intent = new Intent(this, typeof(CachingService));
                StartService(intent);
                settings.login = true;
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
                    Crashes.TrackError(err);
                }
            }
            else
            {
                LoaderManifest.LoaderManifestLoop(this);
            }
        }
        private void ProccessRapidTakeover()
        {
            try
            {
                var isShown = CommonData.GetSetting("UseFastTakeOver");
                if (isShown == "1")
                {
                  
                }
                else
                {
                    rapidTakeover.Visibility = ViewStates.Invisible;
                }
            }
            catch (Exception) { return; }
        }
        private async Task<List<CleanupLocation>> FillTheCleanupList()
        {
            var location = CommonData.GetSetting("DefaultProductionLocation");
            List<CleanupLocation> data = new List<CleanupLocation>();
            await Task.Run(async () =>
            {

                string error;
                var stock = Services.GetObjectList("strl", out error,  location);
                if (stock == null)
                {
                    string WebError = string.Format("Napaka pri preverjanju zaloge." + error);
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
                        data.Add(new CleanupLocation { Ident = x.GetString("Ident"), Location = x.GetString("Location"), SSCC = x.GetString("SSCC"), Name = x.GetString("IdentName"), Serial=x.GetString("SerialNo") });
                    });
                }



            });
            return data;
        }

        private void BtRecalculate_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(RecalculateInventoryTablet));                       
        }

        private void RapidTakeover_Click1(object sender, EventArgs e)
        {
            StartActivity(typeof(RapidTakeover));
                
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

              StartActivity(typeof(MenuPalletsTablet));
              
            
        }

        private void updateList()
        {

        }
        private void RapidTakeover_Click(object sender, EventArgs e)
        {

              StartActivity(typeof(RapidTakeover));
               
            
        }

        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            switch (keyCode)
            {
                // In smart-phone
                case Keycode.F1:

                    Button_Click(this, null);
                    break;
                // Return true;

                case Keycode.F2:
                    ButtonInterWarehouse_Click(this, null);
                    break;
                case Keycode.F3:
                    ButtonUnfinished_Click(this, null);
                    break;
                case Keycode.F4:
                    ButtonIssued_Click(this, null);
                    break;
                case Keycode.F5:
                    ButtonPrint_Click(this, null);
                    break;
                case Keycode.F6:
                    BtnInventory_Click(this, null);
                    break;

                case Keycode.F7:
                    BtnCheckStock_Click(this, null);
                    break;
                case Keycode.F8:
                    BtnCheckStock_Click(this, null);
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

            StartActivity(typeof(PackagingEnteredPositionsViewTablet));
             
            

        }

        private void BtnCheckStock_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(CheckStockTablet));       
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
            StartActivity(typeof(UnfinishedIssuedGoodsViewTablet));                   
        }

        private void ButtonUnfinished_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(UnfinishedProductionViewTablet));                 
        }

        private void ButtonInterWarehouse_Click(object sender, EventArgs e)
        {

            StartActivity(typeof(UnfinishedInterWarehouseViewTablet));
                   
        }

        private void Button_Click(object sender, EventArgs e)
        {

            StartActivity(typeof(UnfinishedTakeoversViewTablet));                     
        }
    }
}