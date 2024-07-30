using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Media;
using Android.Net;
using Android.Views;
using BarCode2D_Receiver;
using Com.Jsibbold.Zoomage;
using TrendNET.WMS.Core.Data;
using TrendNET.WMS.Device.App;
using TrendNET.WMS.Device.Services;
using WMS.App;
using WMS.ExceptionStore;
using static Android.App.ActionBar;
using static Android.Views.ViewTreeObserver;
using AlertDialog = Android.App.AlertDialog;
using WebApp = TrendNET.WMS.Device.Services.WebApp;

namespace WMS
{
    [Activity(Label = "WMS")]
    public class InterWarehouseSerialOrSSCCEntry : CustomBaseActivity, IBarcodeResult
    {
        private EditText? tbIdent;
        private EditText? tbSSCC;
        private EditText? tbSerialNum;
        private EditText? tbPacking;
        private TextView? lbQty;
        private ImageView? imagePNG;
        private EditText? lbIdentName;
        private Button? btSaveOrUpdate;
        private Button? btCreate;
        private Button? btFinish;
        private Button? btOverview;
        private Button? btExit;
        private SoundPool soundPool;
        private int soundPoolId;
        private Barcode2D barcode2D;
        private LinearLayout ssccRow;
        private LinearLayout serialRow;
        private NameValueObject openIdent = (NameValueObject)InUseObjects.Get("OpenIdent");
        private NameValueObject openOrder = (NameValueObject)InUseObjects.Get("OpenOrder");
        private NameValueObject moveHead = (NameValueObject)InUseObjects.Get("MoveHead");
        private NameValueObject moveItem = (NameValueObject)InUseObjects.Get("MoveItem");
        private double? stock;
        private NameValueObject activityIdent;
        private double qtyCheck;
        private Dialog popupDialogConfirm;
        private Button? btnYesConfirm;
        private Button? btnNoConfirm;
        private ProgressDialogClass progress;
        private ListView listData;
        private UniversalAdapter<LocationClass> dataAdapter;
        private List<LocationClass> items = new List<LocationClass>();
        private int selected;
        private Dialog popupDialog;
        private ZoomageView image;
        private SearchableSpinner? searchableSpinnerIssueLocation;
        private SearchableSpinner? searchableSpinnerReceiveLocation;

        protected async override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                SetTheme(Resource.Style.AppTheme_NoActionBar);

                SetContentView(Resource.Layout.InterWarehouseSerialOrSSCCEntry);


                if (App.Settings.tablet)
                {
                    base.RequestedOrientation = ScreenOrientation.Landscape;
                    base.SetContentView(Resource.Layout.InterWarehouseSerialOrSSCCEntryTablet);
                    listData = FindViewById<ListView>(Resource.Id.listData);
                    dataAdapter = UniversalAdapterHelper.GetInterWarehouseSerialOrSSCCEntry(this, items);
                    listData.Adapter = dataAdapter;
                    imagePNG = FindViewById<ImageView>(Resource.Id.imagePNG);
                    imagePNG.Visibility = ViewStates.Invisible;
                }
                else
                {
                    base.RequestedOrientation = ScreenOrientation.Portrait;
                    base.SetContentView(Resource.Layout.InterWarehouseSerialOrSSCCEntry);
                }

                AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
                var _customToolbar = new CustomToolbar(this, toolbar, Resource.Id.navIcon);
                _customToolbar.SetNavigationIcon(App.Settings.RootURL + "/Services/Logo");
                SetSupportActionBar(_customToolbar._toolbar);
                SupportActionBar.SetDisplayShowTitleEnabled(false);
                Window.SetSoftInputMode(Android.Views.SoftInput.AdjustResize);

                btSaveOrUpdate = FindViewById<Button>(Resource.Id.btSaveOrUpdate);
                btCreate = FindViewById<Button>(Resource.Id.btCreate);
                btFinish = FindViewById<Button>(Resource.Id.btFinish);
                btOverview = FindViewById<Button>(Resource.Id.btOverview);
                btExit = FindViewById<Button>(Resource.Id.btExit);

