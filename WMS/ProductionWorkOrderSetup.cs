using Android.Content;
using Android.Content.PM;
using Android.Media;
using Android.Net;
using Android.Views;
using Android.Views.InputMethods;
using BarCode2D_Receiver;
using TrendNET.WMS.Core.Data;
using TrendNET.WMS.Device.App;
using TrendNET.WMS.Device.Services;
using WMS.App;
namespace WMS
{
    [Activity(Label = "ProductionWorkOrderSetup", ScreenOrientation = ScreenOrientation.Portrait)]
    public class ProductionWorkOrderSetup : CustomBaseActivity, IBarcodeResult
    {
        private NameValueObject moveHead = null;
        private NameValueObject ident = null;
        private EditText tbWorkOrder;
        private EditText tbOpenQty;
        private EditText tbClient;
        private EditText tbIdent;
        private EditText tbName;
        private Button check;
        private Button btCard;
        private Button btConfirm;
        private Button btPalette;
        private Button button2;
        SoundPool soundPool;
        int soundPoolId;
        private Barcode2D barcode2D;

        public async void GetBarcode(string barcode)
        {
            if (tbWorkOrder.HasFocus)
            {
                if (barcode != "Scan fail")
                {

                    tbWorkOrder.Text = "";
                    tbOpenQty.Text = "";
                    tbIdent.Text = "";
                    tbName.Text = "";

                    tbWorkOrder.Text = barcode;
                    await ProcessWorkOrder();
                }
                else
                {
                    tbWorkOrder.Text = "";
                    tbWorkOrder.RequestFocus();
                }
            }
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetTheme(Resource.Style.AppTheme_NoActionBar);
            if (App.Settings.tablet)
            {
                base.RequestedOrientation = ScreenOrientation.Landscape;
                base.SetContentView(Resource.Layout.ProductionWorkOrderSetupTablet);
            }
            else
            {
                base.RequestedOrientation = ScreenOrientation.Portrait;
                base.SetContentView(Resource.Layout.ProductionWorkOrderSetup);
            }
            AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
            var _customToolbar = new CustomToolbar(this, toolbar, Resource.Id.navIcon);
            _customToolbar.SetNavigationIcon(App.Settings.RootURL + "/Services/Logo");
            SetSupportActionBar(_customToolbar._toolbar);
            SupportActionBar.SetDisplayShowTitleEnabled(false);
            // button next
            tbWorkOrder = FindViewById<EditText>(Resource.Id.tbWorkOrder);
            tbOpenQty = FindViewById<EditText>(Resource.Id.tbOpenQty);
            tbClient = FindViewById<EditText>(Resource.Id.tbClient);
            tbIdent = FindViewById<EditText>(Resource.Id.tbIdent);
            tbName = FindViewById<EditText>(Resource.Id.tbName);
            btCard = FindViewById<Button>(Resource.Id.btCard);
            btConfirm = FindViewById<Button>(Resource.Id.btConfirm);
            btPalette = FindViewById<Button>(Resource.Id.btPalette);
            button2 = FindViewById<Button>(Resource.Id.button2);
            color();
            tbOpenQty.FocusChange += TbOpenQty_FocusChange;
            btCard.Click += BtCard_Click;
            btConfirm.Click += BtConfirm_Click;
            btPalette.Click += BtPalette_Click;
            button2.Click += Button2_Click;
            tbWorkOrder.RequestFocus();
            tbOpenQty.Text = "";
            tbClient.Text = "";
            tbIdent.Text = "";
            tbName.Text = "";
            btConfirm.Visibility = ViewStates.Gone;
            btCard.Visibility = ViewStates.Gone;
            btPalette.Visibility = ViewStates.Gone;
            barcode2D = new Barcode2D(this, this);
            tbOpenQty.Enabled = false;
            tbClient.Enabled = false;
            tbIdent.Enabled = false;
            tbName.Enabled = false;
            tbOpenQty.Focusable = true;
            tbClient.Focusable = true;
            tbIdent.Focusable = true;
            tbName.Focusable = false;


            var _broadcastReceiver = new NetworkStatusBroadcastReceiver();
            _broadcastReceiver.ConnectionStatusChanged += OnNetworkStatusChanged;
            Application.Context.RegisterReceiver(_broadcastReceiver,

            new IntentFilter(ConnectivityManager.ConnectivityAction));


            tbWorkOrder.EditorAction += (sender, e) =>
            {
                if (e.ActionId == ImeAction.Done || e.Event.Action == KeyEventActions.Down)
                {
                    await ProcessWorkOrder();
                }
            };



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
        private async void TbOpenQty_FocusChange(object sender, View.FocusChangeEventArgs e)
        {
            await ProcessWorkOrder();
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(MainMenu));
            Finish();
        }

