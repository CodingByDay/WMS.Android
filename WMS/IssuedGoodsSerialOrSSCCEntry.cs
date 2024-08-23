using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Media;
using Android.Net;
using Android.Views;
using BarCode2D_Receiver;
using Com.Jsibbold.Zoomage;
using Newtonsoft.Json;
using TrendNET.WMS.Core.Data;
using TrendNET.WMS.Device.App;
using TrendNET.WMS.Device.Services;
using WMS.App;
using WMS.ExceptionStore;
using static Android.App.ActionBar;
using static WMS.App.MultipleStock;
using AlertDialog = Android.App.AlertDialog;
using WebApp = TrendNET.WMS.Device.Services.WebApp;

namespace WMS
{
    [Activity(Label = "WMS")]
    public class IssuedGoodsSerialOrSSCCEntry : CustomBaseActivity, IBarcodeResult
    {
        private static bool? checkIssuedOpenQty = null;
        private MorePalletsAdapter adapter;
        private MorePalletsAdapter adapterNew;
        private Button btConfirm;
        private Button btCreate;
        private Button btCreateSame;
        private Button btExit;
        private SearchableSpinner? searchableSpinnerIssueLocation;
        private Button btFinish;
        private Button btnNo;
        private Button btnNoConfirm;
        private Button btnYes;
        private Button btnYesConfirm;
        private Button btOverview;
        private int check;
        private List<IssuedGoods> connectedPositions = new List<IssuedGoods>();
        private bool createPositionAllowed = false;
        private CustomAutoCompleteAdapter<string> DataAdapter;
        private NameValueObject dataObject;
        private string error;
        private MorePallets existsDuplicate;
        private NameValueObject extraData = (NameValueObject)InUseObjects.Get("ExtraData");
        private string ident;
        private bool isBatch;
        private bool isFirst;
        private bool isMorePalletsMode = false;
        private bool isOkayToCallBarcode;
        private bool isOpened = false;
        private bool isPackaging = false;
        private NameValueObject lastItem = (NameValueObject)InUseObjects.Get("LastItem");
        private TextView lbPalette;
        private TextView lbQty;
        private List<string> locations = new List<string>();
        private ListView lvCardMore;
        private NameValueObject moveHead = (NameValueObject)InUseObjects.Get("MoveHead");
        private NameValueObject moveItem = (NameValueObject)InUseObjects.Get("MoveItem");
        private NameValueObject moveItemNew;
        private NameValueObject openIdent = (NameValueObject)InUseObjects.Get("OpenIdent");
        private NameValueObject openOrder = (NameValueObject)InUseObjects.Get("OpenOrder");
        private ApiResultSet OpenOrderItem = (ApiResultSet)InUseObjects.Get("OpenOrderItem");
        private Dialog popupDialog;
        private Dialog popupDialogConfirm;
        private Dialog popupDialogMain;
        private Dialog popupDialogMainIssueing;
        private double qtyCheck = 0;
        private string qtyStock;
        private string query;
        private Trail receivedTrail;
        private ApiResultSet result;
        private LinearLayout serialRow;
        private SoundPool soundPool;
        private int soundPoolId;
        private Barcode2D barcode2D;
        private string sscc;
        private LinearLayout ssccRow;
        private double stock;
        private EditText tbIdent;
        private EditText tbPacking;
        private EditText tbSerialNum;
        private EditText tbSSCC;
        private EditText tbSSCCpopup;
        private string warehouse;
        private List<IssuedGoods> data = new List<IssuedGoods>();
        private double serialOverflowQuantity = 0;
        private bool isProccessOrderless = false;
        private ListView listData;
        private UniversalAdapter<LocationClass> dataAdapter;
        private ZoomageView? imagePNG;
        private List<LocationClass> items = new List<LocationClass>();
        private ZoomageView? image;
        private double packaging;
        private double quantity;
        private Spinner cbMultipleLocations;
        private List<MultipleStock> adapterLocations = new List<MultipleStock>();
        private ArrayAdapter<MultipleStock> adapterLocation;

        public static List<IssuedGoods> FilterIssuedGoods(List<IssuedGoods> issuedGoodsList, string acSSCC = null, string acSerialNo = null, string acLocation = null)
        {
            try
            {
                var filtered = issuedGoodsList;

                if (!String.IsNullOrEmpty(acSSCC))
                {
                    filtered = filtered.Where(x => x.acSSCC == acSSCC).ToList();
                }

                if (!String.IsNullOrEmpty(acSerialNo))
                {
                    filtered = filtered.Where(x => x.acSerialNo == acSerialNo).ToList();
                }

                if (!String.IsNullOrEmpty(acLocation))
                {
                    filtered = filtered.Where(x => x.aclocation == acLocation).ToList();
                }

                return filtered;
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
                return new List<IssuedGoods>();
            }
        }

