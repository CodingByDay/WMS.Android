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
using WMS.ExceptionStore;
namespace WMS
{
    [Activity(Label = "WMS")]
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
        private ApiResultSet? operations;
        private Button btNext;
        private TextView lbInfo;
        private EditText tbOperation;
        private int currentOperationIndex = 1;
        private long? operationId;
        private LinearLayout entireExtraButtonRow;
        private Row operation;

        public async void GetBarcode(string barcode)
        {
            try
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
            catch (Exception ex)
            {
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
                // Button next
                tbWorkOrder = FindViewById<EditText>(Resource.Id.tbWorkOrder);
                tbOpenQty = FindViewById<EditText>(Resource.Id.tbOpenQty);
                tbClient = FindViewById<EditText>(Resource.Id.tbClient);
                tbIdent = FindViewById<EditText>(Resource.Id.tbIdent);
                tbName = FindViewById<EditText>(Resource.Id.tbName);
                btCard = FindViewById<Button>(Resource.Id.btCard);
                btConfirm = FindViewById<Button>(Resource.Id.btConfirm);
                btPalette = FindViewById<Button>(Resource.Id.btPalette);
                button2 = FindViewById<Button>(Resource.Id.button2);
                btNext = FindViewById<Button>(Resource.Id.btNext);
                lbInfo = FindViewById<TextView>(Resource.Id.lbInfo);
                tbOperation = FindViewById<EditText>(Resource.Id.tbOperation);
                entireExtraButtonRow = FindViewById<LinearLayout>(Resource.Id.entireExtraButtonRow);
                entireExtraButtonRow.Visibility = ViewStates.Gone;
                btConfirm.Visibility = ViewStates.Gone;
                color();
                tbOpenQty.FocusChange += TbOpenQty_FocusChange;
                btCard.Click += BtCard_Click;
                btConfirm.Click += BtConfirm_Click;
                btPalette.Click += BtPalette_Click;
                button2.Click += Button2_Click;
                btNext.Click += BtNext_Click;
                tbWorkOrder.RequestFocus();
                tbOpenQty.Text = "";
                tbClient.Text = "";
                tbIdent.Text = "";
                tbName.Text = "";
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

                new IntentFilter(ConnectivityManager.ConnectivityAction), ReceiverFlags.NotExported);


                tbWorkOrder.EditorAction += async (sender, e) =>
                {
                    if (e.ActionId == ImeAction.Done || e.Event.Action == KeyEventActions.Down)
                    {
                        await ProcessWorkOrder();
                    }
                };

            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }

        }

        private async void BtNext_Click(object? sender, EventArgs e)
        {
            if (currentOperationIndex < operations.Rows.Count)
            {
                currentOperationIndex += 1;
            } else
            {
                currentOperationIndex = 1;
            }
            await ShowOperationAtIndex();
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
        private async void TbOpenQty_FocusChange(object sender, View.FocusChangeEventArgs e)
        {
            try
            {
                await ProcessWorkOrder();
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private void Button2_Click(object sender, EventArgs e)
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

        private void color()
        {
            try
            {
                tbWorkOrder.SetBackgroundColor(Android.Graphics.Color.Aqua);
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private void BtPalette_Click(object sender, EventArgs e)
        {
            try
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
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private async void BtConfirm_Click(object sender, EventArgs e)
        {
            try
            {
                if (await SaveMoveHead())
                {
                    var intent = new Intent(this, typeof(ProductionSerialOrSSCCEntry));
                    intent.PutExtra("Qty", operation.DoubleValue("OPENQTY").ToString());
                    intent.PutExtra("OperationId", operationId.ToString()); 
                    StartActivity(intent);
                    Finish();
                }
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private void BtCard_Click(object sender, EventArgs e)
        {
            try
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
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }


        private async Task<bool> SaveMoveHead()
        {
            try
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
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
                return false;
            }
        }


        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            try
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
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
                return false;
            }
        }

        private async Task ProcessWorkOrder()
        {
            try
            {
                string workorder = tbWorkOrder.Text.Trim();

                if(String.IsNullOrWhiteSpace(workorder))
                {
                    return;
                }

                var parameters = new List<Services.Parameter>();

                string sql = $"SELECT O.acKey AS ACKEY, W.acConsignee, W.acIdent, W.acName, O.anPlanQty - ISNULL(O.anProducedQty, 0) AS OPENQTY, W.acDocType, O.adSchedEndTime AS ADSCHEDENDTIME, O.acIdentOper, O.acNameOper, O.anWOExItemID FROM uWMSOpenWOOper O JOIN tHF_WOEx W ON O.acKey = W.acKey WHERE O.acKey = @acKey;";

                parameters.Add(new Services.Parameter { Name = "acKey", Type = "String", Value = workorder });

                operations = await AsyncServices.AsyncServices.GetObjectListBySqlAsync(sql, parameters);

                await ShowOperationAtIndex();

            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private async Task ShowOperationAtIndex()
        {
            try
            {
                LoaderManifest.LoaderManifestLoopResources(this);

                operation = operations.Rows.ElementAt(currentOperationIndex - 1);
                if (operation != null && operation.Items.Count > 0)
                {
                    operationId = operation.IntValue("anWOExItemID");
                    lbInfo.Text = $"{Resources.GetString(Resource.String.s364)}: {currentOperationIndex}/{operations.Rows.Count}";
                    tbOpenQty.Text = operation.DoubleValue("OPENQTY").ToString();
                    tbClient.Text = operation.StringValue("acConsignee");
                    tbIdent.Text = operation.StringValue("acIdent");
                    tbName.Text = operation.StringValue("acName");
                    string identOperation = operation.StringValue("acIdentOper");
                    string operationName = string.Empty;
                    if (!String.IsNullOrEmpty(identOperation))
                    {
                        operationName += identOperation;
                        if (!String.IsNullOrEmpty(tbName.Text))
                        {
                            operationName += "-" + tbName.Text;
                        }
                    }
                    else
                    {
                        operationName += "X";
                        if (!String.IsNullOrEmpty(tbName.Text))
                        {
                            operationName += "-" + tbName.Text;
                        }
                    }

                    tbOperation.Text = operationName;

                    if (await CommonData.GetSettingAsync("ProductionIgnoreIdentCardInfo", this) == "1")
                    {
                        entireExtraButtonRow.Visibility = ViewStates.Visible;
                        btConfirm.Visibility = ViewStates.Visible;
                    }
                    else
                    {
                        string error;
                        ident = Services.GetObject("id", operation.StringValue("acIdent"), out error);
                        if (ident == null)
                        {
                            string SuccessMessage = string.Format($"{Resources.GetString(Resource.String.s216)}" + error);
                            Toast.MakeText(this, SuccessMessage, ToastLength.Long).Show();
                        }
                        if (ident.GetString("ProcessingMode").ToLower().Contains("karton"))
                        {
                            entireExtraButtonRow.Visibility = ViewStates.Visible;
                        }
                        else
                        {
                            btConfirm.Visibility = ViewStates.Visible;
                        }

                    }
                }
            } catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            } finally
            {
                LoaderManifest.LoaderManifestLoopStop(this);
            }
        }
    }
}