        private void color()
        {
            tbWorkOrder.SetBackgroundColor(Android.Graphics.Color.Aqua);
        }

        private void BtPalette_Click(object sender, EventArgs e)
        {
            if (ident != null)
            {
                var cardInfo = new NameValueObject("CardInfo");
                cardInfo.SetString("WorkOrder", tbWorkOrder.Text.Trim());
                cardInfo.SetString("Ident", tbIdent.Text.Trim());
                cardInfo.SetDouble("UM1toUM2", ident.GetDouble("UM1toUM2"));
                cardInfo.SetDouble("UM1toUM3", ident.GetDouble("UM1toUM3"));
                InUseObjects.Set("CardInfo", cardInfo);
                StartActivity(typeof(ProductionPalette));
                Finish();
            }
        }

        private async void BtConfirm_Click(object sender, EventArgs e)
        {
            if (await SaveMoveHead())
            {
                StartActivity(typeof(ProductionSerialOrSSCCEntry));
                Finish();
            }
        }

        private void BtCard_Click(object sender, EventArgs e)
        {
            var cardInfo = new NameValueObject("CardInfo");
            cardInfo.SetString("WorkOrder", tbWorkOrder.Text.Trim());
            cardInfo.SetString("Ident", tbIdent.Text.Trim());
            cardInfo.SetDouble("UM1toUM2", ident.GetDouble("UM1toUM2"));
            cardInfo.SetDouble("UM1toUM3", ident.GetDouble("UM1toUM3"));
            InUseObjects.Set("CardInfo", cardInfo);
            StartActivity(typeof(ProductionCard));
            Finish();
        }


