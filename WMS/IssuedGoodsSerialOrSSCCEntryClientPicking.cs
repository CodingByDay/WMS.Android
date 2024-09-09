using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Media;
using Android.Net;
using Android.Views;
using BarCode2D_Receiver;
using System.Diagnostics;
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
    public class IssuedGoodsSerialOrSSCCEntryClientPicking : CustomBaseActivity, IBarcodeResult
    {

        private EditText tbIdent;
        private EditText tbSSCC;
        private EditText tbSerialNum;
        private EditText tbPacking;

        private NameValueObject openIdent = (NameValueObject)InUseObjects.Get("OpenIdent");
        private NameValueObject openOrder = (NameValueObject)InUseObjects.Get("OpenOrder");
        private ApiResultSet OpenOrderItem = (ApiResultSet)InUseObjects.Get("OpenOrderItem");
        private NameValueObject moveHead = (NameValueObject)InUseObjects.Get("MoveHead");
        private NameValueObject moveItem = (NameValueObject)InUseObjects.Get("MoveItem");
        private NameValueObject extraData = (NameValueObject)InUseObjects.Get("ExtraData");
        private NameValueObject lastItem = (NameValueObject)InUseObjects.Get("LastItem");

        private Button btCreateSame;
        private Button btCreate;
        private Button btFinish;
        private Button btOverview;
        private Button btExit;

        private static bool? checkIssuedOpenQty = null;
        private Dialog popupDialogMain;
        private Button btConfirm;
        private EditText tbSSCCpopup;
        private ListView lvCardMore;
        private MorePalletsAdapter adapter;
        private Dialog popupDialog;
        private Button btnYes;
        private Button btnNo;
        private bool isFirst;
        private bool isMorePalletsMode = false;
        private bool isBatch;
        private int check;
        private bool isOkayToCallBarcode;
        private MorePalletsAdapter adapterNew;
        private NameValueObject moveItemNew;
        private Dialog popupDialogConfirm;
        private Button btnYesConfirm;
        private Button btnNoConfirm;
        private CustomAutoCompleteAdapter<string> DataAdapter;
        private string qtyStock;
        private MorePallets existsDuplicate;
        private string error;
        private string query;
        private ApiResultSet result;
        private NameValueObject dataObject;
        private string ident;
        private string sscc;
        private string warehouse;
        private Dialog popupDialogMainIssueing;
        private List<IssuedGoods> data = new List<IssuedGoods>();
        private List<IssuedGoods> dist;
        private List<LocationClass> items = new List<LocationClass>();
        private TextView lbQty;
        private bool isPackaging = false;

        private SoundPool soundPool;
        private int soundPoolId;
        private Barcode2D barcode2D;
        private SearchableSpinner? searchableSpinnerIssueLocation;
        private bool isOpened = false;
        private ClientPickingPosition receivedTrail;
        private List<string> locations = new List<string>();
        private double qtyCheck = 0;
        private LinearLayout ssccRow;
        private LinearLayout serialRow;
        private List<IssuedGoods> connectedPositions = new List<IssuedGoods>();
        private bool createPositionAllowed = false;
        private double stock;
        private ListView listData;
        private UniversalAdapter<LocationClass> dataAdapter;
        private double serialOverflowQuantity = 0;
        private Spinner cbMultipleLocations;
        private List<MultipleStock> adapterLocations;
        private ArrayAdapter<MultipleStock> adapterLocation;

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
                    base.SetContentView(Resource.Layout.IssuedGoodsSerialOrSSCCEntryClientPickingTablet);
                    listData = FindViewById<ListView>(Resource.Id.listData);
                    dataAdapter = UniversalAdapterHelper.GetIssuedGoodsSerialOrSSCCEntryClientPicking(this, items);
                    listData.Adapter = dataAdapter;
                }
                else
                {
                    base.RequestedOrientation = ScreenOrientation.Portrait;
                    base.SetContentView(Resource.Layout.IssuedGoodsSerialOrSSCCEntryClientPicking);
                }

                LoaderManifest.LoaderManifestLoopResources(this);

                // Definitions
                AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
                var _customToolbar = new CustomToolbar(this, toolbar, Resource.Id.navIcon);
                _customToolbar.SetNavigationIcon(App.Settings.RootURL + "/Services/Logo");
                SetSupportActionBar(_customToolbar._toolbar);
                SupportActionBar.SetDisplayShowTitleEnabled(false);
                Window.SetSoftInputMode(Android.Views.SoftInput.AdjustResize);
                tbIdent = FindViewById<EditText>(Resource.Id.tbIdent);
                tbSSCC = FindViewById<EditText>(Resource.Id.tbSSCC);
                tbSerialNum = FindViewById<EditText>(Resource.Id.tbSerialNum);
                tbPacking = FindViewById<EditText>(Resource.Id.tbPacking);
                cbMultipleLocations = FindViewById<Spinner>(Resource.Id.cbMultipleLocations);

                searchableSpinnerIssueLocation = FindViewById<SearchableSpinner>(Resource.Id.searchableSpinnerIssueLocation);
                var locations = await HelperMethods.GetLocationsForGivenWarehouse(moveHead.GetString("Wharehouse"));
                searchableSpinnerIssueLocation.SetItems(locations);
                searchableSpinnerIssueLocation.ColorTheRepresentation(1);

                if (await CommonData.GetSettingAsync("IssueSummaryView", this) == "1")
                {
                    // If the company opted for this.
                    searchableSpinnerIssueLocation.icon.Visibility = ViewStates.Gone;
                    cbMultipleLocations.ItemSelected += CbMultipleLocations_ItemSelected;
                }
                else
                {
                    searchableSpinnerIssueLocation.ShowDropDown();
                    cbMultipleLocations.Visibility = ViewStates.Gone;
                }

                tbIdent.InputType = Android.Text.InputTypes.ClassNumber;
                tbSSCC.InputType = Android.Text.InputTypes.ClassNumber;
                lbQty = FindViewById<TextView>(Resource.Id.lbQty);

                barcode2D = new Barcode2D(this, this);

             
                btCreateSame = FindViewById<Button>(Resource.Id.btCreateSame);
                btCreate = FindViewById<Button>(Resource.Id.btCreate);
                btFinish = FindViewById<Button>(Resource.Id.btFinish);
                btOverview = FindViewById<Button>(Resource.Id.btOverview);
                btExit = FindViewById<Button>(Resource.Id.btExit);

                tbPacking.KeyPress += TbPacking_KeyPress;
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

                // Main logic for the entry
                await SetUpForm();

                SetUpProcessDependentButtons();


                if (ssccRow.Visibility != ViewStates.Visible && serialRow.Visibility != ViewStates.Visible)
                {
                    tbPacking.RequestFocus();
                    tbPacking.SelectAll();
                }


                LoaderManifest.LoaderManifestLoopStop(this);

            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private bool initialLocation = true;





        private async Task<bool> SaveMoveItem()
        {
            try
            {
                try
                {
                    double parsed;

                    CheckData();

                    QuantityProcessing result = QuantityProcessing.OtherError;

                    if (double.TryParse(tbPacking.Text, out parsed) && createPositionAllowed)
                    {
                        var element = data.ElementAt(0);
                        result = HelperMethods.IsOverTheLimitTransactionAllowed(element.anStock ?? 0, element.anMaxQty ?? 0, parsed);
                    }
                    else
                    {
                        result = QuantityProcessing.OtherError;
                    }

                    if (!Base.Store.isUpdate)
                    {
                        if (result != QuantityProcessing.GoodToGo)
                        {
                            if (result == QuantityProcessing.OverTheStock)
                            {
                                Toast.MakeText(this, $"{Resources.GetString(Resource.String.s353)}", ToastLength.Long).Show();
                                return false;
                            }
                            else if (result == QuantityProcessing.OverTheOrdered)
                            {
                                Toast.MakeText(this, $"{Resources.GetString(Resource.String.s354)}", ToastLength.Long).Show();
                                return false;
                            }
                            else if (result == QuantityProcessing.OtherError)
                            {
                                Toast.MakeText(this, $"{Resources.GetString(Resource.String.s270)}", ToastLength.Long).Show();
                                return false;
                            }
                            return false;
                        }
                        else
                        {
                            var isCorrectLocation = await IsLocationCorrect();

                            if (!isCorrectLocation)
                            {
                                // Nepravilna lokacija za izbrano skladišče
                                Toast.MakeText(this, $"{Resources.GetString(Resource.String.s333)}", ToastLength.Long).Show();
                                return false;
                            }

              
                            
                            if (dist.Count == 1)
                            {
                                var element = dist.ElementAt(0);
                                moveItem = new NameValueObject("MoveItem");
                                moveItem.SetInt("HeadID", moveHead.GetInt("HeadID"));
                                moveItem.SetString("LinkKey", element.acKey);
                                moveItem.SetInt("LinkNo", element.anNo);
                                moveItem.SetInt("LinkNo", receivedTrail.No);
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
                                    return true;
                                }
                                else
                                {
                                    return false;
                                }
                            }
                            else
                            {
                               return false; 
                            }
                       
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
                                return false;
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
                                    return false;
                                }
                                else
                                {
                                    return true;
                                }
                            }
                        }
                        else
                        {
                            Toast.MakeText(this, $"{Resources.GetString(Resource.String.s270)}", ToastLength.Long).Show();
                            return false;
                        }

                    }
                }
                catch (Exception ex)
                {
                    GlobalExceptions.ReportGlobalException(ex);
                    return false;
                }
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
                return false;
            }
        }




        private void CbMultipleLocations_ItemSelected(object? sender, AdapterView.ItemSelectedEventArgs e)
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

                    // This is maybe a good idea.
                    if(ssccRow.Visibility == ViewStates.Visible && !String.IsNullOrEmpty(selected.SSCC))
                    {
                        tbSSCC.Text = selected.SSCC;
                    }

                    if (serialRow.Visibility == ViewStates.Visible && !String.IsNullOrEmpty(selected.Serial))
                    {
                        tbSerialNum.Text = selected.Serial;
                    }

                    tbPacking.SelectAll();
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




        private void TbPacking_KeyPress(object? sender, View.KeyEventArgs e)
        {
            try
            {
                if (e.KeyCode == Keycode.Enter && e.Event.Action == KeyEventActions.Down)
                {
                    FilterData();
                }
                e.Handled = false;
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
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

        private async void BtCreate_Click(object? sender, EventArgs e)
        {
            try
            {
                double parsed;
           
                CheckData();

                QuantityProcessing result = QuantityProcessing.OtherError;

                if (double.TryParse(tbPacking.Text, out parsed) && createPositionAllowed)
                {
                    var element = data.ElementAt(0);
                    result = HelperMethods.IsOverTheLimitTransactionAllowed(element.anStock ?? 0, element.anMaxQty ?? 0, parsed);
                }
                else
                {
                    result = QuantityProcessing.OtherError;
                }

                if (!Base.Store.isUpdate)
                {
                    if (result != QuantityProcessing.GoodToGo)
                    {
                        if (result == QuantityProcessing.OverTheStock)
                        {
                            Toast.MakeText(this, $"{Resources.GetString(Resource.String.s353)}", ToastLength.Long).Show();
                            return;
                        }
                        else if (result == QuantityProcessing.OverTheOrdered)
                        {
                            Toast.MakeText(this, $"{Resources.GetString(Resource.String.s354)}", ToastLength.Long).Show();
                            return;
                        }
                        else if (result == QuantityProcessing.OtherError)
                        {
                            Toast.MakeText(this, $"{Resources.GetString(Resource.String.s270)}", ToastLength.Long).Show();
                            return;
                        }
                    }
                    else
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
                CheckData();

                double parsed;

                QuantityProcessing result = QuantityProcessing.OtherError;

                if (double.TryParse(tbPacking.Text, out parsed) && createPositionAllowed)
                {
                    var element = data.ElementAt(0);
                    result = HelperMethods.IsOverTheLimitTransactionAllowed(element.anStock ?? 0, element.anMaxQty ?? 0, parsed);
                }
                else
                {
                    result = QuantityProcessing.OtherError;
                }

                if (result != QuantityProcessing.GoodToGo)
                {
                    if (result == QuantityProcessing.OverTheStock)
                    {
                        Toast.MakeText(this, $"{Resources.GetString(Resource.String.s353)}", ToastLength.Long).Show();
                        return;
                    }
                    else if (result == QuantityProcessing.OverTheOrdered)
                    {
                        Toast.MakeText(this, $"{Resources.GetString(Resource.String.s354)}", ToastLength.Long).Show();
                        return;
                    }
                    else if (result == QuantityProcessing.OtherError)
                    {
                        Toast.MakeText(this, $"{Resources.GetString(Resource.String.s270)}", ToastLength.Long).Show();
                        return;
                    }
                }
                else
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


        private void TbSerialNum_KeyPress(object? sender, View.KeyEventArgs e)
        {
            try
            {
                if (e.KeyCode == Keycode.Enter && e.Event.Action == KeyEventActions.Down)
                {
                    FilterData();
                }

                e.Handled = false;
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private void TbSSCC_KeyPress(object? sender, View.KeyEventArgs e)
        {
            try
            {
                if (e.KeyCode == Keycode.Enter && e.Event.Action == KeyEventActions.Down)
                {
                    FilterData();
                }

                e.Handled = false;
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





        private async Task CreateMethodFromStart()
        {
            try
            {
                await Task.Run(async () =>
                {
                    if (dist.Count == 1)
                    {
                        var element = dist.ElementAt(0);
                        moveItem = new NameValueObject("MoveItem");
                        moveItem.SetInt("HeadID", moveHead.GetInt("HeadID"));
                        moveItem.SetString("LinkKey", element.acKey);
                        moveItem.SetInt("LinkNo", element.anNo);
                        moveItem.SetInt("LinkNo", receivedTrail.No);
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
                                if (Base.Store.modeIssuing == 3)
                                {
                                    StartActivity(typeof(ClientPickingWithTrail));
                                    Finish();
                                }
                            });

                        }
                        else
                        {
                            StartActivity(typeof(MainActivity));
                            Finish();
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
                await Task.Run(async () =>
                {
                    if (dist.Count == 1)
                    {

                        var element = dist.ElementAt(0);
                        moveItem = new NameValueObject("MoveItem");
                        moveItem.SetInt("HeadID", moveHead.GetInt("HeadID"));
                        moveItem.SetString("LinkKey", element.acKey);
                        moveItem.SetInt("LinkNo", element.anNo);
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
                            var picture = await CommonData.GetQtyPictureAsync(this);
                            RunOnUiThread(() =>
                            {

                                serialOverflowQuantity = Convert.ToDouble(tbPacking.Text.Trim());
                                stock -= serialOverflowQuantity;

                                lbQty.Text = $"{Resources.GetString(Resource.String.s83)} \n ( " + stock.ToString(picture) + " )";

                                // Check to see if the maximum is already reached.
                                if (stock <= 0)
                                {
                                    StartActivity(typeof(ClientPickingWithTrail));
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

                            });


                            createPositionAllowed = true;
                            await GetConnectedPositions(element.acKey, element.anNo, element.acIdent);
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
                await FinishMethod();
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

                // Adding the position creation to the finish button. 9.9.2024 Janko Jovičić
                if (!await SaveMoveItem())
                {
                    return;
                }

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
                                        StartActivity(typeof(IssuedGoodsBusinessEventSetupClientPicking));
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
                            // UI changes.
                            RunOnUiThread(() =>
                            {
                                Toast.MakeText(this, $"{Resources.GetString(Resource.String.s216)}" + result, ToastLength.Long).Show();
                            });
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




        private async Task SetUpForm()
        {

            try
            {
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

                if (Base.Store.isUpdate && moveItem != null)
                {
                    // Update logic ?? it seems to be true.
                    tbIdent.Text = moveItem.GetString("IdentName");
                    tbSerialNum.Text = moveItem.GetString("SerialNo");
                    tbSSCC.Text = moveItem.GetString("SSCC");
                    searchableSpinnerIssueLocation.spinnerTextValueField.Text = moveItem.GetString("Location");
                    tbPacking.Text = moveItem.GetDouble("Packing").ToString();
                    btCreateSame.Text = $"{Resources.GetString(Resource.String.s293)}";
                    lbQty.Text = $"{Resources.GetString(Resource.String.s83)} \n ( " + moveItem.GetDouble("Qty").ToString() + " )";
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
                    // Not the update ?? it seems to be true
                    tbIdent.Text = openIdent.GetString("Code") + " " + openIdent.GetString("Name");

                    if (Intent.Extras != null && Intent.GetByteArrayExtra("selected") != null)
                    {
                        byte[] trailBytes = Intent.GetByteArrayExtra("selected");
                        receivedTrail = ClientPickingPosition.Deserialize<ClientPickingPosition>(trailBytes);

                        if (await CommonData.GetSettingAsync("IssueSummaryView", this) == "1")
                        {
                            cbMultipleLocations.Visibility = ViewStates.Visible;
                            adapterLocations = await GetStockState(receivedTrail);
                            adapterLocation = new ArrayAdapter<MultipleStock>(this,
                            Android.Resource.Layout.SimpleSpinnerItem, adapterLocations);
                            adapterLocation.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
                            cbMultipleLocations.Adapter = adapterLocation;
                        }

                        searchableSpinnerIssueLocation.spinnerTextValueField.Text = receivedTrail.Location;

                        qtyCheck = Double.Parse(receivedTrail.Quantity);
                        lbQty.Text = $"{Resources.GetString(Resource.String.s83)} \n ( " + qtyCheck.ToString(await CommonData.GetQtyPictureAsync(this)) + " )";
                        stock = qtyCheck;
                        tbPacking.Text = qtyCheck.ToString();
                        await GetConnectedPositions(receivedTrail.Order, receivedTrail.No, receivedTrail.Ident);

                        FilterData();
                    }
                }

                isPackaging = openIdent.GetBool("IsPackaging");

                if (isPackaging)
                {
                    ssccRow.Visibility = ViewStates.Gone;
                    serialRow.Visibility = ViewStates.Gone;
                }

                if (ssccRow.Visibility != ViewStates.Visible && serialRow.Visibility != ViewStates.Visible)
                {
                    FilterData();
                }
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private async Task<List<MultipleStock>> GetStockState(ClientPickingPosition? obj)
        {
            try
            {
                List<MultipleStock> data = new List<MultipleStock>();

                var sql = "SELECT aclocation, anQty, acSerialNo, acSSCC FROM uWMSStockByWarehouse WHERE acIdent = @acIdent AND acWarehouse = @acWarehouse;";
                var parameters = new List<Services.Parameter>();

                parameters.Add(new Services.Parameter { Name = "acWarehouse", Type = "String", Value = moveHead.GetString("Wharehouse") });
                parameters.Add(new Services.Parameter { Name = "acIdent", Type = "String", Value = obj.Ident });

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


        /// <summary>
        /// Podatke preneseš v masko - kličeš NE isti view ampak vedno "uWMSOrderItemByKeyOut", ker moraš
        /// tudi pri subjektih zapisati na katero naročilo z pozicijo(acKey in anNo) se vrši izdaja.
        /// uWMSOrderItemByKeyOut; vhodni parameter acKey varchar(13), anNo int, acIdent varchar(16), acLocation varchar(50);
        /// izhod: acName varchar(80), acSubject varchar(30), acSerialNo varchar(100), acSSCC varchar(18), anQty decimal (19,6)
        /// če je zapis 1 potem prikažeš tiste podatke in uporabnik le potrdi
        /// če je zapisov več si jih shraniš in z dodatnimi vpisi/skeniranji(SSCC ali serijska) "filtriraš" podatke, ko prideš na enega izpolniš vse podatke, uporabnik lahko spremeni količino - v oklepaju je že od vsega začetka vpisan anQty.
        /// če uporabnik klikne na gumb serijska, se iz seznama pobriše ta vrsitca in maska ostane kot je bila po koncu koraka 4.
        /// lahko pa enostavno ponoviš klic view-a ki bi že moral imeti zapisane podatke in osvežene, če ne bo kaj težav z asinhronimi klici...
        /// </summary>
        /// <param name="acKey">Številka naročila</param>
        /// <param name="anNo">Pozicija znotraj naročila</param>
        /// <param name="acIdent">Ident</param>
        private async Task GetConnectedPositions(string acKey, int anNo, string acIdent)
        {
            try
            {
                connectedPositions.Clear();

                var parameters = new List<Services.Parameter>();

                parameters.Add(new Services.Parameter { Name = "acKey", Type = "String", Value = acKey });
                parameters.Add(new Services.Parameter { Name = "anNo", Type = "Int32", Value = anNo });
                parameters.Add(new Services.Parameter { Name = "acIdent", Type = "String", Value = acIdent });


                var subjects = await AsyncServices.AsyncServices.GetObjectListBySqlAsync($"SELECT * FROM uWMSOrderItemByKeyOut WHERE acKey = @acKey AND anNo = @anNo AND acIdent = @acIdent;", parameters, this);

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
                                acKey = row.StringValue("acKey"),
                                acIdent = row.StringValue("acIdent"),
                                anNo = (int)(row.IntValue("anNo") ?? -1),
                                anPackQty = row.DoubleValue("anPackQty") ?? 0,
                                anMaxQty = row.DoubleValue("anMaxQty"),
                                anStock = row.DoubleValue("anStock")
                            });
                        }
                    }

                    if (App.Settings.tablet)
                    {
                        items.Clear();
                        foreach (var connected in connectedPositions)
                        {
                            items.Add(new LocationClass
                            {
                                ident = connected.acIdent,
                                location = connected.aclocation,
                                serial = connected.acSerialNo,
                                sscc = connected.acSSCC,
                                quantity = connected.anQty.ToString()
                            });
                        }
                        dataAdapter.NotifyDataSetChanged();

                       
                    }
                }
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

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

        public void GetBarcode(string barcode)
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


                            FilterData();
                        }
                    }
                    else if (tbSerialNum.HasFocus)
                    {
                        if (barcode != "Scan fail")
                        {


                            tbSerialNum.Text = barcode;

                            tbPacking.RequestFocus();


                            FilterData();

                        }
                    }
                    else if (searchableSpinnerIssueLocation.spinnerTextValueField.HasFocus)
                    {
                        if (barcode != "Scan fail")
                        {


                            searchableSpinnerIssueLocation.spinnerTextValueField.Text = barcode;


                            FilterData();

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


        private void FilterData()
        {
            try
            {
                data = FilterIssuedGoods(connectedPositions, tbSSCC.Text, tbSerialNum.Text, searchableSpinnerIssueLocation.spinnerTextValueField.Text);


                // Temporary solution because of the SQL error.
                dist = data
                    .GroupBy(x => new { x.acName, x.acSSCC, x.acSerialNo, x.aclocation, x.acSubject, x.anQty })
                    .Select(g => g.First())
                    .ToList();

                if (dist.Count == 1)
                {
                    // Do stuff and allow creating the position
                    createPositionAllowed = true;
                    tbPacking.Text = dist.ElementAt(0).anQty.ToString();

                    if (ssccRow.Visibility == ViewStates.Visible)
                    {
                        tbSSCC.Text = dist.ElementAt(0).acSSCC;
                    }
                    if (serialRow.Visibility == ViewStates.Visible)
                    {
                        tbSerialNum.Text = dist.ElementAt(0).acSerialNo;
                    }

                    tbPacking.PostDelayed(() =>
                    {
                        tbPacking.RequestFocus();
                        tbPacking.SetSelection(0, tbPacking.Text.Length);
                    }, 100); // Delay in milliseconds
                }
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }
    }
}