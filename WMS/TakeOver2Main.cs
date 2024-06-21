using Android.Content;
using Android.Content.PM;
using Android.Media;
using Android.Net;
using Android.Views;
using BarCode2D_Receiver;
using TrendNET.WMS.Core.Data;
using TrendNET.WMS.Device.App;
using TrendNET.WMS.Device.Services;
using WMS.App;
using WMS.ExceptionStore;
using WMS.Printing;

namespace WMS
{
    [Activity(Label = "TakeOver2Main", ScreenOrientation = ScreenOrientation.Portrait)]
    public class TakeOver2Main : CustomBaseActivity, IBarcodeResult
    {
        SoundPool soundPool;
        int soundPoolId;
        private Barcode2D barcode2D;
        private EditText tbIdent;
        private EditText tbNaziv;
        private EditText tbKolicinaDoSedaj;
        private EditText tbKolicinaNova;
        private Button btnOrder;
        private Button btConfirm;
        private Button btSSCC;
        private Button btOverview;
        private Button btExit;
        private EditText tbLocation;
        private NameValueObject moveHead = (NameValueObject)InUseObjects.Get("MoveHead");
        private NameValueObject moveItem = (NameValueObject)InUseObjects.Get("MoveItem");

        public async void GetBarcode(string barcode)
        {
            try
            {
                if (tbIdent.HasFocus)
                {

                    tbIdent.Text = barcode;
                    await ProcessIdent();
                }
                else if (tbLocation.HasFocus)
                {

                    tbLocation.Text = barcode;
                }
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }
        private async Task ProcessIdent()
        {
            try
            {
                try
                {
                    var ident = await CommonData.LoadIdentAsync(tbIdent.Text.Trim(), this);
                    if (ident == null)
                    {

                        tbIdent.Text = "";
                        tbIdent.RequestFocus();
                        return;
                    }


                    try
                    {
                        string error;
                        var mhID = moveHead.GetInt("HeadID").ToString();
                        var mis = Services.GetObjectList("mi", out error, mhID);
                        if (mis == null)
                        {
                            Toast.MakeText(this, $"{Resources.GetString(Resource.String.s216)}" + error, ToastLength.Long).Show();

                            return;
                        }

                        var existing = mis.Items.FirstOrDefault(mi => mi.GetString("Ident") == ident.GetString("Code"));
                        if (existing != null)
                        {
                            moveItem = existing;
                        }
                    }
                    catch (Exception err)
                    {

                        SentrySdk.CaptureException(err);
                        return;

                    }

                    tbIdent.Text = ident.GetString("Code");
                    tbNaziv.Text = ident.GetString("Name");
                    tbKolicinaDoSedaj.Text = moveItem == null ? "" : moveItem.GetDouble("Qty").ToString("###,###,##0.00");
                    tbKolicinaNova.Text = "";

                }
                catch (Exception ex)
                {
                    SentrySdk.CaptureException(ex);
                    return;
                }
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }
        private async Task<bool> SaveHead()
        {
            try
            {
                try
                {
                    if (!moveHead.GetBool("Saved"))
                    {
                        moveHead.SetString("DocumentType", await CommonData.GetSettingAsync("DirectTakeOverDocType", this));

                        if (string.IsNullOrEmpty(moveHead.GetString("DocumentType")))
                        {
                            StartActivity(typeof(MainMenu));
                            Finish();
                        }
                        moveHead.SetString("Wharehouse", await CommonData.GetSettingAsync("DefaultWarehouse", this));
                        moveHead.SetBool("ByOrder", false);
                        moveHead.SetInt("Clerk", Services.UserID());
                        moveHead.SetString("Type", "I");
                        moveHead.SetString("LinkKey", "");
                        string error;
                        var savedMoveHead = Services.SetObject("mh", moveHead, out error);
                        if (savedMoveHead == null)
                        {
                            Toast.MakeText(this, $"{Resources.GetString(Resource.String.s216)}" + error, ToastLength.Long).Show();

                            return false;
                        }
                        else
                        {
                            moveHead.SetInt("HeadID", savedMoveHead.GetInt("HeadID"));
                            moveHead.SetBool("Saved", true);
                        }
                    }

                    return true;

                }
                catch (Exception ex)
                {
                    SentrySdk.CaptureException(ex);
                    return false;
                }
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
                return false;
            }
        }
        private async Task<NameValueObject?> SaveItem(bool allowEmpty)
        {
            try
            {
                if (allowEmpty && string.IsNullOrEmpty(tbIdent.Text.Trim()))
                {
                    return null;
                }

                if (!await CommonData.IsValidLocationAsync(await CommonData.GetSettingAsync("DefaultWarehouse", this), tbLocation.Text.Trim(), this))
                {
                    Toast.MakeText(this, $"{Resources.GetString(Resource.String.s270)}", ToastLength.Long).Show();
                    return null;
                }

                if (await SaveHead())
                {
                    if (string.IsNullOrEmpty(tbIdent.Text.Trim()))
                    {
                        Toast.MakeText(this, $"{Resources.GetString(Resource.String.s270)}", ToastLength.Long).Show();

                        return null;
                    }

                    var ident = await CommonData.LoadIdentAsync(tbIdent.Text.Trim(), this);
                    if (ident == null) { return null; }

                    double kol;
                    try
                    {
                        var kolDoSedajStr = tbKolicinaDoSedaj.Text.Trim();
                        var kolDoSedaj = string.IsNullOrEmpty(kolDoSedajStr) ? 0.0 : Convert.ToDouble(kolDoSedajStr);
                        var kolNovaStr = tbKolicinaNova.Text.Trim();
                        var kolNova = string.IsNullOrEmpty(kolNovaStr) ? 0.0 : Convert.ToDouble(kolNovaStr);
                        if (kolNova == 0.0)
                        {
                            Toast.MakeText(this, $"{Resources.GetString(Resource.String.s270)}", ToastLength.Long).Show();

                            return null;
                        }
                        kol = kolDoSedaj + kolNova;
                        if (kol < 0.0)
                        {
                            Toast.MakeText(this, $"{Resources.GetString(Resource.String.s325)}", ToastLength.Long).Show();

                            return null;
                        }
                    }
                    catch
                    {

                        tbKolicinaNova.RequestFocus();
                        return null;
                    }
                    try
                    {
                        if (moveItem == null) { moveItem = new NameValueObject("MoveItem"); }
                        moveItem.SetInt("HeadID", moveHead.GetInt("HeadID"));
                        moveItem.SetString("LinkKey", "");
                        moveItem.SetString("Ident", ident.GetString("Code"));
                        moveItem.SetDouble("Packing", 0.0);
                        moveItem.SetDouble("Factor", 1.0);
                        moveItem.SetDouble("Qty", kol);
                        moveItem.SetInt("MorePrints", 0);
                        moveItem.SetInt("Clerk", Services.UserID());
                        moveItem.SetString("Location", tbLocation.Text.Trim());
                        moveItem.SetInt("UserID", Services.UserID());
                        moveItem.SetString("DeviceID", App.Settings.ID);

                        InUseObjects.Set("MoveItem", moveItem);

                        string error;
                        moveItem = Services.SetObject("mi", moveItem, out error);
                        if (moveItem == null)
                        {
                            Toast.MakeText(this, $"{Resources.GetString(Resource.String.s213)}" + error, ToastLength.Long).Show();

                            return null;
                        }

                        return ident;
                    }
                    catch (Exception err)
                    {
                        SentrySdk.CaptureException(err);
                        return null;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
                return null;
            }
        }



        private void color()
        {
            try
            {
                tbIdent.SetBackgroundColor(Android.Graphics.Color.Aqua);
                tbLocation.SetBackgroundColor(Android.Graphics.Color.Aqua);
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }
        protected override async void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                SetTheme(Resource.Style.AppTheme_NoActionBar);
                if (App.Settings.tablet)
                {
                    base.RequestedOrientation = ScreenOrientation.Landscape;
                    base.SetContentView(Resource.Layout.TakeOver2MainTablet);
                }
                else
                {
                    base.RequestedOrientation = ScreenOrientation.Portrait;
                    base.SetContentView(Resource.Layout.TakeOver2Main);
                }
                AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
                var _customToolbar = new CustomToolbar(this, toolbar, Resource.Id.navIcon);
                _customToolbar.SetNavigationIcon(App.Settings.RootURL + "/Services/Logo");
                SetSupportActionBar(_customToolbar._toolbar);
                SupportActionBar.SetDisplayShowTitleEnabled(false);
                tbIdent = FindViewById<EditText>(Resource.Id.tbIdent);
                tbNaziv = FindViewById<EditText>(Resource.Id.tbNaziv);
                tbNaziv.Enabled = false;
                tbKolicinaDoSedaj = FindViewById<EditText>(Resource.Id.tbKolicinaDoSedaj);
                tbKolicinaNova = FindViewById<EditText>(Resource.Id.tbKolicinaNova);
                btnOrder = FindViewById<Button>(Resource.Id.btnOrder);
                btConfirm = FindViewById<Button>(Resource.Id.btConfirm);
                btSSCC = FindViewById<Button>(Resource.Id.btSSCC);
                btOverview = FindViewById<Button>(Resource.Id.btOverview);
                btExit = FindViewById<Button>(Resource.Id.btExit);
                tbLocation = FindViewById<EditText>(Resource.Id.tbLocation);
                barcode2D = new Barcode2D(this, this);
                btnOrder.Click += BtnOrder_Click;
                btConfirm.Click += BtConfirm_Click;
                btSSCC.Click += BtSSCC_Click;
                btOverview.Click += BtOverview_Click;
                btExit.Click += BtExit_Click;
                tbIdent.KeyPress += TbIdent_KeyPress;
                tbLocation.Text = await CommonData.GetSettingAsync("DefaultPaletteLocation", this);
                color();
                if (moveItem != null)
                {

                    var ident = await CommonData.LoadIdentAsync(moveItem.GetString("Ident"), this);

                    if (ident == null)
                    {
                        Toast.MakeText(this, "Ident no longer supported!?", ToastLength.Long).Show();

                    }

                    tbIdent.Text = ident.GetString("Code");
                    tbNaziv.Text = ident.GetString("Name");
                    tbKolicinaDoSedaj.Text = moveItem == null ? "" : moveItem.GetDouble("Qty").ToString("###,###,##0.00");
                    tbKolicinaNova.Text = "";
                }
                var _broadcastReceiver = new NetworkStatusBroadcastReceiver();
                _broadcastReceiver.ConnectionStatusChanged += OnNetworkStatusChanged;
                Application.Context.RegisterReceiver(_broadcastReceiver,
                new IntentFilter(ConnectivityManager.ConnectivityAction), ReceiverFlags.NotExported);


                tbIdent.RequestFocus();
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private void BtExit_Click(object? sender, EventArgs e)
        {
            try
            {
                StartActivity(typeof(MainMenu));
                Finish();
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private async void BtOverview_Click(object? sender, EventArgs e)
        {
            try
            {
                await SaveItem(true);
                StartActivity(typeof(TakeOverEnteredPositionsView));
                Finish();
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private async void BtSSCC_Click(object? sender, EventArgs e)
        {
            try
            {
                if (await SaveItem(false) != null)
                {

                    try
                    {

                        var nvo = new NameValueObject("InternalSticker");
                        PrintingCommon.SetNVOCommonData(ref nvo);
                        nvo.SetString("Ident", tbIdent.Text);
                        PrintingCommon.SendToServer(nvo);
                        Toast.MakeText(this, "Success. ", ToastLength.Long).Show();

                    }
                    catch (Exception ex)
                    {
                        SentrySdk.CaptureException(ex);
                    }
                }
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private async void BtConfirm_Click(object? sender, EventArgs e)
        {
            try
            {
                if (await SaveItem(false) != null)
                {
                    InUseObjects.Set("MoveItem", null);
                    StartActivity(typeof(TakeOver2Main));
                    Finish();
                }
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private async void BtnOrder_Click(object? sender, EventArgs e)
        {
            try
            {
                if (await SaveItem(false) != null)
                {
                    InUseObjects.Set("MoveItem", moveItem);
                    StartActivity(typeof(TakeOver2Orders));
                    Finish();
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
        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            try
            {
                switch (keyCode)
                {
                    // in smartphone        
                    case Keycode.F1:
                        if (btnOrder.Enabled == true)
                        {
                            BtnOrder_Click(this, null);
                        }
                        break;


                    case Keycode.F2:
                        if (btConfirm.Enabled == true)
                        {
                            BtConfirm_Click(this, null);
                        }
                        break;

                    case Keycode.F3:
                        if (btSSCC.Enabled == true)
                        {
                            BtSSCC_Click(this, null);
                        }
                        break;

                    case Keycode.F4:
                        if (btOverview.Enabled == true)
                        {
                            BtOverview_Click(this, null);
                        }
                        break;


                    case Keycode.F8:
                        if (btExit.Enabled == true)
                        {
                            BtExit_Click(this, null);
                        }
                        break;


                }
                return base.OnKeyDown(keyCode, e);
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
                return false;
            }
        }
        private async void TbIdent_KeyPress(object sender, View.KeyEventArgs e)
        {
            try
            {
                e.Handled = false;
                if (e.Event.Action == KeyEventActions.Down && e.KeyCode == Keycode.Enter)
                {
                    // Add your logic here. 
                    await ProcessIdent();
                    e.Handled = true;

                }
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }








    }
}