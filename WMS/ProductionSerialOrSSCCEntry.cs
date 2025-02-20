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
using AlertDialog = Android.App.AlertDialog;
using WebApp = TrendNET.WMS.Device.Services.WebApp;
namespace WMS
{
    [Activity(Label = "WMS")]
    public class ProductionSerialOrSSCCEntry : CustomBaseActivity, IBarcodeResult
    {
        private NameValueObject moveHead = (NameValueObject)InUseObjects.Get("MoveHead");
        private NameValueObject moveItem = (NameValueObject)InUseObjects.Get("MoveItem");
        private NameValueObject openWorkOrder = null;
        private EditText tbIdent;
        private EditText tbSSCC;
        private EditText tbSerialNum;
        private EditText tbPacking;
        private TextView lbQty;
        private Button btSaveOrUpdate;
        private Button btOverview;
        private Button btFinish;
        private Button btExit;
        SoundPool soundPool;
        int soundPoolId;
        private ListView listData;
        private UniversalAdapter<ProductionSerialOrSSCCList> dataAdapter;
        private ImageView imagePNG;
        private LinearLayout rowPallet;
        private EditText tbPalletCode;

        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            try
            {
                switch (keyCode)
                {

                    case Keycode.F2:
                        if (btSaveOrUpdate.Enabled == true)
                        {
                            BtSaveOrUpdate_Click(this, null);
                        }
                        break;

                    case Keycode.F3:
                        if (btOverview.Enabled == true)
                        {
                            btOverview_Click(this, null);
                        }
                        break;

                    case Keycode.F4:
                        if (btFinish.Enabled == true)
                        {
                            btFinishClick(this, null);
                        }
                        break;

                    case Keycode.F8:
                        if (btExit.Enabled == true)
                        {
                            btExitClick(this, null);
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
        public void GetBarcode(string barcode)
        {
            try
            {
                if (tbSSCC.HasFocus)
                {
                    if (barcode != "Scan fail")
                    {
                        tbSSCC.Text = "";
                        tbSerialNum.Text = "";
                        tbPacking.Text = "";
                        searchableSpinnerLocation.spinnerTextValueField.Text = "";
                        tbIdent.Text = "";

                        tbSSCC.Text = barcode;
                        tbSerialNum.RequestFocus();

                    }
                }
                else if (tbSerialNum.HasFocus)
                {
                    if (barcode != "Scan fail")
                    {

                        tbSerialNum.Text = barcode;
                        ProcessSerialNum();
                        searchableSpinnerLocation.spinnerTextValueField.RequestFocus();
                    }
                }
                else if (searchableSpinnerLocation.spinnerTextValueField.HasFocus)
                {

                    searchableSpinnerLocation.spinnerTextValueField.Text = barcode;
                }
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private static bool? checkWorkOrderOpenQty = null;

        private async Task FillTheList()
        {
            try
            {
                data.Clear();
                var stock = await AsyncServices.AsyncServices.GetObjectListAsync("str", moveHead.GetString("Wharehouse") + "||" + identCode); 
                var number = stock.Items.Count();


                if (stock != null)
                {
                    stock.Items.ForEach(async x =>
                    {
                        var picture = await CommonData.GetQtyPictureAsync(this);

                        data.Add(new ProductionSerialOrSSCCList
                        {
                            Ident = x.GetString("Ident"),
                            Location = x.GetString("Location"),
                            Qty = x.GetDouble("RealStock").ToString(picture),
                            SerialNumber = x.GetString("SerialNo")

                        });
                    });

                    dataAdapter.NotifyDataSetChanged();

                }
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private static bool? getWorkOrderDefaultQty = null;
        private ProgressDialogClass progress;
        private Dialog popupDialogConfirm;
        private Button btnYesConfirm;
        private Button btnNoConfirm;
        private List<ProductionSerialOrSSCCList> data = new List<ProductionSerialOrSSCCList>();
        private string identCode;
        private Dialog popupDialog;
        private ZoomageView? image;
        private Barcode2D barcode2D;
        private SearchableSpinner? searchableSpinnerLocation;
        private NameValueObject? ident;
        private LinearLayout? ssccRow;
        private LinearLayout? serialRow;
        private double stock;
        private string operationQty;
        private int operationId;

       

        private async Task<bool> SaveMoveItem()
        {
            try
            {
                if (!Base.Store.isUpdate)
                {
                    if (!await HelperMethods.CanCreateProductionPosition(moveHead.GetInt("HeadID")))
                    {
                        // Create the new document and save it to internal storage 17.09.2024
                        CreateNewDocument();
                    }

                    string error;

                    if(rowPallet.Visibility == ViewStates.Visible && String.IsNullOrEmpty(tbPalletCode.Text))
                    {
                        RunOnUiThread(() =>
                        {
                            string Message = string.Format($"{Resources.GetString(Resource.String.s365)}");
                            DialogHelper.ShowDialogError(this, this, Message);

                            tbPalletCode.RequestFocus();
                        });

                        return false;
                    }



                    if(tbSSCC.Enabled && string.IsNullOrEmpty(tbSSCC.Text.Trim()))
                    {
                        RunOnUiThread(() =>
                        {
                            string Message = string.Format($"{Resources.GetString(Resource.String.s254)}");
                            DialogHelper.ShowDialogError(this, this, Message);

                            tbSSCC.RequestFocus();
                        });

                        return false;
                    }

                    if (tbSerialNum.Enabled && string.IsNullOrEmpty(tbSerialNum.Text.Trim()))
                    {
                        tbSerialNum.Text = GetNextSerialNum();
                        if (string.IsNullOrEmpty(tbSerialNum.Text.Trim()))
                        {
                            RunOnUiThread(() =>
                            {
                                string Message = string.Format($"{Resources.GetString(Resource.String.s314)}");
                                DialogHelper.ShowDialogError(this, this, Message);
                                tbSerialNum.RequestFocus();
                            });

                            return false;
                        }
                    }

                    if (!await CommonData.IsValidLocationAsync(moveHead.GetString("Wharehouse"), searchableSpinnerLocation.spinnerTextValueField.Text.Trim(), this))
                    {
                        RunOnUiThread(() =>
                        {
                            string Message = string.Format($"{Resources.GetString(Resource.String.s258)} '" + searchableSpinnerLocation.spinnerTextValueField.Text.Trim() + $"' {Resources.GetString(Resource.String.s272)} '" + moveHead.GetString("Wharehouse") + "'!");
                            DialogHelper.ShowDialogError(this, this, Message);
                            searchableSpinnerLocation.spinnerTextValueField.RequestFocus();
                        });

                        return false;
                    }

                    if (tbSSCC.Enabled)
                    {
                        var stock = Services.GetObject("sts", tbSSCC.Text.Trim(), out error);
                        if (stock == null)
                        {
                            RunOnUiThread(() =>
                            {
                                string SuccessMessage = string.Format($"{Resources.GetString(Resource.String.s213)}" + error);
                                DialogHelper.ShowDialogError(this, this, SuccessMessage);
                            });


                            return false;
                        }

                        if (stock.GetBool("ExistsSSCC"))
                        {
                            RunOnUiThread(() =>
                            {
                                string SuccessMessage = string.Format($"{Resources.GetString(Resource.String.s315)}");
                                DialogHelper.ShowDialogError(this, this, SuccessMessage);
                            });


                            return false;
                        }
                    }
                    if (string.IsNullOrEmpty(tbPacking.Text.Trim()))
                    {
                        RunOnUiThread(() =>
                        {
                            string SuccessMessage = string.Format($"{Resources.GetString(Resource.String.s270)}");
                            DialogHelper.ShowDialogError(this, this, SuccessMessage);
                        });

                        return false;
                    }
                    else
                    {
                        try
                        {
                            var qty = Convert.ToDouble(tbPacking.Text.Trim());

                            if (qty == 0.0)
                            {
                                RunOnUiThread(() =>
                                {
                                    string SuccessMessage = string.Format($"{Resources.GetString(Resource.String.s298)}");
                                    DialogHelper.ShowDialogError(this, this, SuccessMessage);
                                });


                                return false;
                            }

                
                            var max = Math.Abs(openWorkOrder.GetDouble("OpenQty"));
                            if (Math.Abs(qty) > max)
                            {
                                RunOnUiThread(() =>
                                {
                                    Toast.MakeText(this, $"{Resources.GetString(Resource.String.s354)}", ToastLength.Long).Show();
                                });
                                return false;

                            }
                            
                        }
                        catch (Exception e)
                        {
                            RunOnUiThread(() =>
                            {
                                string SuccessMessage = string.Format($"{Resources.GetString(Resource.String.s220)}");
                                DialogHelper.ShowDialogError(this, this, SuccessMessage);

                                tbPacking.RequestFocus();
                            });

                            return false;
                        }
                    }


                    if (moveItem == null) { moveItem = new NameValueObject("MoveItem"); }
                    moveItem.SetInt("HeadID", moveHead.GetInt("HeadID"));
                    moveItem.SetString("LinkKey", openWorkOrder.GetString("Key"));
                    moveItem.SetInt("LinkNo", 0);
                    moveItem.SetInt("Operation", operationId);
                    moveItem.SetString("Ident", openWorkOrder.GetString("Ident"));
                    moveItem.SetString("SSCC", tbSSCC.Text.Trim());
                    moveItem.SetString("SerialNo", tbSerialNum.Text.Trim());
                    moveItem.SetDouble("Packing", Convert.ToDouble(tbPacking.Text.Trim()));
                    moveItem.SetDouble("Factor", 1);
                    moveItem.SetDouble("Qty", Convert.ToDouble(tbPacking.Text.Trim()) * 1);
                    moveItem.SetString("Location", searchableSpinnerLocation.spinnerTextValueField.Text.Trim());
                    moveItem.SetInt("Clerk", Services.UserID());

                    moveItem = Services.SetObject("mi", moveItem, out error);
                    if (moveItem == null)
                    {
                        RunOnUiThread(() =>
                        {
                            string SuccessMessage = string.Format($"{Resources.GetString(Resource.String.s213)}" + error);
                            DialogHelper.ShowDialogError(this, this, SuccessMessage);
                        });


                        return false;
                    }
                    else
                    {
                        InUseObjects.Invalidate("MoveItem");
                        return true;
                    }
                } else
                {
                    // Update flow.
                    double newQty;
                    if (Double.TryParse(tbPacking.Text, out newQty))
                    {
                        if (newQty > moveItem.GetDouble("Qty"))
                        {
                            RunOnUiThread(() =>
                            {
                                Toast.MakeText(this, $"{Resources.GetString(Resource.String.s291)}", ToastLength.Long).Show();
                            });
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
                            if (subjects.Success)
                            {
                                InUseObjects.Invalidate("MoveItem");
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }
                    else
                    {
                        RunOnUiThread(() =>
                        {
                            Toast.MakeText(this, $"{Resources.GetString(Resource.String.s270)}", ToastLength.Long).Show();
                        });
                        return false;
                    }
                }
            }
                catch (Exception err)
                {

                    SentrySdk.CaptureException(err);
                    return false;

                }
                   
        }

        private async Task FillSugestedLocation()
        {
            try
            {
                var location = await CommonData.GetSettingAsync("DefaultProductionLocation", this);
                searchableSpinnerLocation.spinnerTextValueField.Text = location;
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private async void ProcessSerialNum()
        {
            try
            {
                if (string.IsNullOrEmpty(tbSerialNum.Text.Trim()))
                {
                    tbSerialNum.Text = GetNextSerialNum();
                    if (string.IsNullOrEmpty(tbSerialNum.Text.Trim()))
                    {

                        tbSerialNum.RequestFocus();
                        return;
                    }
                }
                
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private string GetNextSerialNum()
        {
            try
            {
                try
                {

                    string error;
                    var ident = openWorkOrder.GetString("Ident");
                    var workOrder = openWorkOrder.GetString("Key");
                    var serNumObj = Services.GetObject("sn", ident + "|" + workOrder, out error);


                    if (serNumObj != null)
                    {
                        return serNumObj.GetString("SerialNo");
                    }
                    else
                    {
                        return "";
                    }
                }
                catch (Exception err)
                {

                    SentrySdk.CaptureException(err);
                    return "";

                }
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
                return string.Empty;
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
                    base.SetContentView(Resource.Layout.ProductionSerialOrSSCCEntryTablet);
                    listData = FindViewById<ListView>(Resource.Id.listData);
                    dataAdapter = UniversalAdapterHelper.GetProductionSerialOrSSCCEntry(this, data);
                    listData.Adapter = dataAdapter;
                    imagePNG = FindViewById<ImageView>(Resource.Id.imagePNG);
                }
                else
                {
                    base.RequestedOrientation = ScreenOrientation.Portrait;
                    base.SetContentView(Resource.Layout.ProductionSerialOrSSCCEntry);
                }

                AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
                var _customToolbar = new CustomToolbar(this, toolbar, Resource.Id.navIcon);
                _customToolbar.SetNavigationIcon(App.Settings.RootURL + "/Services/Logo");
                SetSupportActionBar(_customToolbar._toolbar);
                SupportActionBar.SetDisplayShowTitleEnabled(false);
                tbIdent = FindViewById<EditText>(Resource.Id.tbIdent);
                tbSSCC = FindViewById<EditText>(Resource.Id.tbSSCC);
                tbSerialNum = FindViewById<EditText>(Resource.Id.tbSerialNum);
                tbPacking = FindViewById<EditText>(Resource.Id.tbPacking);
                tbPacking.SetSelectAllOnFocus(true);
                lbQty = FindViewById<TextView>(Resource.Id.lbQty);
                btSaveOrUpdate = FindViewById<Button>(Resource.Id.btSaveOrUpdate);
                btOverview = FindViewById<Button>(Resource.Id.btOverview);
                btFinish = FindViewById<Button>(Resource.Id.btFinish);
                btExit = FindViewById<Button>(Resource.Id.btExit);
                tbIdent.InputType = Android.Text.InputTypes.ClassNumber;
                tbSSCC.InputType = Android.Text.InputTypes.ClassNumber;
                tbSerialNum.InputType = Android.Text.InputTypes.ClassNumber;
                var _broadcastReceiver = new NetworkStatusBroadcastReceiver();
                _broadcastReceiver.ConnectionStatusChanged += OnNetworkStatusChanged;
                Application.Context.RegisterReceiver(_broadcastReceiver,
                new IntentFilter(ConnectivityManager.ConnectivityAction), ReceiverFlags.NotExported);
                ssccRow = FindViewById<LinearLayout>(Resource.Id.sscc_row);
                serialRow = FindViewById<LinearLayout>(Resource.Id.serial_row);
                rowPallet = FindViewById<LinearLayout>(Resource.Id.pallet_row);
                tbPalletCode = FindViewById<EditText>(Resource.Id.tbPalletCode);

                btSaveOrUpdate.Click += BtSaveOrUpdate_Click;
                btOverview.Click += btOverview_Click;
                btFinish.Click += btFinishClick;
                btExit.Click += btExitClick;
                tbSSCC.FocusChange += TbSSCC_FocusChange;
                barcode2D = new Barcode2D(this, this);

                searchableSpinnerLocation = FindViewById<SearchableSpinner>(Resource.Id.searchableSpinnerLocation);
                var locations = await HelperMethods.GetLocationsForGivenWarehouse(moveHead.GetString("Wharehouse"));
                searchableSpinnerLocation.SetItems(locations);
                searchableSpinnerLocation.ColorTheRepresentation(1);
                searchableSpinnerLocation.ShowDropDown();


                var isPalletCodeHidden = await CommonData.GetSettingAsync("Pi.HideLegCode", this);

                if (isPalletCodeHidden != null)
                {
                    if (isPalletCodeHidden == "1")
                    {
                        rowPallet.Visibility = ViewStates.Gone;
                    }              
                }


                CheckIfApplicationStopingException();

                // Color the fields that can be scanned
                ColorFields();

                // Stop the loader
                SetUpProcessDependentButtons();

                SetUpOperationData();
                // Main logic for the entry
                await SetUpForm();

                await FillSugestedLocation();

                if (App.Settings.tablet)
                {
                    await FillTheList();
                }
            
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private void SetUpOperationData()
        {
            operationQty = Intent.GetStringExtra("Qty") ?? "0";  // Default to "0" if null
            string operationIdString = Intent.GetStringExtra("OperationId") ?? "0"; // Default to "0" if null
            if (!int.TryParse(operationIdString, out operationId))
            {
                operationId = 0; // Default to 0 if parsing fails
            }
        }

        private async Task SetUpForm()
        {
            try
            {
                var key = moveHead.GetString("LinkKey");

                string error;

                openWorkOrder = Services.GetObject("wo", key, out error);

                if (openWorkOrder == null)
                {
                    StartActivity(typeof(MainMenu));
                }

                lbQty.Text = $"{Resources.GetString(Resource.String.s40)} (" + operationQty.ToString() + ")";

                stock = openWorkOrder.GetDouble("OpenQty");
                ident = await CommonData.LoadIdentAsync(openWorkOrder.GetString("Ident"), this);

                if (ident != null)
                {
                    showPictureIdent(ident.GetString("Code"));
                    identCode = ident.GetString("Code");
                    tbIdent.Text = ident.GetString("Code") + " " + ident.GetString("Name");

                    if(!ident.GetBool("isSSCC"))
                    {
                        ssccRow.Visibility = ViewStates.Gone;
                    }

                    if(!ident.GetBool("HasSerialNumber"))
                    {
                        serialRow.Visibility = ViewStates.Gone;
                    }

                    tbSSCC.Enabled = ident.GetBool("isSSCC");
                    tbSerialNum.Enabled = ident.GetBool("HasSerialNumber");
                } else
                {
                    StartActivity(typeof(MainMenu));
                }


                if (Base.Store.isUpdate)
                {

                    tbSSCC.Text = moveItem.GetString("SSCC");
                    tbSerialNum.Text = moveItem.GetString("SerialNo");
                    lbQty.Text = $"{Resources.GetString(Resource.String.s40)} (" + moveItem.GetDouble("Packing") + ")";
                    stock = moveItem.GetDouble("Packing");
                    tbPacking.Text = moveItem.GetDouble("Packing").ToString(await CommonData.GetQtyPictureAsync(this));
                    searchableSpinnerLocation.spinnerTextValueField.Text = moveItem.GetString("Location");


                    searchableSpinnerLocation.spinnerTextValueField.Enabled = false;
                    tbSSCC.Enabled = false;
                    tbSerialNum.Enabled = false;
                    tbIdent.Enabled = false;
                    
                    searchableSpinnerLocation.spinnerTextValueField.Enabled = false;
                    tbSSCC.Enabled = false;
                    tbSerialNum.Enabled = false;
                    tbIdent.Enabled = false;                   
                    tbPacking.RequestFocus();
                }
                else
                {
                    if (tbSSCC.Enabled)
                    {
                        tbSSCC.RequestFocus();
                    }
                    else if (tbSerialNum.Enabled)
                    {
                        tbSerialNum.RequestFocus();
                    }
                    else
                    {
                        tbPacking.RequestFocus();
                    }

                    if (tbSSCC.Enabled && (await CommonData.GetSettingAsync("AutoCreateSSCCProduction", this) == "1"))
                    {
                        tbSSCC.Text = await CommonData.GetNextSSCCAsync(this);
                        tbPacking.RequestFocus();
                    }

                    ProcessSerialNum();
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
                // UI changes.
                RunOnUiThread(() =>
                {
                    // This method changes the UI so it shows in a visible way that it is the update screen. - 18.03.2024
                    if (Base.Store.isUpdate)
                    {
                        btSaveOrUpdate.Text = $"{Resources.GetString(Resource.String.s290)}";
                    }
               
                });
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
                if (moveHead != null)
                {
                    // No error here, safe (ish) to continue
                    return;
                }
                else
                {
                    // Destroy the activity

                    StartActivity(typeof(MainMenu));
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



        private async void TbSSCC_FocusChange(object sender, View.FocusChangeEventArgs e)
        {
            try
            {
                await FillSugestedLocation();
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
                tbPalletCode.SetBackgroundColor(Android.Graphics.Color.Aqua);
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }


        private void btExitClick(object sender, EventArgs e)
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



        private async Task FinishMethod()
        {
            try
            {
                await Task.Run(async () =>
                {
                    bool resultAsync;

                    resultAsync = await SaveMoveItem();
                    
                    if (resultAsync)
                    {
                        var headID = moveHead.GetInt("HeadID");

                        try
                        {

                            bool success;
                            string? result;

                            // Changes for the pallet process. 20.02.2025 Janko Jovičić
                            if (rowPallet.Visibility == ViewStates.Visible)
                            {
                                (success, result) = await WebApp.GetAsync($"mode=finish&stock=add&paletteCode={tbPalletCode.Text}&print=" + Services.DeviceUser() + "&id=" + headID.ToString(), this);
                            } else
                            {
                                (success, result) = await WebApp.GetAsync("mode=finish&stock=add&print=" + Services.DeviceUser() + "&id=" + headID.ToString(), this);
                            }

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
                                            Base.Store.isUpdate = false;
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
                                            Base.Store.isUpdate = false;
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
                                    AlertDialog.Builder alert = new AlertDialog.Builder(this);
                                    alert.SetTitle($"{Resources.GetString(Resource.String.s265)}");
                                    alert.SetMessage($"{Resources.GetString(Resource.String.s218)}" + result);

                                    alert.SetPositiveButton("Ok", (senderAlert, args) =>
                                    {
                                        Base.Store.isUpdate = false;
                                        alert.Dispose();
                                        StartActivity(typeof(MainMenu));
                                        Finish();

                                    });

                                    Dialog dialog = alert.Create();
                                    dialog.Show();
                                });

                            }
                        }
                        catch (Exception ex)
                        {
                            SentrySdk.CaptureException(ex);
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }


        private void btFinishClick(object sender, EventArgs e)
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
                popupDialogConfirm.Dismiss();
                popupDialogConfirm.Hide();

                LoaderManifest.LoaderManifestLoopResources(this);

                await FinishMethod();
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            } finally
            {
                LoaderManifest.LoaderManifestLoopStop(this);
            }

        }

        private void btOverview_Click(object sender, EventArgs e)
        {
            try
            {
                StartActivity(typeof(ProductionEnteredPositionsView));
                Finish();
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
                string location = string.Empty;


                // TODO: Add a way to check serial numbers
                location = searchableSpinnerLocation.spinnerTextValueField.Text;


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



        private async Task CreateMethodSame()
        {
            try
            {
               
                    try
                    {
                        LoaderManifest.LoaderManifestLoopResources(this);

                        if (await SaveMoveItem())
                        {
                          
                            if (Base.Store.isUpdate)
                            {
                                Base.Store.isUpdate = false;
                                StartActivity(typeof(ProductionEnteredPositionsView));
                                Finish();
                            }
                            else
                            {
                                Base.Store.isUpdate = false;
                                StartActivity(typeof(ProductionSerialOrSSCCEntry));
                                Finish();
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

        private void CreateNewDocument()
        {
            try
            {
                string error;
                var moveHeadCreate = new NameValueObject("MoveHead");
                moveHeadCreate.SetInt("Clerk", Services.UserID());
                moveHeadCreate.SetString("Type", "W");
                moveHeadCreate.SetString("LinkKey", moveHead.GetString("LinkKey"));
                moveHeadCreate.SetString("LinkNo", moveHead.GetString("LinkNo"));
                moveHeadCreate.SetString("Document1", "");
                moveHeadCreate.SetDateTime("Document1Date", null);
                moveHeadCreate.SetString("Note", "");
                moveHeadCreate.SetString("Issuer", "");
                moveHeadCreate.SetString("Receiver", "");
                moveHeadCreate.SetString("Wharehouse", moveHead.GetString("Wharehouse"));
                moveHeadCreate.SetString("DocumentType", moveHead.GetString("DocumentType"));
                
                var savedMoveHead = Services.SetObject("mh", moveHeadCreate, out error);
                if (savedMoveHead == null)
                {
                    StartActivity(typeof(ProductionWorkOrderSetup));
                    Finish();
                } else
                {
                    moveHeadCreate.SetInt("HeadID", savedMoveHead.GetInt("HeadID"));
                    moveHeadCreate.SetBool("Saved", true);
                    InUseObjects.Set("MoveHead", moveHeadCreate);
                    moveHead = (NameValueObject)InUseObjects.Get("MoveHead");
                    // Refresh the object
                }

            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private async void BtSaveOrUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                try
                {
                    LoaderManifest.LoaderManifestLoopResources(this);

                    double parsed;
                    if (double.TryParse(tbPacking.Text, out parsed))
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
                        return;
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
    }
}