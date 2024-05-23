using Stream = Android.Media.Stream;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.Media;
using Android.Net;
using Android.Nfc;
using Android.OS;
using Android.Runtime;
using Android.Text.Util;
using Android.Views;
using Android.Widget;
using BarCode2D_Receiver;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Newtonsoft.Json;
using WMS.App;
using TrendNET.WMS.Core.Data;
using TrendNET.WMS.Device.App;
using TrendNET.WMS.Device.Services;
using static Android.App.ActionBar;
using static Android.Graphics.Paint;
using static Android.Icu.Text.Transliterator;
using WebApp = TrendNET.WMS.Device.Services.WebApp;
using System.Text.Json;

using AndroidX.AppCompat.App;
using AlertDialog = Android.App.AlertDialog;
using Aspose.Words.Drawing;
using Android.Graphics.Drawables;
using Android.Renderscripts;
using Com.Jsibbold.Zoomage;
using AndroidX.Lifecycle;
using System.Data.Common;
using static WMS.App.MultipleStock;

namespace WMS
{
    [Activity(Label = "IssuedGoodsSerialOrSSCCEntry", ScreenOrientation = ScreenOrientation.Portrait)]
    public class IssuedGoodsSerialOrSSCCEntry : CustomBaseActivity, IBarcodeResult
    {
        private static bool? checkIssuedOpenQty = null;
        private MorePalletsAdapter adapter;
        private MorePalletsAdapter adapterNew;
        private Button btConfirm;
        private Button btCreate;
        private Button btCreateSame;
        private Button btExit;
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
        private ProgressDialogClass progress;
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
        private EditText tbLocation;
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