                btSaveOrUpdate.Click += BtSaveOrUpdate_Click;
                btCreate.Click += BtCreate_Click;
                btFinish.Click += BtFinish_Click;
                btOverview.Click += BtOverview_Click;
                btExit.Click += BtExit_Click;

                tbIdent = FindViewById<EditText>(Resource.Id.tbIdent);
                tbSSCC = FindViewById<EditText>(Resource.Id.tbSSCC);
                tbSerialNum = FindViewById<EditText>(Resource.Id.tbSerialNum);

                tbPacking = FindViewById<EditText>(Resource.Id.tbPacking);
                tbPacking.SetSelectAllOnFocus(true);
                tbSSCC.InputType = Android.Text.InputTypes.ClassNumber;
                lbQty = FindViewById<TextView>(Resource.Id.lbQty);
                imagePNG = FindViewById<ImageView>(Resource.Id.imagePNG);
                lbIdentName = FindViewById<EditText>(Resource.Id.lbIdentName);
                lbIdentName = FindViewById<EditText>(Resource.Id.lbIdentName);
                ssccRow = FindViewById<LinearLayout>(Resource.Id.sscc_row);
                serialRow = FindViewById<LinearLayout>(Resource.Id.serial_row);
                barcode2D = new Barcode2D(this, this);
                var _broadcastReceiver = new NetworkStatusBroadcastReceiver();
                _broadcastReceiver.ConnectionStatusChanged += OnNetworkStatusChanged;
                Application.Context.RegisterReceiver(_broadcastReceiver,
                new IntentFilter(ConnectivityManager.ConnectivityAction), ReceiverFlags.NotExported);
                tbIdent.KeyPress += TbIdent_KeyPress;
                tbSSCC.KeyPress += TbSSCC_KeyPress;
                tbSerialNum.KeyPress += TbSerialNum_KeyPress;

                searchableSpinnerIssueLocation = FindViewById<SearchableSpinner>(Resource.Id.searchableSpinnerIssueLocation);
                var locationsIssuer = await HelperMethods.GetLocationsForGivenWarehouse(moveHead.GetString("Issuer"));
                searchableSpinnerIssueLocation.SetItems(locationsIssuer);
                searchableSpinnerIssueLocation.ColorTheRepresentation(1);

                searchableSpinnerReceiveLocation = FindViewById<SearchableSpinner>(Resource.Id.searchableSpinnerReceiveLocation);
                var locationsReceiver = await HelperMethods.GetLocationsForGivenWarehouse(moveHead.GetString("Receiver"));
                searchableSpinnerReceiveLocation.SetItems(locationsReceiver);
                searchableSpinnerReceiveLocation.ColorTheRepresentation(1);

                tbPacking.FocusChange += TbPacking_FocusChange;
                // Method calls

                CheckIfApplicationStopingException();

                // Color the fields that can be scanned
                ColorFields();