        private async Task<bool> SaveMoveHead()
        {
            NameValueObject workOrder = null;

            try
            {

                string error;
                workOrder = Services.GetObject("wo", tbWorkOrder.Text.Trim(), out error);
                if (workOrder == null)
                {
                    string SuccessMessage = string.Format($"{Resources.GetString(Resource.String.s299)}");
                    Toast.MakeText(this, SuccessMessage, ToastLength.Long).Show();
                    return false;
                }
            }
            catch (Exception err)
            {

                SentrySdk.CaptureException(err);
                return false;

            }

            var ident = await CommonData.LoadIdentAsync(tbIdent.Text.Trim(), this);
            if (ident == null) { return false; }

            if (moveHead == null) { moveHead = new NameValueObject("MoveHead"); }
            if (!moveHead.GetBool("Saved"))
            {

                try
                {

                    string error;
                    var productionWarehouse = Services.GetObject("pw", workOrder.GetString("DocumentType") + "|" + ident.GetString("Set"), out error);


                    if ((productionWarehouse == null) || (string.IsNullOrEmpty(productionWarehouse.GetString("ProductionWarehouse"))))
                    {
                        string SuccessMessage = string.Format($"{Resources.GetString(Resource.String.s316)}");
                        Toast.MakeText(this, SuccessMessage, ToastLength.Long).Show();
                        return false;
                    }


                    moveHead.SetInt("Clerk", Services.UserID());
                    moveHead.SetString("Type", "W");
                    moveHead.SetString("LinkKey", workOrder.GetString("Key"));
                    moveHead.SetString("LinkNo", workOrder.GetString("No"));
                    moveHead.SetString("Document1", "");
                    moveHead.SetDateTime("Document1Date", null);
                    moveHead.SetString("Note", "");
                    moveHead.SetString("Issuer", "");
                    moveHead.SetString("Receiver", "");
                    moveHead.SetString("Wharehouse", productionWarehouse.GetString("ProductionWarehouse"));
                    moveHead.SetString("DocumentType", workOrder.GetString("DocumentType"));

                    var savedMoveHead = Services.SetObject("mh", moveHead, out error);
                    if (savedMoveHead == null)
                    {
                        string SuccessMessage = string.Format($"{Resources.GetString(Resource.String.s213)}" + error);
                        Toast.MakeText(this, SuccessMessage, ToastLength.Long).Show();
                        return false;
                    }
                    else
                    {
                        moveHead.SetInt("HeadID", savedMoveHead.GetInt("HeadID"));
                        moveHead.SetBool("Saved", true);
                        InUseObjects.Set("MoveHead", moveHead);
                        return true;
                    }
                }
                catch (Exception err)
                {

                    SentrySdk.CaptureException(err);
                    return false;

                }
            }
            else
            {
                return true;
            }
        }


        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            switch (keyCode)
            {
                // in smartphone
                case Keycode.F2:
                    if (btCard.Enabled == true)
                    {
                        BtCard_Click(this, null);
                    }
                    break;

                case Keycode.F3:
                    if (btConfirm.Enabled == true)
                    {
                        BtConfirm_Click(this, null);
                    }
                    break;


                case Keycode.F4:
                    if (btPalette.Enabled == true)
                    {
                        BtPalette_Click(this, null);
                    }
                    break;

            }
            return base.OnKeyDown(keyCode, e);
        }

        private async Task ProcessWorkOrder()
        {
            tbOpenQty.Text = "";
            tbClient.Text = "";
            tbIdent.Text = "";
            tbName.Text = "";


            btConfirm.Visibility = ViewStates.Gone;
            btCard.Visibility = ViewStates.Gone;
            btPalette.Visibility = ViewStates.Gone;

            try
            {
                string error;
                NameValueObject workOrder = Services.GetObject("wo", tbWorkOrder.Text.Trim(), out error);
                if (workOrder == null)
                {
                    string SuccessMessage = string.Format($"{Resources.GetString(Resource.String.s216)}" + error);
                    Toast.MakeText(this, SuccessMessage, ToastLength.Long).Show();

                }
                else
                {
                    ident = Services.GetObject("id", workOrder.GetString("Ident"), out error);
                    if (ident == null)
                    {
                        string SuccessMessage = string.Format($"{Resources.GetString(Resource.String.s216)}" + error);
                        Toast.MakeText(this, SuccessMessage, ToastLength.Long).Show();
                    }
                    else
                    {
                        tbOpenQty.Text = workOrder.GetDouble("OpenQty").ToString(await CommonData.GetQtyPictureAsync(this));
                        tbClient.Text = workOrder.GetString("Consignee");
                        tbIdent.Text = workOrder.GetString("Ident");
                        tbName.Text = workOrder.GetString("Name");

                        if (await CommonData.GetSettingAsync("ProductionIgnoreIdentCardInfo", this) == "1")
                        {
                            btCard.Visibility = ViewStates.Visible;
                            btPalette.Visibility = ViewStates.Visible;
                            btConfirm.Visibility = ViewStates.Visible;
                        }
                        else
                        {
                            if (ident.GetString("ProcessingMode").ToLower().Contains("karton"))
                            {
                                btCard.Visibility = ViewStates.Visible;
                                btPalette.Visibility = ViewStates.Visible;
                            }
                            else
                            {
                                btConfirm.Visibility = ViewStates.Visible;
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                SentrySdk.CaptureException(err);
            }

        }



    }
}