        public void GetBarcode(string barcode)
        {
            try
            {
                if (tbSSCC.HasFocus)
                {
                    if (barcode != "Scan fail")
                    {
                        Sound();

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
                        Sound();

                        tbSerialNum.Text = barcode;

                        tbPacking.RequestFocus();

                        FilterData();
                    }
                }
                else if (tbLocation.HasFocus)
                {
                    if (barcode != "Scan fail")
                    {
                        Sound();

                        tbLocation.Text = barcode;

                        FilterData();

                        if(isProccessOrderless)
                        {
                            GetQuantityOrderLess();
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                Crashes.TrackError(ex);
                Toast.MakeText(this, $"{Resources.GetString(Resource.String.s225)}", ToastLength.Long).Show();
            }
        }

        private void GetQuantityOrderLess()
        {
            if (openIdent != null && moveHead != null)
            {
                string location = tbLocation.Text;
                string ident = openIdent.GetString("Code");
                string warehouse = moveHead.GetString("Wharehouse");
                string sscc = string.IsNullOrEmpty(tbSSCC.Text) ? null : tbSSCC.Text;
                string serial = string.IsNullOrEmpty(tbSerialNum.Text) ? null : tbSerialNum.Text;
                LoadStock(location, ident, warehouse, sscc, serial);
            }
        }


        private async void LoadStock(string location, string ident, string warehouse, string sscc = null, string serial = null)
        {
            var parameters = new List<Services.Parameter>();

            parameters.Add(new Services.Parameter { Name = "acIdent", Type = "String", Value = ident });
            parameters.Add(new Services.Parameter { Name = "aclocation", Type = "String", Value = location });
            parameters.Add(new Services.Parameter { Name = "acWarehouse", Type = "String", Value = warehouse });

            string sql = "SELECT TOP 1 anQty FROM uWMSStockByWarehouse WHERE acIdent = @acIdent AND aclocation = @aclocation AND acWarehouse = @acWarehouse";

            if(sscc!=null)
            {
                sql += " AND acSSCC = @acSSCC";
                parameters.Add(new Services.Parameter { Name = "acSSCC", Type = "String", Value = sscc });
            }

            if (serial != null)
            {
                sql += " AND acSerialNo = @acSerialNo;";
                parameters.Add(new Services.Parameter { Name = "acSerialNo", Type = "String", Value = serial });
            }

            var qty = await AsyncServices.AsyncServices.GetObjectListBySqlAsync(sql, parameters);

            if(qty.Success)
            {

                if (qty.Rows.Count > 0)
                {
                    double result = (double?)qty.Rows[0].DoubleValue("anQty") ?? 0;
                    qtyCheck = result;
                    lbQty.Text = $"{Resources.GetString(Resource.String.s83)} ( " + qtyCheck.ToString(CommonData.GetQtyPicture()) + " )";
                    tbPacking.Text = qtyCheck.ToString();
                    stock = qtyCheck;
                } else
                {
                    double result =  0;
                    qtyCheck = result;
                    lbQty.Text = $"{Resources.GetString(Resource.String.s83)} ( " + qtyCheck.ToString(CommonData.GetQtyPicture()) + " )";
                    tbPacking.Text = qtyCheck.ToString();
                    stock = qtyCheck;
                }

                tbPacking.RequestFocus();
                tbPacking.SelectAll();
            } 
        }

        public bool IsOnline()
        {
            var cm = (ConnectivityManager)GetSystemService(ConnectivityService);
            return cm.ActiveNetworkInfo == null ? false : cm.ActiveNetworkInfo.IsConnected;
        }

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetTheme(Resource.Style.AppTheme_NoActionBar);
            // Start the loader
            LoaderManifest.LoaderManifestLoopResources(this);
            if (settings.tablet)
            {
                RequestedOrientation = ScreenOrientation.Landscape;
                SetContentView(Resource.Layout.IssuedGoodsSerialOrSSCCEntryTablet);
                listData = FindViewById<ListView>(Resource.Id.listData);
                dataAdapter = UniversalAdapterHelper.GetIssuedGoodsSerialOrSSCCEntry(this, items);
                listData.Adapter = dataAdapter;
                imagePNG = FindViewById<ZoomageView>(Resource.Id.imagePNG);
                imagePNG.Visibility = ViewStates.Invisible;
            }
            else
            {
                RequestedOrientation = ScreenOrientation.Portrait;
                SetContentView(Resource.Layout.IssuedGoodsSerialOrSSCCEntry);
            }
            // Definitions
            AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
            var _customToolbar = new CustomToolbar(this, toolbar, Resource.Id.navIcon);
            _customToolbar.SetNavigationIcon(settings.RootURL + "/Services/Logo");
            SetSupportActionBar(_customToolbar._toolbar);
            SupportActionBar.SetDisplayShowTitleEnabled(false);
            tbIdent = FindViewById<EditText>(Resource.Id.tbIdent);
            Window.SetSoftInputMode(Android.Views.SoftInput.AdjustResize);
            tbIdent.Enabled = false;
            tbSSCC = FindViewById<EditText>(Resource.Id.tbSSCC);
            tbSerialNum = FindViewById<EditText>(Resource.Id.tbSerialNum);
            tbLocation = FindViewById<EditText>(Resource.Id.tbLocation);
            tbPacking = FindViewById<EditText>(Resource.Id.tbPacking);
            cbMultipleLocations = FindViewById<Spinner>(Resource.Id.cbMultipleLocations);
            tbIdent.InputType = Android.Text.InputTypes.ClassNumber;
            tbSSCC.InputType = Android.Text.InputTypes.ClassNumber;
            tbLocation.InputType = Android.Text.InputTypes.ClassText;
            lbQty = FindViewById<TextView>(Resource.Id.lbQty);
            soundPool = new SoundPool(10, Stream.Music, 0);
            soundPoolId = soundPool.Load(this, Resource.Raw.beep, 1);
            barcode2D = new Barcode2D();
            barcode2D.open(this, this);
            btCreateSame = FindViewById<Button>(Resource.Id.btCreateSame);
            btCreate = FindViewById<Button>(Resource.Id.btCreate);
            btFinish = FindViewById<Button>(Resource.Id.btFinish);
            btOverview = FindViewById<Button>(Resource.Id.btOverview);
            btExit = FindViewById<Button>(Resource.Id.btExit);

            if (CommonData.GetSetting("IssueSummaryView") == "1")
            {
                // If the company opted for this.
                cbMultipleLocations.ItemSelected += CbMultipleLocations_ItemSelected;
            }

            tbLocation.KeyPress += TbLocation_KeyPress;
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
            new IntentFilter(ConnectivityManager.ConnectivityAction));
            ssccRow = FindViewById<LinearLayout>(Resource.Id.sscc_row);
            serialRow = FindViewById<LinearLayout>(Resource.Id.serial_row);

            // Method calls

            CheckIfApplicationStopingException();

            // Color the fields that can be scanned
            ColorFields();

            // Stop the loader
            LoaderManifest.LoaderManifestLoopStop(this);
           
            SetUpProcessDependentButtons();
            // Main logic for the entry
            SetUpForm();

            if (settings.tablet)
            {
                fillItems();
            }
        }
        private bool initialDropdownEvent = true;

        private void CbMultipleLocations_ItemSelected(object? sender, AdapterView.ItemSelectedEventArgs e)
        {
           

            var selected = adapterLocation.GetItem(e.Position);

            if (selected != null)
            {

                tbLocation.Text = selected.Location;

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
                CheckData();
            }
            initialDropdownEvent = false;
        }

        protected override void OnDestroy()
        {
            // The problem seems to have been a memory leak. Unregister broadcast rgeceiver on activities where the scanning occurs. 21.05.2024 Janko Jovičić // 
            barcode2D.close(this);
            base.OnDestroy();
        }

        private void showPictureIdent(string ident)
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

   


        private void ImageClick(Drawable d)
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


        private async void fillItems()
        {
            var code = openIdent.GetString("Code");
            var wh = moveHead.GetString("Wharehouse");
            items = await AdapterStore.getStockForWarehouseAndIdent(code, wh);

        }


        private void SetUpProcessDependentButtons()
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

        private async void BtCreate_Click(object? sender, EventArgs e)
        {
            if (!isProccessOrderless)
            {
                CheckData();
            }

            if (!Base.Store.isUpdate)
            {
                double parsed;

                if (isProccessOrderless && double.TryParse(tbPacking.Text, out parsed) && stock >= parsed)
                {
                    var isCorrectLocation = IsLocationCorrect();

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
                                Analytics.TrackEvent(subjects.Error);
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
        private void CheckData()
        {          
            data = FilterIssuedGoods(connectedPositions, tbSSCC.Text, tbSerialNum.Text, tbLocation.Text);
            if(data.Count == 1)
            {
                createPositionAllowed = true;
            }
        }
        private async void BtCreateSame_Click(object? sender, EventArgs e)
        {
            if (!isProccessOrderless)
            {
                CheckData();
            }
            
            double parsed;
            if (isProccessOrderless && double.TryParse(tbPacking.Text, out parsed) && stock >= parsed)
            {
                var isCorrectLocation = IsLocationCorrect();

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
                var isCorrectLocation = IsLocationCorrect();

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
        private bool IsLocationCorrect()
        {
            string location = tbLocation.Text;

            if (!CommonData.IsValidLocation(moveHead.GetString("Wharehouse"), location))
            {
                return false;
            }
            else
            {
                return true;
            }
        }


        private void BtExit_Click(object? sender, EventArgs e)
        {
            StartActivity(typeof(MainMenu));
        }


        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
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


        private void BtFinish_Click(object? sender, EventArgs e)
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

        private void BtnNoConfirm_Click(object sender, EventArgs e)
        {
            popupDialogConfirm.Dismiss();
            popupDialogConfirm.Hide();
        }

        private async void BtnYesConfirm_Click(object sender, EventArgs e)
        {
            await FinishMethod();
        }

        private void BtOverview_Click(object? sender, EventArgs e)
        {
            StartActivity(typeof(IssuedGoodsEnteredPositionsView));
            Finish();
        }

        private void CheckIfApplicationStopingException()
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

        private void ColorFields()
        {
            tbSSCC.SetBackgroundColor(Android.Graphics.Color.Aqua);
            tbSerialNum.SetBackgroundColor(Android.Graphics.Color.Aqua);
            tbLocation.SetBackgroundColor(Android.Graphics.Color.Aqua);
        }

        private async Task CreateMethodFromStart()
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
                    moveItem.SetString("Location", tbLocation.Text.Trim());
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

        private async Task CreateMethodSame()
        {
  
                if (data.Count == 1 || isProccessOrderless)
                {
                    var element = new IssuedGoods();

                    if(!isProccessOrderless)
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
                    moveItem.SetString("Location", tbLocation.Text.Trim());
                    moveItem.SetString("Palette", "1");
                    string error;
                    moveItem = Services.SetObject("mi", moveItem, out error);

                    if (moveItem != null && error == string.Empty)
                    {

                        serialOverflowQuantity = Convert.ToDouble(tbPacking.Text.Trim());
                        stock -= serialOverflowQuantity;

               
                        lbQty.Text = $"{Resources.GetString(Resource.String.s83)} ( " + stock.ToString(CommonData.GetQtyPicture()) + " )";
                   

                        // Check to see if the maximum is already reached.
                        if(stock <= 0)
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
                            } else
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

        private void FilterData()
        {
            data = FilterIssuedGoods(connectedPositions, tbSSCC.Text, tbSerialNum.Text, tbLocation.Text);
            if (data.Count == 1)
            {
                var element = data.ElementAt(0);

                // This is perhaps not needed due to the quantity checking requirments. lbQty.Text = $"{Resources.GetString(Resource.String.s83)} ( " + element.anQty.ToString() + " )";
                if (element.anPackQty != -1 && element.anPackQty <= element.anQty)
                {
                    tbPacking.Text = element.anPackQty.ToString();
                    lbQty.Text = $"{Resources.GetString(Resource.String.s83)} ( " + quantity.ToString(CommonData.GetQtyPicture()) + " )";
                }
                else
                {
                    tbPacking.Text = element.anQty.ToString();
                    lbQty.Text = $"{Resources.GetString(Resource.String.s83)} ( " + quantity.ToString(CommonData.GetQtyPicture())  + " )";
                }
                if (serialRow.Visibility == ViewStates.Visible)
                {
                    tbSerialNum.Text = element.acSerialNo ?? string.Empty;
                }
                tbLocation.Text = element.aclocation;
                // Do stuff and allow creating the position
                createPositionAllowed = true;



                tbPacking.PostDelayed(() => {
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

        private async Task FinishMethod()
        {
            await Task.Run(async () =>
            {
                RunOnUiThread(() =>
                {
                    progress = new ProgressDialogClass();
                    progress.ShowDialogSync(this, $"{Resources.GetString(Resource.String.s262)}");
                });

                try
                {
                    var headID = moveHead.GetInt("HeadID");

                    string result;

                    if (WebApp.Get("mode=finish&stock=remove&print=" + Services.DeviceUser() + "&id=" + headID.ToString(), out result))
                    {
                        if (result.StartsWith("OK!"))
                        {
                            RunOnUiThread(() =>
                            {
                                progress.StopDialogSync();

                                var id = result.Split('+')[1];

                                Toast.MakeText(this, $"{Resources.GetString(Resource.String.s264)}" + id, ToastLength.Long).Show();

                                AlertDialog.Builder alert = new AlertDialog.Builder(this);

                                alert.SetTitle($"{Resources.GetString(Resource.String.s263)}");

                                alert.SetMessage($"{Resources.GetString(Resource.String.s264)}" + id);

                                alert.SetPositiveButton("Ok", (senderAlert, args) =>
                                {
                                    alert.Dispose();
                                    StartActivity(typeof(IssuedGoodsBusinessEventSetup));
                                });

                                Dialog dialog = alert.Create();
                                dialog.Show();
                            });
                        }
                        else
                        {
                            RunOnUiThread(() =>
                            {
                                progress.StopDialogSync();
                                AlertDialog.Builder alert = new AlertDialog.Builder(this);
                                alert.SetTitle($"{Resources.GetString(Resource.String.s265)}");
                                alert.SetMessage($"{Resources.GetString(Resource.String.s266)}" + result);

                                alert.SetPositiveButton("Ok", (senderAlert, args) =>
                                {
                                    alert.Dispose();
                                    StartActivity(typeof(MainMenu));
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
                finally
                {
                    RunOnUiThread(() =>
                    {
                        progress.StopDialogSync();
                    });
                }
            });
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
            connectedPositions.Clear();
            var sql = "SELECT * from uWMSOrderItemByKeyOut WHERE acKey = @acKey AND anNo = @anNo AND acIdent = @acIdent";
            var parameters = new List<Services.Parameter>();
            parameters.Add(new Services.Parameter { Name = "acKey", Type = "String", Value = acKey });
            parameters.Add(new Services.Parameter { Name = "anNo", Type = "Int32", Value = anNo });
            parameters.Add(new Services.Parameter { Name = "acIdent", Type = "String", Value = acIdent });

            var subjects = await AsyncServices.AsyncServices.GetObjectListBySqlAsync(sql, parameters);
            if (!subjects.Success)
            {
                RunOnUiThread(() =>
                {
                    Analytics.TrackEvent(subjects.Error);
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
                            anNo = (int) (row.IntValue("anNo") ?? -1),
                            acKey = row.StringValue("acKey"),    
                            acIdent = row.StringValue("acIdent"),
                            anPackQty = row.DoubleValue("anPackQty") ?? -1
                        });
                    }
                }
            }
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

        private async void SetUpForm()
        {
            if(settings.tablet)
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
                tbLocation.Text = moveItem.GetString("Location");
                tbPacking.Text = moveItem.GetDouble("Qty").ToString();
                lbQty.Text = $"{Resources.GetString(Resource.String.s83)} ( " + moveItem.GetDouble("Qty").ToString() + " )";
                btCreateSame.Text = $"{Resources.GetString(Resource.String.s293)}";
                // Lock down all other fields
                tbIdent.Enabled = false;
                tbSerialNum.Enabled = false;
                tbSSCC.Enabled = false;
                tbLocation.Enabled = false;
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
                    tbLocation.RequestFocus();
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


                        if (CommonData.GetSetting("IssueSummaryView") == "1")
                        {
                            cbMultipleLocations.Visibility = ViewStates.Visible;
                            adapterLocations = await GetStockState(receivedTrail.Ident);
                            adapterLocation = new ArrayAdapter<MultipleStock>(this,
                            Android.Resource.Layout.SimpleSpinnerItem, adapterLocations);
                            adapterLocation.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
                            cbMultipleLocations.Adapter = adapterLocation;
                        }


                        tbLocation.Text = receivedTrail.Location;

                        if (receivedTrail.Packaging != -1 && Double.TryParse(receivedTrail.Qty, out double qty) && receivedTrail.Packaging <= qty)
                        {
                            packaging = receivedTrail.Packaging;
                            quantity = Double.Parse(receivedTrail.Qty);                   
                            lbQty.Text = $"{Resources.GetString(Resource.String.s83)} ( " + quantity.ToString(CommonData.GetQtyPicture()) + " )";
                            stock = quantity;
                            tbPacking.Text = packaging.ToString();
                        } else
                        {
                            packaging = receivedTrail.Packaging;
                            quantity = Double.Parse(receivedTrail.Qty);
                            lbQty.Text = $"{Resources.GetString(Resource.String.s83)} ( " + quantity.ToString(CommonData.GetQtyPicture()) + " )";
                            stock = quantity;
                            tbPacking.Text = quantity.ToString();
                        }                   
                        
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
                            lbQty.Text = $"{Resources.GetString(Resource.String.s83)} ( " + qtyCheck.ToString(CommonData.GetQtyPicture()) + " )";
                            tbPacking.Text = qtyCheck.ToString();
                            stock = qtyCheck;

                        }

                        await GetConnectedPositions(code2d.__helper__convertedOrder, code2d.__helper__position, code2d.ident);

                        // Reset the 2d code to nothing
                        Base.Store.code2D = null;

                        tbPacking.RequestFocus();
                        tbPacking.SelectAll();

                        FilterData();
                    }
                    else if (Base.Store.modeIssuing == 1)
                    {
                        // This flow is for idents.

                        var order = Base.Store.OpenOrder;
                        
                        if (order != null)
                        {
                            if (order.Packaging != -1)
                            {
                                quantity = order.Quantity ?? 0;
                                lbQty.Text = $"{Resources.GetString(Resource.String.s83)} ( " + quantity.ToString(CommonData.GetQtyPicture()) + " )";
                                stock = quantity;
                            }
                            else
                            {
                                quantity = order.Quantity ?? 0;
                                lbQty.Text = $"{Resources.GetString(Resource.String.s83)} ( " + quantity.ToString(CommonData.GetQtyPicture()) + " )";
                                stock = quantity;
                            }

                            await GetConnectedPositions(order.Order, order.Position ?? -1, order.Ident);


                            if (CommonData.GetSetting("IssueSummaryView") == "1")
                            {
                                cbMultipleLocations.Visibility = ViewStates.Visible;
                                adapterLocations = await GetStockState(order.Ident);
                                adapterLocation = new ArrayAdapter<MultipleStock>(this,
                                Android.Resource.Layout.SimpleSpinnerItem, adapterLocations);
                                adapterLocation.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
                                cbMultipleLocations.Adapter = adapterLocation;
                            }
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

        private async Task<List<MultipleStock>> GetStockState(String ident)
        {
            List<MultipleStock> data = new List<MultipleStock>();

            var sql = "SELECT * FROM uWMSStockByWarehouse WHERE acIdent = @acIdent AND acWarehouse = @acWarehouse;";
            var parameters = new List<Services.Parameter>();

            parameters.Add(new Services.Parameter { Name = "acWarehouse", Type = "String", Value = moveHead.GetString("Wharehouse") });
            parameters.Add(new Services.Parameter { Name = "acIdent", Type = "String", Value = ident });
            
            var stocks = await AsyncServices.AsyncServices.GetObjectListBySqlAsync(sql, parameters);

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

                        } else if (ssccRow.Visibility == ViewStates.Gone && serialRow.Visibility == ViewStates.Visible)
                        {
                            type = Showing.Serial;
                        } else
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

        private void Sound()
        {
            soundPool.Play(soundPoolId, 1, 1, 0, 0, 1);
        }

        private void TbSerialNum_KeyPress(object? sender, View.KeyEventArgs e)
        {
            if (e.KeyCode == Keycode.Enter && e.Event.Action == KeyEventActions.Down)
            {
                FilterData();
            }
            else
            {
                e.Handled = false;
            }
        }

        private void TbSSCC_KeyPress(object? sender, View.KeyEventArgs e)
        {
            if (e.KeyCode == Keycode.Enter && e.Event.Action == KeyEventActions.Down)
            {
                FilterData();
            }
            else
            {
                e.Handled = false;
            }
        }


        private void TbLocation_KeyPress(object? sender, View.KeyEventArgs e)
        {
            if (e.KeyCode == Keycode.Enter && e.Event.Action == KeyEventActions.Down)
            {
                if (isProccessOrderless)
                {
                    var isCorrectLocation = IsLocationCorrect();

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
            } else
            {
                e.Handled = false;
            }

            FilterData();
        }

    }
}