        public async void GetBarcode(string barcode)
        {
            try
            {
                try
                {
                    if (tbSSCC.HasFocus)
                    {
                        if (barcode != "Scan fail")
                        {


                            tbSSCC.Text = barcode;

                            if (serialRow.Visibility == ViewStates.Visible)
                            {
                                tbSerialNum.RequestFocus();
                            }
                            else
                            {
                                tbPacking.RequestFocus();
                            }

                            await FilterData();
                        }
                    }
                    else if (tbSerialNum.HasFocus)
                    {
                        if (barcode != "Scan fail")
                        {


                            tbSerialNum.Text = barcode;

                            tbPacking.RequestFocus();

                            await FilterData();
                        }
                    }
                    else if (searchableSpinnerIssueLocation.spinnerTextValueField.HasFocus)
                    {
                        if (barcode != "Scan fail")
                        {


                            searchableSpinnerIssueLocation.spinnerTextValueField.Text = barcode;

                            await FilterData();

                            if (isProccessOrderless)
                            {
                                GetQuantityOrderLess();
                            }

                        }
                    }
                }
                catch (Exception ex)
                {
                    SentrySdk.CaptureException(ex);
                    Toast.MakeText(this, $"{Resources.GetString(Resource.String.s225)}", ToastLength.Long).Show();
                }
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private async void GetQuantityOrderLess()
        {
            try
            {
                if (openIdent != null && moveHead != null)
                {
                    string location = searchableSpinnerIssueLocation.spinnerTextValueField.Text;
                    string ident = openIdent.GetString("Code");
                    string warehouse = moveHead.GetString("Wharehouse");
                    string sscc = string.IsNullOrEmpty(tbSSCC.Text) ? null : tbSSCC.Text;
                    string serial = string.IsNullOrEmpty(tbSerialNum.Text) ? null : tbSerialNum.Text;
                    await LoadStock(location, ident, warehouse, sscc, serial);
                }
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }


        private async Task LoadStock(string location, string ident, string warehouse, string sscc = null, string serial = null)
        {
            try
            {
                var parameters = new List<Services.Parameter>();

                parameters.Add(new Services.Parameter { Name = "acIdent", Type = "String", Value = ident });
                parameters.Add(new Services.Parameter { Name = "aclocation", Type = "String", Value = location });
                parameters.Add(new Services.Parameter { Name = "acWarehouse", Type = "String", Value = warehouse });

                string sql = "SELECT TOP 1 anQty FROM uWMSStockByWarehouse WHERE acIdent = @acIdent AND aclocation = @aclocation AND acWarehouse = @acWarehouse";

                if (sscc != null)
                {
                    sql += " AND acSSCC = @acSSCC";
                    parameters.Add(new Services.Parameter { Name = "acSSCC", Type = "String", Value = sscc });
                }

                if (serial != null)
                {
                    sql += " AND acSerialNo = @acSerialNo;";
                    parameters.Add(new Services.Parameter { Name = "acSerialNo", Type = "String", Value = serial });
                }

                var qty = await AsyncServices.AsyncServices.GetObjectListBySqlAsync(sql, parameters, this);

                if (qty.Success)
                {

                    if (qty.Rows.Count > 0)
                    {
                        double result = (double?)qty.Rows[0].DoubleValue("anQty") ?? 0;
                        qtyCheck = result;
                        lbQty.Text = $"{Resources.GetString(Resource.String.s83)} ( " + qtyCheck.ToString(await CommonData.GetQtyPictureAsync(this)) + " )";
                        tbPacking.Text = qtyCheck.ToString();
                        stock = qtyCheck;
                    }
                    else
                    {
                        double result = 0;
                        qtyCheck = result;
                        lbQty.Text = $"{Resources.GetString(Resource.String.s83)} ( " + qtyCheck.ToString(await CommonData.GetQtyPictureAsync(this)) + " )";
                        tbPacking.Text = qtyCheck.ToString();
                        stock = qtyCheck;
                    }

                    tbPacking.RequestFocus();
                    tbPacking.SelectAll();
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

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                SetTheme(Resource.Style.AppTheme_NoActionBar);
                // Start the loader
                if (App.Settings.tablet)
                {
                    base.RequestedOrientation = ScreenOrientation.Landscape;
                    base.SetContentView(Resource.Layout.IssuedGoodsSerialOrSSCCEntryTablet);
                    listData = FindViewById<ListView>(Resource.Id.listData);
                    dataAdapter = UniversalAdapterHelper.GetIssuedGoodsSerialOrSSCCEntry(this, items);
                    listData.Adapter = dataAdapter;
                    imagePNG = FindViewById<ZoomageView>(Resource.Id.imagePNG);
                    imagePNG.Visibility = ViewStates.Invisible;
                }
                else
                {
                    base.RequestedOrientation = ScreenOrientation.Portrait;
                    base.SetContentView(Resource.Layout.IssuedGoodsSerialOrSSCCEntry);
                }



                // Definitions
                AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
                var _customToolbar = new CustomToolbar(this, toolbar, Resource.Id.navIcon);
                _customToolbar.SetNavigationIcon(App.Settings.RootURL + "/Services/Logo");
                SetSupportActionBar(_customToolbar._toolbar);
                SupportActionBar.SetDisplayShowTitleEnabled(false);
                tbIdent = FindViewById<EditText>(Resource.Id.tbIdent);
                Window.SetSoftInputMode(Android.Views.SoftInput.AdjustResize);
                tbIdent.Enabled = false;
                tbSSCC = FindViewById<EditText>(Resource.Id.tbSSCC);
                tbSerialNum = FindViewById<EditText>(Resource.Id.tbSerialNum);
                tbPacking = FindViewById<EditText>(Resource.Id.tbPacking);
                cbMultipleLocations = FindViewById<Spinner>(Resource.Id.cbMultipleLocations);
                tbIdent.InputType = Android.Text.InputTypes.ClassNumber;
                tbSSCC.InputType = Android.Text.InputTypes.ClassNumber;
                lbQty = FindViewById<TextView>(Resource.Id.lbQty);
                barcode2D = new Barcode2D(this, this);
                btCreateSame = FindViewById<Button>(Resource.Id.btCreateSame);
                btCreate = FindViewById<Button>(Resource.Id.btCreate);
                btFinish = FindViewById<Button>(Resource.Id.btFinish);
                btOverview = FindViewById<Button>(Resource.Id.btOverview);
                btExit = FindViewById<Button>(Resource.Id.btExit);

                searchableSpinnerIssueLocation = FindViewById<SearchableSpinner>(Resource.Id.searchableSpinnerIssueLocation);
                var locations = await HelperMethods.GetLocationsForGivenWarehouse(moveHead.GetString("Wharehouse"));
                searchableSpinnerIssueLocation.SetItems(locations);
                searchableSpinnerIssueLocation.ColorTheRepresentation(1);



                if (await CommonData.GetSettingAsync("IssueSummaryView", this) == "1")
                {
                    // If the company opted for this.
                    cbMultipleLocations.ItemSelected += CbMultipleLocations_ItemSelected;
                    searchableSpinnerIssueLocation.icon.Visibility = ViewStates.Gone;                    
                } else
                {
                    searchableSpinnerIssueLocation.ShowDropDown();
                    cbMultipleLocations.Visibility = ViewStates.Gone;
                }

                searchableSpinnerIssueLocation.spinnerTextValueField.KeyPress += TbLocation_KeyPress;
                tbSSCC.KeyPress += TbSSCC_KeyPress;
                tbSerialNum.KeyPress += TbSerialNum_KeyPress;
                btCreateSame.Click += BtCreateSame_Click;
                btCreate.Click += BtCreate_Click;
                btFinish.Click += BtFinish_Click;
                btExit.Click += BtExit_Click;
                btOverview.Click += BtOverview_Click;
                var _broadcastReceiver = new NetworkStatusBroadcastReceiver();
                _broadcastReceiver.ConnectionStatusChanged += OnNetworkStatusChanged;
                Application.Context.RegisterReceiver(_broadcastReceiver,
                new IntentFilter(ConnectivityManager.ConnectivityAction), ReceiverFlags.NotExported);
                ssccRow = FindViewById<LinearLayout>(Resource.Id.sscc_row);
                serialRow = FindViewById<LinearLayout>(Resource.Id.serial_row);

                // Method calls

                CheckIfApplicationStopingException();

                // Color the fields that can be scanned
                ColorFields();



                SetUpProcessDependentButtons();
                // Main logic for the entry
                await SetUpForm();

                if (App.Settings.tablet)
                {
                    await fillItems();
                }


            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }
        private bool initialDropdownEvent = true;

        private async void CbMultipleLocations_ItemSelected(object? sender, AdapterView.ItemSelectedEventArgs e)
        {
            try
            {

                var selected = adapterLocation.GetItem(e.Position);

                if (selected != null)
                {

                    searchableSpinnerIssueLocation.spinnerTextValueField.Text = selected.Location;

                    if (selected.Quantity > stock)
                    {
                        tbPacking.Text = stock.ToString();
                    }
                    else
                    {
                        tbPacking.Text = selected.Quantity.ToString();
                    }

                    /* This is maybe a good idea.
                    if(!selected.excludeSSCCSerial)
                    {
                        tbSerialNum.Text = selected.Serial;
                        tbSSCC.Text = selected.SSCC;
                    }
                    */

                    tbPacking.SelectAll();
                    await FilterData();
                }
                initialDropdownEvent = false;
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }


        private void showPictureIdent(string ident)
        {
            try
            {
                try
                {
                    Android.Graphics.Bitmap show = Services.GetImageFromServerIdent(moveHead.GetString("Wharehouse"), ident);
                    var debug = moveHead.GetString("Wharehouse");
                    Drawable d = new BitmapDrawable(Resources, show);
                    imagePNG.SetImageDrawable(d);
                    imagePNG.Visibility = ViewStates.Visible;
                    imagePNG.Click += (e, ev) => { ImageClick(d); };

                }
                catch (Exception)
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }




        private void ImageClick(Drawable d)
        {
            try
            {
                popupDialog = new Dialog(this);
                popupDialog.SetContentView(Resource.Layout.WarehousePicture);
                popupDialog.Window.SetSoftInputMode(SoftInput.AdjustResize);
                popupDialog.Show();

                popupDialog.Window.SetLayout(LayoutParams.MatchParent, LayoutParams.WrapContent);
                popupDialog.Window.SetBackgroundDrawableResource(Android.Resource.Color.HoloBlueBright);
                image = popupDialog.FindViewById<ZoomageView>(Resource.Id.image);
                image.SetMinimumHeight(500);
                image.SetMinimumWidth(800);
                image.SetImageDrawable(d);
                // Access Popup layout fields like below
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }


        private async Task fillItems()
        {
            try
            {
                var code = openIdent.GetString("Code");
                var wh = moveHead.GetString("Wharehouse");
                items = await AdapterStore.getStockForWarehouseAndIdent(code, wh);
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }


        private void SetUpProcessDependentButtons()
        {
            try
            {
                // This method changes the UI so it shows in a visible way that it is the update screen. - 18.03.2024
                if (Base.Store.isUpdate)
                {
                    btCreateSame.Visibility = ViewStates.Gone;
                    btCreate.Text = $"{Resources.GetString(Resource.String.s290)}";
                }
                else if (Base.Store.code2D != null)
                {
                    btCreateSame.Visibility = ViewStates.Gone;
                    // 2d code reading process.
                }
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private async void BtCreate_Click(object? sender, EventArgs e)
        {
            try
            {
                try
                {
                    LoaderManifest.LoaderManifestLoopResources(this);
                    if (!isProccessOrderless)
                    {
                        CheckData();
                    }

                    if (!Base.Store.isUpdate)
                    {
                        double parsed;

                        if (isProccessOrderless && double.TryParse(tbPacking.Text, out parsed) && stock >= parsed)
                        {
                            var isCorrectLocation = await IsLocationCorrect();

                            if (!isCorrectLocation)
                            {
                                // Nepravilna lokacija za izbrano skladišče
                                Toast.MakeText(this, $"{Resources.GetString(Resource.String.s333)}", ToastLength.Long).Show();
                                return;
                            }

                            await CreateMethodFromStart();
                        }
                        else if (createPositionAllowed && double.TryParse(tbPacking.Text, out parsed) && stock == 0)
                        {

                            if (Base.Store.modeIssuing == 2)
                            {
                                StartActivity(typeof(IssuedGoodsIdentEntryWithTrail));
                                Finish();
                            }
                            else if (Base.Store.modeIssuing == 1)
                            {
                                StartActivity(typeof(IssuedGoodsIdentEntry));
                                Finish();
                            }

                        }
                        else if (createPositionAllowed && double.TryParse(tbPacking.Text, out parsed) && stock >= parsed)
                        {
                            await CreateMethodFromStart();
                        }

                        else
                        {
                            Toast.MakeText(this, $"{Resources.GetString(Resource.String.s270)}", ToastLength.Long).Show();
                        }
                    }
                    else
                    {
                        // Update flow.

                        double newQty;
                        if (Double.TryParse(tbPacking.Text, out newQty))
                        {
                            if (newQty > moveItem.GetDouble("Qty"))
                            {
                                Toast.MakeText(this, $"{Resources.GetString(Resource.String.s291)}", ToastLength.Long).Show();
                            }
                            else
                            {
                                var parameters = new List<Services.Parameter>();
                                var tt = moveItem.GetInt("ItemID");
                                parameters.Add(new Services.Parameter { Name = "anQty", Type = "Decimal", Value = newQty });
                                parameters.Add(new Services.Parameter { Name = "anItemID", Type = "Int32", Value = moveItem.GetInt("ItemID") });
                                string debugString = $"UPDATE uWMSMoveItem SET anQty = {newQty} WHERE anIDItem = {moveItem.GetInt("ItemID")}";
                                var subjects = Services.Update($"UPDATE uWMSMoveItem SET anQty = @anQty WHERE anIDItem = @anItemID;", parameters);
                                if (!subjects.Success)
                                {
                                    RunOnUiThread(() =>
                                    {
                                        SentrySdk.CaptureMessage(subjects.Error);
                                        return;
                                    });
                                }
                                else
                                {
                                    StartActivity(typeof(IssuedGoodsEnteredPositionsView));
                                    Finish();
                                }
                            }
                        }
                        else
                        {
                            Toast.MakeText(this, $"{Resources.GetString(Resource.String.s270)}", ToastLength.Long).Show();
                        }
                    }
                }
                catch (Exception ex)
                {
                    SentrySdk.CaptureException(ex);
                }
                finally
                {
                    LoaderManifest.LoaderManifestLoopStop(this);
                }
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }
        private void CheckData()
        {
            try
            {
                data = FilterIssuedGoods(connectedPositions, tbSSCC.Text, tbSerialNum.Text, searchableSpinnerIssueLocation.spinnerTextValueField.Text);
                if (data.Count == 1)
                {
                    createPositionAllowed = true;
                }
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }
        private async void BtCreateSame_Click(object? sender, EventArgs e)
        {
            try
            {
                try
                {
                    LoaderManifest.LoaderManifestLoopResources(this);
                    if (!isProccessOrderless)
                    {
                        CheckData();
                    }

                    double parsed;
                    if (isProccessOrderless && double.TryParse(tbPacking.Text, out parsed) && stock >= parsed)
                    {
                        var isCorrectLocation = await IsLocationCorrect();

                        if (!isCorrectLocation)
                        {
                            // Nepravilna lokacija za izbrano skladišče
                            Toast.MakeText(this, $"{Resources.GetString(Resource.String.s333)}", ToastLength.Long).Show();
                            return;
                        }

                        await CreateMethodSame();
                    }
                    else if (createPositionAllowed && double.TryParse(tbPacking.Text, out parsed) && stock == 0)
                    {
                        if (Base.Store.modeIssuing == 2)
                        {
                            StartActivity(typeof(IssuedGoodsIdentEntryWithTrail));
                            Finish();
                        }
                        else if (Base.Store.modeIssuing == 1)
                        {
                            StartActivity(typeof(IssuedGoodsIdentEntry));
                            Finish();
                        }
                    }
                    else if (createPositionAllowed && double.TryParse(tbPacking.Text, out parsed) && stock >= parsed)
                    {
                        var isCorrectLocation = await IsLocationCorrect();

                        if (!isCorrectLocation)
                        {
                            // Nepravilna lokacija za izbrano skladišče
                            Toast.MakeText(this, $"{Resources.GetString(Resource.String.s333)}", ToastLength.Long).Show();
                            return;
                        }

                        await CreateMethodSame();
                    }
                    else
                    {
                        Toast.MakeText(this, $"{Resources.GetString(Resource.String.s270)}", ToastLength.Long).Show();
                    }
                }
                catch (Exception ex)
                {
                    SentrySdk.CaptureException(ex);
                }
                finally
                {
                    LoaderManifest.LoaderManifestLoopStop(this);
                }
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }
        private async Task<bool> IsLocationCorrect()
        {
            try
            {
                string location = searchableSpinnerIssueLocation.spinnerTextValueField.Text;

                if (!await CommonData.IsValidLocationAsync(moveHead.GetString("Wharehouse"), location, this))
                {
                    return false;
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


        private void BtExit_Click(object? sender, EventArgs e)
        {
            try
            {
                StartActivity(typeof(MainMenu));
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
                    case Keycode.F2:
                        BtCreateSame_Click(this, null);
                        break;
                    case Keycode.F3:
                        BtCreate_Click(this, null);
                        break;
                    case Keycode.F4:
                        BtFinish_Click(this, null);
                        break;
                    case Keycode.F5:
                        BtOverview_Click(this, null);
                        break;
                    case Keycode.F8:
                        BtExit_Click(this, null);
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


        private void BtFinish_Click(object? sender, EventArgs e)
        {
            try
            {
                popupDialogConfirm = new Dialog(this);
                popupDialogConfirm.SetContentView(Resource.Layout.Confirmation);
                popupDialogConfirm.Window.SetSoftInputMode(SoftInput.AdjustResize);
                popupDialogConfirm.Show();
                popupDialogConfirm.Window.SetLayout(LayoutParams.MatchParent, LayoutParams.WrapContent);
                popupDialogConfirm.Window.SetBackgroundDrawable(new ColorDrawable(Color.ParseColor("#081a45")));
                btnYesConfirm = popupDialogConfirm.FindViewById<Button>(Resource.Id.btnYes);
                btnNoConfirm = popupDialogConfirm.FindViewById<Button>(Resource.Id.btnNo);
                btnYesConfirm.Click += BtnYesConfirm_Click;
                btnNoConfirm.Click += BtnNoConfirm_Click;
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private void BtnNoConfirm_Click(object sender, EventArgs e)
        {
            try
            {
                popupDialogConfirm.Dismiss();
                popupDialogConfirm.Hide();
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private async void BtnYesConfirm_Click(object sender, EventArgs e)
        {
            try
            {
                try
                {
                    LoaderManifest.LoaderManifestLoopResources(this);
                    await FinishMethod();
                }
                catch (Exception ex)
                {
                    SentrySdk.CaptureException(ex);
                }
                finally
                {
                    LoaderManifest.LoaderManifestLoopStop(this);
                }
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private void BtOverview_Click(object? sender, EventArgs e)
        {
            try
            {
                StartActivity(typeof(IssuedGoodsEnteredPositionsView));
                Finish();
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private void CheckIfApplicationStopingException()
        {
            try
            {
                if (moveHead != null && openIdent != null)
                {
                    // No error here, safe (ish) to continue
                    return;
                }
                else
                {
                    // Destroy the activity
                    Finish();
                    StartActivity(typeof(MainMenu));
                }
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private void ColorFields()
        {
            try
            {
                tbSSCC.SetBackgroundColor(Android.Graphics.Color.Aqua);
                tbSerialNum.SetBackgroundColor(Android.Graphics.Color.Aqua);
                searchableSpinnerIssueLocation.spinnerTextValueField.SetBackgroundColor(Android.Graphics.Color.Aqua);
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private async Task CreateMethodFromStart()
        {
            try
            {
                await Task.Run(() =>
                {
                    if (data.Count == 1 || isProccessOrderless)
                    {
                        var element = new IssuedGoods { };

                        if (!isProccessOrderless)
                        {
                            element = data.ElementAt(0);
                        }

                        moveItem = new NameValueObject("MoveItem");
                        moveItem.SetInt("HeadID", moveHead.GetInt("HeadID"));

                        if (!isProccessOrderless)
                        {
                            moveItem.SetString("LinkKey", element.acKey);
                            moveItem.SetInt("LinkNo", element.anNo);
                        }
                        else
                        {
                            moveItem.SetString("LinkKey", string.Empty);
                            moveItem.SetInt("LinkNo", 0);
                        }

                        moveItem.SetString("Ident", openIdent.GetString("Code"));
                        moveItem.SetString("SSCC", tbSSCC.Text.Trim());
                        moveItem.SetString("SerialNo", tbSerialNum.Text.Trim());
                        moveItem.SetDouble("Packing", Convert.ToDouble(tbPacking.Text.Trim()));
                        moveItem.SetDouble("Factor", 1);
                        moveItem.SetDouble("Qty", Convert.ToDouble(tbPacking.Text.Trim()));
                        moveItem.SetInt("Clerk", Services.UserID());
                        moveItem.SetString("Location", searchableSpinnerIssueLocation.spinnerTextValueField.Text.Trim());
                        moveItem.SetString("Palette", "1");

                        string error;

                        moveItem = Services.SetObject("mi", moveItem, out error);

                        if (moveItem != null && error == string.Empty)
                        {
                            RunOnUiThread(() =>
                            {
                                if (isProccessOrderless)
                                {
                                    StartActivity(typeof(IssuedGoodsIdentEntry));
                                    Finish();
                                }
                                else
                                {
                                    if (Base.Store.modeIssuing == 2)
                                    {
                                        StartActivity(typeof(IssuedGoodsIdentEntryWithTrail));
                                        Finish();
                                    }
                                    else if (Base.Store.modeIssuing == 1)
                                    {
                                        StartActivity(typeof(IssuedGoodsIdentEntry));
                                        Finish();
                                    }
                                }
                            });

                            createPositionAllowed = false;
                        }
                    }
                    else
                    {
                        return;
                    }
                });
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private async Task CreateMethodSame()
        {
            try
            {
                if (data.Count == 1 || isProccessOrderless)
                {
                    var element = new IssuedGoods();

                    if (!isProccessOrderless)
                    {
                        element = data.ElementAt(0);
                    }
                    // This solves the problem of updating the item. The problem occurs because of the old way of passing data.
                    moveItem = new NameValueObject("MoveItem");
                    moveItem.SetInt("HeadID", moveHead.GetInt("HeadID"));
                    if (!isProccessOrderless)
                    {
                        moveItem.SetString("LinkKey", element.acKey);
                        moveItem.SetInt("LinkNo", element.anNo);
                    }
                    else
                    {
                        moveItem.SetString("LinkKey", string.Empty);
                        moveItem.SetInt("LinkNo", 0);
                    }

                    moveItem.SetString("Ident", openIdent.GetString("Code"));
                    moveItem.SetString("SSCC", tbSSCC.Text.Trim());
                    moveItem.SetString("SerialNo", tbSerialNum.Text.Trim());
                    moveItem.SetDouble("Packing", Convert.ToDouble(tbPacking.Text.Trim()));
                    moveItem.SetDouble("Factor", 1);
                    moveItem.SetDouble("Qty", Convert.ToDouble(tbPacking.Text.Trim()));
                    moveItem.SetInt("Clerk", Services.UserID());
                    moveItem.SetString("Location", searchableSpinnerIssueLocation.spinnerTextValueField.Text.Trim());
                    moveItem.SetString("Palette", "1");
                    string error;
                    moveItem = Services.SetObject("mi", moveItem, out error);

                    if (moveItem != null && error == string.Empty)
                    {

                        serialOverflowQuantity = Convert.ToDouble(tbPacking.Text.Trim());
                        stock -= serialOverflowQuantity;


                        lbQty.Text = $"{Resources.GetString(Resource.String.s83)} ( " + stock.ToString(await CommonData.GetQtyPictureAsync(this)) + " )";


                        // Check to see if the maximum is already reached.
                        if (stock <= 0)
                        {
                            if (!isProccessOrderless)
                            {
                                if (Base.Store.modeIssuing == 2)
                                {
                                    StartActivity(typeof(IssuedGoodsIdentEntryWithTrail));
                                    Finish();
                                }
                                else if (Base.Store.modeIssuing == 1)
                                {
                                    StartActivity(typeof(IssuedGoodsIdentEntry));
                                    Finish();
                                }
                            }
                            else
                            {
                                StartActivity(typeof(IssuedGoodsIdentEntry));
                                Finish();
                            }
                        }


                        // Succesfull position creation
                        if (ssccRow.Visibility == ViewStates.Visible)
                        {
                            tbSSCC.Text = string.Empty;
                            tbSSCC.RequestFocus();
                        }

                        if (serialRow.Visibility == ViewStates.Visible)
                        {
                            tbSerialNum.Text = string.Empty;

                            if (ssccRow.Visibility == ViewStates.Gone)
                            {
                                tbSerialNum.RequestFocus();
                            }
                        }

                        tbPacking.Text = string.Empty;

                        if (!isProccessOrderless)
                        {
                            createPositionAllowed = false;
                            await GetConnectedPositions(element.acKey, element.anNo, element.acIdent);
                        }
                    }
                }
                else
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private async Task FilterData()
        {
            try
            {
                data = FilterIssuedGoods(connectedPositions, tbSSCC.Text, tbSerialNum.Text, searchableSpinnerIssueLocation.spinnerTextValueField.Text);
                if (data.Count == 1)
                {
                    var element = data.ElementAt(0);

                    // This is perhaps not needed due to the quantity checking requirments. lbQty.Text = $"{Resources.GetString(Resource.String.s83)} ( " + element.anQty.ToString() + " )";
                    if (element.anPackQty != -1 && element.anPackQty <= element.anQty && await CommonData.GetSettingAsync("UsePackagingQuantity") == "1")
                    {
                        tbPacking.Text = element.anPackQty.ToString();
                        lbQty.Text = $"{Resources.GetString(Resource.String.s83)} ( " + quantity.ToString(await CommonData.GetQtyPictureAsync(this)) + " )";
                    }
                    else
                    {
                        tbPacking.Text = element.anQty.ToString();
                        lbQty.Text = $"{Resources.GetString(Resource.String.s83)} ( " + quantity.ToString(await CommonData.GetQtyPictureAsync(this)) + " )";
                    }
                    if (serialRow.Visibility == ViewStates.Visible)
                    {
                        tbSerialNum.Text = element.acSerialNo ?? string.Empty;
                    }
                    searchableSpinnerIssueLocation.spinnerTextValueField.Text = element.aclocation;
                    // Do stuff and allow creating the position
                    createPositionAllowed = true;


                    tbPacking.PostDelayed(() =>
                    {
                        tbPacking.RequestFocus();
                        tbPacking.SetSelection(0, tbPacking.Text.Length);
                    }, 100); // Delay in milliseconds

                    // This flow should end up with correct data in the fields and the select focus on the qty field. 
                }
                else
                {
                    lbQty.Text = $"{Resources.GetString(Resource.String.s292)}";
                }
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private async Task FinishMethod()
        {
            try
            {
                await Task.Run(async () =>
                {


                    try
                    {
                        var headID = moveHead.GetInt("HeadID");

                        var (success, result) = await WebApp.GetAsync("mode=finish&stock=remove&print=" + Services.DeviceUser() + "&id=" + headID.ToString(), this);

                        if (success)
                        {
                            if (result.StartsWith("OK!"))
                            {
                                RunOnUiThread(() =>
                                {

                                    var id = result.Split('+')[1];

                                    Toast.MakeText(this, $"{Resources.GetString(Resource.String.s264)}" + id, ToastLength.Long).Show();

                                    AlertDialog.Builder alert = new AlertDialog.Builder(this);

                                    alert.SetTitle($"{Resources.GetString(Resource.String.s263)}");

                                    alert.SetMessage($"{Resources.GetString(Resource.String.s264)}" + id);

                                    alert.SetPositiveButton("Ok", (senderAlert, args) =>
                                    {
                                        alert.Dispose();
                                        StartActivity(typeof(IssuedGoodsBusinessEventSetup));
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
                                    alert.SetMessage($"{Resources.GetString(Resource.String.s266)}" + result);

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
                            Toast.MakeText(this, $"{Resources.GetString(Resource.String.s216)}" + result, ToastLength.Long).Show();
                        }
                    }
                    catch (Exception ex)
                    {
                        SentrySdk.CaptureException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        /// <summary>
        /// Podatke preneseš v masko - kličeš NE isti view ampak vedno "uWMSOrderItemByKeyOut", ker moraš
        /// tudi pri subjektih zapisati na katero naročilo z pozicijo(acKey in anNo) se vrši izdaja.
        /// uWMSOrderItemByKeyOut; vhodni parameter acKey varchar(13), anNo int, acIdent varchar(16), acLocation varchar(50);
        /// izhod: acName varchar(80), acSubject varchar(30), acSerialNo varchar(100), acSSCC varchar(18), anQty decimal (19,6)
        /// če je zapis 1 potem prikažeš tiste podatke in uporabnik le potrdi
        /// če je zapisov več si jih shraniš in z dodatnimi vpisi/skeniranji(SSCC ali serijska) "filtriraš" podatke, ko prideš na enega izpolniš vse podatke, uporabnik lahko spremeni količino - v oklepaju je že od vsega začetka vpisan anQty.
        /// če uporabnik klikne na gumb serijska, se iz seznama pobriše ta vrsitca in maska ostane kot je bila po koncu koraka 4.
        /// lahko pa enostavno ponoviš klic view-a ki bi že moral imeti zapisane podatke in osvežene, če ne bo kaj težav z asinhronimi klici...
        ///
        /// </summary>
        /// <param name="acKey">Številka naročila</param>
        /// <param name="anNo">Pozicija znotraj naročila</param>
        /// <param name="acIdent">Ident</param>
        private async Task GetConnectedPositions(string acKey, int anNo, string acIdent)
        {
            try
            {
                connectedPositions.Clear();
                var sql = "SELECT acName, acSubject, acSerialNo, acSSCC, anQty, aclocation, anNo, acKey, acIdent, anPackQty from uWMSOrderItemByKeyOut WHERE acKey = @acKey AND anNo = @anNo AND acIdent = @acIdent";
                var parameters = new List<Services.Parameter>();
                parameters.Add(new Services.Parameter { Name = "acKey", Type = "String", Value = acKey });
                parameters.Add(new Services.Parameter { Name = "anNo", Type = "Int32", Value = anNo });
                parameters.Add(new Services.Parameter { Name = "acIdent", Type = "String", Value = acIdent });

                var subjects = await AsyncServices.AsyncServices.GetObjectListBySqlAsync(sql, parameters, this);
                if (!subjects.Success)
                {
                    RunOnUiThread(() =>
                    {
                        AlertDialog.Builder alert = new AlertDialog.Builder(this);
                        alert.SetTitle($"{Resources.GetString(Resource.String.s265)}");
                        alert.SetMessage($"{subjects.Error}");
                        alert.SetPositiveButton("Ok", (senderAlert, args) =>
                        {
                            alert.Dispose();
                        });
                        Dialog dialog = alert.Create();
                        dialog.Show();

                        SentrySdk.CaptureMessage(subjects.Error);
                        return;
                    });
                }
                else
                {
                    if (subjects.Rows.Count > 0)
                    {
                        for (int i = 0; i < subjects.Rows.Count; i++)
                        {
                            var row = subjects.Rows[i];
                            connectedPositions.Add(new IssuedGoods
                            {
                                acName = row.StringValue("acName"),
                                acSubject = row.StringValue("acSubject"),
                                acSerialNo = row.StringValue("acSerialNo"),
                                acSSCC = row.StringValue("acSSCC"),
                                anQty = row.DoubleValue("anQty"),
                                aclocation = row.StringValue("aclocation"),
                                anNo = (int)(row.IntValue("anNo") ?? -1),
                                acKey = row.StringValue("acKey"),
                                acIdent = row.StringValue("acIdent"),
                                anPackQty = row.DoubleValue("anPackQty") ?? -1
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
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

        private async Task SetUpForm()
        {
            try
            {
                if (App.Settings.tablet)
                {
                    showPictureIdent(openIdent.GetString("Code"));
                }
                // This is the default focus of the view.
                tbSSCC.RequestFocus();

                if (!openIdent.GetBool("isSSCC"))
                {
                    ssccRow.Visibility = ViewStates.Gone;
                    tbSerialNum.RequestFocus();
                }

                if (!openIdent.GetBool("HasSerialNumber"))
                {
                    serialRow.Visibility = ViewStates.Gone;
                    tbPacking.RequestFocus();
                }

                if (Base.Store.isUpdate)
                {
                    // Update logic ?? it seems to be true.
                    tbIdent.Text = moveItem.GetString("IdentName");
                    tbSerialNum.Text = moveItem.GetString("SerialNo");
                    tbSSCC.Text = moveItem.GetString("SSCC");
                    searchableSpinnerIssueLocation.spinnerTextValueField.Text = moveItem.GetString("Location");
                    tbPacking.Text = moveItem.GetDouble("Qty").ToString();
                    lbQty.Text = $"{Resources.GetString(Resource.String.s83)} ( " + moveItem.GetDouble("Qty").ToString() + " )";
                    btCreateSame.Text = $"{Resources.GetString(Resource.String.s293)}";
                    // Lock down all other fields
                    tbIdent.Enabled = false;
                    tbSerialNum.Enabled = false;
                    tbSSCC.Enabled = false;
                    searchableSpinnerIssueLocation.spinnerTextValueField.Enabled = false;

                    tbPacking.RequestFocus();
                    tbPacking.SelectAll();
                }
                else
                {

                    isProccessOrderless =
                     (Base.Store.OpenOrder == null && Intent.Extras == null) &&
                     (Intent.Extras == null || String.IsNullOrEmpty(Intent.Extras.GetString("selected")));

                    if (isProccessOrderless)
                    {
                        tbIdent.Text = openIdent.GetString("Code") + " " + openIdent.GetString("Name");
                        qtyCheck = 10000000;
                        lbQty.Text = $"{Resources.GetString(Resource.String.s83)} ( " + Resources.GetString(Resource.String.s336) + " )";
                        stock = qtyCheck;
                        searchableSpinnerIssueLocation.spinnerTextValueField.RequestFocus();
                    }
                    else
                    {
                        // Not the update ?? it seems to be true
                        tbIdent.Text = openIdent.GetString("Code") + " " + openIdent.GetString("Name");
                        if (Intent.Extras != null && !String.IsNullOrEmpty(Intent.Extras.GetString("selected")) && Base.Store.modeIssuing == 2)
                        {
                            // This flow is for orders.
                            string trailBytes = Intent.Extras.GetString("selected");
                            receivedTrail = JsonConvert.DeserializeObject<Trail>(trailBytes);

                            // Add the new logic here // 
                            await GetConnectedPositions(receivedTrail.Key, receivedTrail.No, receivedTrail.Ident);


                            if (await CommonData.GetSettingAsync("IssueSummaryView", this) == "1")
                            {
                                cbMultipleLocations.Visibility = ViewStates.Visible;
                                adapterLocations = await GetStockState(receivedTrail.Ident);
                                adapterLocation = new ArrayAdapter<MultipleStock>(this,
                                Android.Resource.Layout.SimpleSpinnerItem, adapterLocations);
                                adapterLocation.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
                                cbMultipleLocations.Adapter = adapterLocation;
                            }


                            searchableSpinnerIssueLocation.spinnerTextValueField.Text = receivedTrail.Location;

                            if (receivedTrail.Packaging != -1 && Double.TryParse(receivedTrail.Qty, out double qty) && receivedTrail.Packaging <= qty && await CommonData.GetSettingAsync("UsePackagingQuantity") == "1")
                            {
                                quantity = Double.Parse(receivedTrail.Qty);
                                packaging = receivedTrail.Packaging;
                                lbQty.Text = $"{Resources.GetString(Resource.String.s83)} ( " + quantity.ToString(await CommonData.GetQtyPictureAsync(this)) + " )";
                                stock = quantity;
                                tbPacking.Text = packaging.ToString();
                            }
                            else
                            {
                                quantity = Double.Parse(receivedTrail.Qty);
                                lbQty.Text = $"{Resources.GetString(Resource.String.s83)} ( " + quantity.ToString(await CommonData.GetQtyPictureAsync(this)) + " )";
                                stock = quantity;
                                tbPacking.Text = quantity.ToString();
                            }

                            await FilterData();
                        }
                        else if (Base.Store.modeIssuing == 2 && Base.Store.code2D != null)
                        {
                            var code2d = Base.Store.code2D;
                            tbSerialNum.Text = code2d.charge;
                            qtyCheck = 0;
                            double result;
                            // Try to parse the string to a double
                            if (Double.TryParse(code2d.netoWeight, out result))
                            {
                                qtyCheck = result;
                                lbQty.Text = $"{Resources.GetString(Resource.String.s83)} ( " + qtyCheck.ToString(await CommonData.GetQtyPictureAsync(this)) + " )";
                                tbPacking.Text = qtyCheck.ToString();
                                stock = qtyCheck;

                            }

                            await GetConnectedPositions(code2d.__helper__convertedOrder, code2d.__helper__position, code2d.ident);

                            // Reset the 2d code to nothing
                            Base.Store.code2D = null;

                            tbPacking.RequestFocus();
                            tbPacking.SelectAll();

                            await FilterData();
                        }
                        else if (Base.Store.modeIssuing == 1)
                        {
                            // This flow is for idents.

                            var order = Base.Store.OpenOrder;

                            if (order != null)
                            {
                                packaging = order.Packaging ?? 0;
                                quantity = order.Quantity ?? 0;

                                if (order.Packaging != -1 && packaging <= quantity && await CommonData.GetSettingAsync("UsePackagingQuantity") == "1")
                                {
                                    lbQty.Text = $"{Resources.GetString(Resource.String.s83)} ( " + quantity.ToString(await CommonData.GetQtyPictureAsync(this)) + " )";
                                    stock = quantity;
                                }
                                else
                                {
                                    lbQty.Text = $"{Resources.GetString(Resource.String.s83)} ( " + quantity.ToString(await CommonData.GetQtyPictureAsync(this)) + " )";
                                    stock = quantity;
                                }

                                await GetConnectedPositions(order.Order, order.Position ?? -1, order.Ident);


                                if (await CommonData.GetSettingAsync("IssueSummaryView", this) == "1")
                                {
                                    cbMultipleLocations.Visibility = ViewStates.Visible;
                                    adapterLocations = await GetStockState(order.Ident);
                                    adapterLocation = new ArrayAdapter<MultipleStock>(this,
                                    Android.Resource.Layout.SimpleSpinnerItem, adapterLocations);
                                    adapterLocation.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
                                    cbMultipleLocations.Adapter = adapterLocation;

                                }

                                await FilterData();

                            }
                        }
                    }
                }
                isPackaging = openIdent.GetBool("IsPackaging");
                if (isPackaging)
                {
                    ssccRow.Visibility = ViewStates.Gone;
                    serialRow.Visibility = ViewStates.Gone;
                }
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private async Task<List<MultipleStock>> GetStockState(String ident)
        {
            try
            {
                List<MultipleStock> data = new List<MultipleStock>();

                var sql = "SELECT aclocation, anQty, acSerialNo, acSSCC FROM uWMSStockByWarehouse WHERE acIdent = @acIdent AND acWarehouse = @acWarehouse;";
                var parameters = new List<Services.Parameter>();

                parameters.Add(new Services.Parameter { Name = "acWarehouse", Type = "String", Value = moveHead.GetString("Wharehouse") });
                parameters.Add(new Services.Parameter { Name = "acIdent", Type = "String", Value = ident });

                var stocks = await AsyncServices.AsyncServices.GetObjectListBySqlAsync(sql, parameters, this);

                if (stocks.Success && stocks.Rows.Count > 0)
                {
                    foreach (var stockRow in stocks.Rows)
                    {
                        if (stockRow.DoubleValue("anQty") != null && stockRow.DoubleValue("anQty") > 0)
                        {
                            var item = new MultipleStock
                            {
                                Location = stockRow.StringValue("aclocation"),
                                Quantity = stockRow.DoubleValue("anQty") ?? 0,
                                Serial = stockRow.StringValue("acSerialNo"),
                                SSCC = stockRow.StringValue("acSSCC"),
                            };

                            Showing type = Showing.Ordinary;

                            if (ssccRow.Visibility == ViewStates.Visible)
                            {
                                type = Showing.SSCC;

                            }
                            else if (ssccRow.Visibility == ViewStates.Gone && serialRow.Visibility == ViewStates.Visible)
                            {
                                type = Showing.Serial;
                            }
                            else
                            {
                                type = Showing.Ordinary;
                            }

                            item.ConfigurationMethod(type, this);
                            data.Add(item);
                        }

                    }

                }

                return data;
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
                return new List<MultipleStock>();
            }
        }


        private async void TbSerialNum_KeyPress(object? sender, View.KeyEventArgs e)
        {
            try
            {
                if (e.KeyCode == Keycode.Enter && e.Event.Action == KeyEventActions.Down)
                {
                    await FilterData();
                }
                else
                {
                    e.Handled = false;
                }
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private async void TbSSCC_KeyPress(object? sender, View.KeyEventArgs e)
        {
            try
            {
                if (e.KeyCode == Keycode.Enter && e.Event.Action == KeyEventActions.Down)
                {
                    await FilterData();
                }
                else
                {
                    e.Handled = false;
                }
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }


        private async void TbLocation_KeyPress(object? sender, View.KeyEventArgs e)
        {
            try
            {
                if (e.KeyCode == Keycode.Enter && e.Event.Action == KeyEventActions.Down)
                {
                    if (isProccessOrderless)
                    {
                        var isCorrectLocation = await IsLocationCorrect();

                        if (!isCorrectLocation)
                        {
                            // Nepravilna lokacija za izbrano skladišče
                            Toast.MakeText(this, $"{Resources.GetString(Resource.String.s333)}", ToastLength.Long).Show();
                            return;
                        }
                        else
                        {
                            GetQuantityOrderLess();
                        }

                    }
                }
                else
                {
                    e.Handled = false;
                }

                await FilterData();
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

    }
}