                SetUpProcessDependentButtons();
                // Main logic for the entry
                SetUpForm();
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private async void TbPacking_FocusChange(object? sender, View.FocusChangeEventArgs e)
        {
            try
            {
                if (e.HasFocus)
                {
                    await LoadStock(searchableSpinnerIssueLocation.spinnerTextValueField.Text, tbIdent.Text, moveHead.GetString("Issuer"), tbSSCC.Text, tbSerialNum.Text);
                }
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

     
        private void PopupDialog_KeyPress(object sender, DialogKeyEventArgs e)
        {
            try
            {
                if (e.KeyCode == Keycode.Back)
                {
                    popupDialog.Dismiss();
                    popupDialog.Hide();
                    popupDialog.Window.Dispose();
                }
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private void Fill(List<LocationClass> list)
        {
            try
            {
                foreach (var obj in list)
                {
                    items.Add(new LocationClass { ident = obj.ident, location = obj.location, quantity = obj.quantity });

                }
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private void ListData_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            try
            {
                selected = e.Position;
                var item = items.ElementAt(selected);
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }
        private async Task FillTheIdentLocationList(string ident)
        {
            try
            {
                var wh = moveHead.GetString("Receiver");
                var list = await AdapterStore.getStockForWarehouseAndIdent(wh, ident);
                Fill(list);
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

    

        private void TbSerialNum_KeyPress(object? sender, View.KeyEventArgs e)
        {
            try
            {
                e.Handled = false;

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
                    e.Handled = true;

                    await FillDataBySSCC(tbSSCC.Text);
                } else
                {
                    e.Handled = false;
                }
              
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private async void TbIdent_KeyPress(object? sender, View.KeyEventArgs e)
        {
            try
            {
                if (e.KeyCode == Keycode.Enter && e.Event.Action == KeyEventActions.Down)
                {
                    await ProcessIdent(false);
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
                Finish();
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
                StartActivity(typeof(InterWarehouseEnteredPositionsView));
                Finish();
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
                // Access Popup layout fields like below
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



        private async Task FinishMethod()
        {
            try
            {
                await Task.Run(async () =>
                {

                    try
                    {
                        var headID = moveHead.GetInt("HeadID");
                        var (success, result) = await WebApp.GetAsync("mode=finish&stock=add&print=" + Services.DeviceUser() + "&id=" + headID.ToString(), this);
                        if (success)
                        {
                            if (result.StartsWith("OK!"))
                            {
                                RunOnUiThread(() =>
                                {
                                    var id = result.Split('+')[1];
                                    AlertDialog.Builder alert = new AlertDialog.Builder(this);
                                    alert.SetTitle($"{Resources.GetString(Resource.String.s263)}");
                                    alert.SetMessage($"{Resources.GetString(Resource.String.s264)}" + id);
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
                            RunOnUiThread(() =>
                            {
                                DialogHelper.ShowDialogError(this, this, $"{Resources.GetString(Resource.String.s218)}" + result);
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


        private async void BtCreate_Click(object? sender, EventArgs e)
        {
            try
            {
                try
                {
                    LoaderManifest.LoaderManifestLoopResources(this);
                    if (!Base.Store.isUpdate)
                    {
                        double parsed;
                        if (double.TryParse(tbPacking.Text, out parsed) && stock >= parsed)
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

                if (qty != null && qty.Success)
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


        private async Task <bool> IsLocationCorrect()
        {
            try
            {
                string locationIssuer = searchableSpinnerIssueLocation.spinnerTextValueField.Text;
                string locationReceiver = searchableSpinnerReceiveLocation.spinnerTextValueField.Text;

                if (await CommonData.IsValidLocationAsync(moveHead.GetString("Issuer"), locationIssuer, this) && await CommonData.IsValidLocationAsync(moveHead.GetString("Receiver"), locationReceiver, this))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
                return false;
            }
        }


        private async void BtSaveOrUpdate_Click(object? sender, EventArgs e)
        {
            try
            {
                try
                {
                    LoaderManifest.LoaderManifestLoopResources(this);

                    if (activityIdent != null)
                    {
                        if (activityIdent.GetBool("isSSCC") && tbSSCC.Text != string.Empty)
                        {
                            await CreateMethodFromStart();
                        }
                        else
                        {
                            lbQty.Text = Resources.GetString(Resource.String.s83);
                            tbSerialNum.Text = string.Empty;
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

        private void CheckIfApplicationStopingException()
        {
            try
            {
                if (moveItem == null && moveHead == null)
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


        private async Task CreateMethodFromStart()
        {
            try
            {
                await Task.Run(() =>
                {

                    moveItem = new NameValueObject("MoveItem");


                    moveItem.SetInt("HeadID", moveHead.GetInt("HeadID"));
                    moveItem.SetString("LinkKey", string.Empty);
                    moveItem.SetInt("LinkNo", 0);
                    moveItem.SetString("Ident", tbIdent.Text);
                    moveItem.SetString("SSCC", tbSSCC.Text.Trim());
                    moveItem.SetString("SerialNo", tbSerialNum.Text.Trim());
                    moveItem.SetDouble("Packing", Convert.ToDouble(tbPacking.Text.Trim()));
                    moveItem.SetDouble("Factor", 1);
                    moveItem.SetDouble("Qty", Convert.ToDouble(tbPacking.Text.Trim()));
                    moveItem.SetInt("Clerk", Services.UserID());
                    moveItem.SetString("Location", searchableSpinnerReceiveLocation.spinnerTextValueField.Text.Trim());
                    moveItem.SetString("IssueLocation", searchableSpinnerIssueLocation.spinnerTextValueField.Text.Trim());
                    moveItem.SetString("Palette", "1");



                    string error;

                    moveItem = Services.SetObject("mi", moveItem, out error);

                    if (moveItem != null && error == string.Empty)
                    {
                        RunOnUiThread(() =>
                        {
                            StartActivity(typeof(InterWarehouseSerialOrSSCCEntry));
                        });

                    }
                });
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }




        private async void SetUpForm()
        {
            try
            {
                // This is the default focus of the view.
                tbSSCC.RequestFocus();

                if (Base.Store.isUpdate && moveItem != null)
                {
                    tbIdent.Text = moveItem.GetString("Ident");
                    await ProcessIdent(true);
                    tbSerialNum.Text = moveItem.GetString("SerialNo");
                    tbPacking.Text = moveItem.GetDouble("Qty").ToString();
                    tbSSCC.Text = moveItem.GetString("SSCC");
                    searchableSpinnerIssueLocation.spinnerTextValueField.Text = moveItem.GetString("IssueLocation");
                    searchableSpinnerReceiveLocation.spinnerTextValueField.Text = moveItem.GetString("Location");
                    lbQty.Text = $"{Resources.GetString(Resource.String.s83)} ( " + moveItem.GetDouble("Qty").ToString() + " )";
                    tbIdent.Enabled = false;
                    tbSerialNum.Enabled = false;
                    tbSSCC.Enabled = false;
                    searchableSpinnerIssueLocation.spinnerTextValueField.Enabled = false;
                    searchableSpinnerReceiveLocation.spinnerTextValueField.Enabled = false;
                    stock = moveItem.GetDouble("Qty");
                    tbPacking.RequestFocus();
                    tbPacking.SelectAll();
                }
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }



        public async void GetBarcode(string barcode)
        {
            try
            {
                if (barcode != "Scan fail")
                {
                    if (tbIdent.HasFocus)
                    {

                        tbIdent.Text = barcode;
                        await ProcessIdent(false);
                        tbSSCC.RequestFocus();
                    }
                    else if (tbSSCC.HasFocus)
                    {

                        await FillDataBySSCC(barcode);
                        tbSSCC.Text = barcode;
                    }
                    else if (tbSerialNum.HasFocus)
                    {

                        tbSerialNum.Text = barcode;
                        searchableSpinnerIssueLocation.spinnerTextValueField.RequestFocus();
                    }
                    else if (searchableSpinnerIssueLocation.spinnerTextValueField.HasFocus)
                    {

                        searchableSpinnerIssueLocation.spinnerTextValueField.Text = barcode;
                        await LoadStock(searchableSpinnerIssueLocation.spinnerTextValueField.Text, tbIdent.Text, moveHead.GetString("Issuer"), tbSSCC.Text, tbSerialNum.Text);
                        searchableSpinnerReceiveLocation.spinnerTextValueField.RequestFocus();
                    }
                    else if (searchableSpinnerReceiveLocation.spinnerTextValueField.HasFocus)
                    {

                        searchableSpinnerReceiveLocation.spinnerTextValueField.Text = barcode;
                        tbPacking.RequestFocus();
                    }
                }
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private async Task FillDataBySSCC(string sscc)
        {
            try
            {
                var parameters = new List<Services.Parameter>();
                string warehouse = moveHead.GetString("Issuer");
                parameters.Add(new Services.Parameter { Name = "acSSCC", Type = "String", Value = sscc });
                parameters.Add(new Services.Parameter { Name = "acWarehouse", Type = "String", Value = warehouse });
                string sql = $"SELECT acIdent, aclocation, acSerialNo, acSSCC, anQty FROM uWMSItemBySSCCWarehouse WHERE acSSCC = @acSSCC AND acWarehouse = @acWarehouse";
                var ssccResult = await AsyncServices.AsyncServices.GetObjectListBySqlAsync(sql, parameters);
                RunOnUiThread(() =>
                {
                    if (ssccResult.Success && ssccResult.Rows.Count > 0)
                    {
                        tbIdent.Text = ssccResult.Rows[0].StringValue("acIdent");
                        // Process ident, recommended location is processed as well. 23.04.2024 Janko Jovičić
                        Task.Run(async () => await ProcessIdent(false)).Wait();
                        searchableSpinnerIssueLocation.spinnerTextValueField.Text = ssccResult.Rows[0].StringValue("aclocation");
                        tbSerialNum.Text = ssccResult.Rows[0].StringValue("acSerialNo");
                        tbSSCC.Text = ssccResult.Rows[0].StringValue("acSSCC").ToString();
                        lbQty.Text = $"{Resources.GetString(Resource.String.s83)} ( " + ssccResult.Rows[0].DoubleValue("anQty").ToString() + " )";
                        tbPacking.Text = ssccResult.Rows[0].DoubleValue("anQty").ToString();
                        stock = ssccResult.Rows[0].DoubleValue("anQty");

                        searchableSpinnerReceiveLocation.spinnerTextValueField.RequestFocus();

                        // Post a runnable to select text after the focus is set
                        searchableSpinnerReceiveLocation.spinnerTextValueField.Post(() =>
                        {
                            searchableSpinnerReceiveLocation.spinnerTextValueField.SetSelection(0, searchableSpinnerReceiveLocation.spinnerTextValueField.Text.Length); // Select all text
                        });
                    }
                    else
                    {
                        Toast.MakeText(this, Resources.GetString(Resource.String.s337), ToastLength.Long).Show();
                    }
                });
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }


        private async Task ProcessIdent(bool update)
        {
            try
            {
                activityIdent = await CommonData.LoadIdentAsync(tbIdent.Text.Trim(), this);

                if (activityIdent != null)
                {
                    if (!activityIdent.GetBool("isSSCC"))
                    {
                        RunOnUiThread(() =>
                        {
                            ssccRow.Visibility = ViewStates.Gone;
                        });
                      
                    }

                    if (!activityIdent.GetBool("HasSerialNumber"))
                    {
                       
                        RunOnUiThread(() =>
                        {
                            serialRow.Visibility = ViewStates.Gone;
                        });
                    }

                    if (activityIdent == null)
                    {
                      
                        RunOnUiThread(() =>
                        {
                            tbIdent.Text = "";
                            lbIdentName.Text = "";
                        });
                        return;
                    }

                    if (await CommonData.GetSettingAsync("IgnoreStockHistory", this) != "1" && !update)
                    {
                        try
                        {
                            string error;
                            var recommededLocation = Services.GetObject("rl", activityIdent.GetString("Code") + "|" + moveHead.GetString("Receiver"), out error);
                            if (recommededLocation != null)
                            {

                                RunOnUiThread(() =>
                                {
                                    searchableSpinnerReceiveLocation.spinnerTextValueField.Text = recommededLocation.GetString("Location");
                                });
                              
                            }
                        }
                        catch (Exception err)
                        {

                            SentrySdk.CaptureException(err);
                            return;

                        }
                    }

                    lbIdentName.Text = activityIdent.GetString("Name");

                    if (!update)
                    {
                        RunOnUiThread(() =>
                        {
                            tbSSCC.Enabled = activityIdent.GetBool("isSSCC");
                            tbSerialNum.Enabled = activityIdent.GetBool("HasSerialNumber");
                        });
                      
                    }
                    else
                    {
                        RunOnUiThread(() =>
                        {
                            lbIdentName.Enabled = false;
                        });

                    }

                    await FillTheIdentLocationList(activityIdent.GetString("Code"));


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
                tbIdent.SetBackgroundColor(Android.Graphics.Color.Aqua);
                tbSSCC.SetBackgroundColor(Android.Graphics.Color.Aqua);
                tbSerialNum.SetBackgroundColor(Android.Graphics.Color.Aqua);
                searchableSpinnerReceiveLocation.spinnerTextValueField.SetBackgroundColor(Android.Graphics.Color.Aqua);
                searchableSpinnerIssueLocation.spinnerTextValueField.SetBackgroundColor(Android.Graphics.Color.Aqua);
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
                    btSaveOrUpdate.Visibility = ViewStates.Gone;
                    btCreate.Text = $"{Resources.GetString(Resource.String.s290)}";
                }
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }


        private void OnNetworkStatusChanged(object? sender, EventArgs e)
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

    }
}