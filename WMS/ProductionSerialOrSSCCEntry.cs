﻿using Android.Content;
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
        private bool editMode = false;
        private EditText tbIdent;
        private EditText tbSSCC;
        private EditText tbSerialNum;
        private EditText tbPacking;
        private TextView lbQty;
        private Button btSaveOrUpdate;
        private Button button3;
        private Button button4;
        private Button button5;
        SoundPool soundPool;
        int soundPoolId;
        private ListView listData;
        private UniversalAdapter<ProductionSerialOrSSCCList> dataAdapter;
        private ImageView imagePNG;

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
                        if (button3.Enabled == true)
                        {
                            Button3_Click(this, null);
                        }
                        break;

                    case Keycode.F4:
                        if (button4.Enabled == true)
                        {
                            Button4_Click(this, null);
                        }
                        break;

                    case Keycode.F8:
                        if (button5.Enabled == true)
                        {
                            Button5_Click(this, null);
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

        private void fillItems()
        {
            try
            {
                string error;
                var stock = Services.GetObjectList("str", out error, moveHead.GetString("Wharehouse") + "||" + identCode); /* Defined at the beggining of the activity. */
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

        private async Task GetWorkOrderDefaultQty()
        {
            try
            {
                if (getWorkOrderDefaultQty == null)
                {

                    try
                    {
                        string error;
                        var useObj = Services.GetObject("wodqUse", "", out error);
                        getWorkOrderDefaultQty = useObj == null ? false : useObj.GetBool("Use");
                    }
                    catch (Exception err)
                    {

                        SentrySdk.CaptureException(err);
                        return;

                    }
                }

                if ((bool)getWorkOrderDefaultQty)
                {

                    try
                    {
                        string error;
                        var qtyObj = Services.GetObject("wodq", openWorkOrder.GetString("Key") + "|" + openWorkOrder.GetString("Ident"), out error);
                        if (qtyObj != null)
                        {
                            var qty = qtyObj.GetDouble("DefaultQty");
                            if (qty < 0)
                            {
                                getWorkOrderDefaultQty = false;
                            }
                            else
                            {
                                tbPacking.Text = qty.ToString(await CommonData.GetQtyPictureAsync(this));
                            }
                        }
                    }
                    catch (Exception err)
                    {

                        SentrySdk.CaptureException(err);
                        return;

                    }
                }
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private async Task<bool> SaveMoveItem()
        {
            try
            {

                if (string.IsNullOrEmpty(tbPacking.Text.Trim()))
                {
                    return true;
                }



                if (tbSSCC.Enabled && string.IsNullOrEmpty(tbSSCC.Text.Trim()))
                {
                    RunOnUiThread(() =>
                    {
                        string SuccessMessage = string.Format($"{Resources.GetString(Resource.String.s254)}");
                        DialogHelper.ShowDialogError(this, this, SuccessMessage);

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
                            string SuccessMessage = string.Format($"{Resources.GetString(Resource.String.s314)}");
                            DialogHelper.ShowDialogError(this, this, SuccessMessage);
                            tbSerialNum.RequestFocus();
                        });

                        return false;
                    }
                }

                if (!await CommonData.IsValidLocationAsync(moveHead.GetString("Wharehouse"), searchableSpinnerLocation.spinnerTextValueField.Text.Trim(), this))
                {
                    RunOnUiThread(() =>
                    {
                        string SuccessMessage = string.Format($"{Resources.GetString(Resource.String.s258)} '" + searchableSpinnerLocation.spinnerTextValueField.Text.Trim() + $"' {Resources.GetString(Resource.String.s272)} '" + moveHead.GetString("Wharehouse") + "'!");
                        DialogHelper.ShowDialogError(this, this, SuccessMessage);
                        searchableSpinnerLocation.spinnerTextValueField.RequestFocus();
                    });

                    return false;
                }

                string error;
                try
                {


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

                            if (CheckWorkOrderOpenQty())
                            {
                                var max = Math.Abs(openWorkOrder.GetDouble("OpenQty"));
                                if (Math.Abs(qty) > max)
                                {
                                    var picture = await CommonData.GetQtyPictureAsync(this);
                                    string SuccessMessage = string.Format($"{Resources.GetString(Resource.String.s40)} (" + qty.ToString(picture) + ") ne sme presegati max. količine (" + max.ToString(await CommonData.GetQtyPictureAsync(this)) + ")!");

                                    RunOnUiThread(() =>
                                    {
                                        DialogHelper.ShowDialogError(this, this, SuccessMessage);
                                        tbPacking.RequestFocus();
                                    });

                                    return false;
                                }
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
                }
                catch (Exception err)
                {

                    SentrySdk.CaptureException(err);
                    return false;

                }
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
                return false;
            }
        }

        private async Task fillSugestedLocation(string warehouse)
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
                await GetWorkOrderDefaultQty();
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }
        private bool CheckWorkOrderOpenQty()
        {
            try
            {
                if (checkWorkOrderOpenQty == null)
                {
                    try
                    {
                        string error;
                        var useObj = Services.GetObject("cwooqUse", "", out error);
                        checkWorkOrderOpenQty = useObj == null ? false : useObj.GetBool("Use");
                    }
                    catch (Exception err)
                    {

                        SentrySdk.CaptureException(err);
                        return false;

                    }
                }
                return (bool)checkWorkOrderOpenQty;
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
                return false;
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
                lbQty = FindViewById<TextView>(Resource.Id.lbQty);
                btSaveOrUpdate = FindViewById<Button>(Resource.Id.btSaveOrUpdate);
                button3 = FindViewById<Button>(Resource.Id.button3);
                button4 = FindViewById<Button>(Resource.Id.button4);
                button5 = FindViewById<Button>(Resource.Id.button5);
                tbIdent.InputType = Android.Text.InputTypes.ClassNumber;
                tbSSCC.InputType = Android.Text.InputTypes.ClassNumber;
                tbSerialNum.InputType = Android.Text.InputTypes.ClassNumber;

                searchableSpinnerLocation = FindViewById<SearchableSpinner>(Resource.Id.searchableSpinnerLocation);
                var locations = await HelperMethods.GetLocationsForGivenWarehouse(moveHead.GetString("Wharehouse"));
                searchableSpinnerLocation.SetItems(locations);
                searchableSpinnerLocation.ColorTheRepresentation(1);
                searchableSpinnerLocation.ShowDropDown();

                color();
                tbSSCC.RequestFocus();
                btSaveOrUpdate.Click += BtSaveOrUpdate_Click;
                button3.Click += Button3_Click;
                button4.Click += Button4_Click;
                button5.Click += Button5_Click;
                tbSSCC.FocusChange += TbSSCC_FocusChange;
                barcode2D = new Barcode2D(this, this);
                try
                {

                    var key = moveHead.GetString("LinkKey");
                    string error;
                    openWorkOrder = Services.GetObject("wo", key, out error);
                    if (openWorkOrder == null)
                    {
                        StartActivity(typeof(MainMenu));
                    }
                    lbQty.Text = $"{Resources.GetString(Resource.String.s40)} (" + openWorkOrder.GetDouble("OpenQty").ToString(await CommonData.GetQtyPictureAsync(this)) + ")";
                }
                catch (Exception err)
                {
                    SentrySdk.CaptureException(err);
                }

                var ident = await CommonData.LoadIdentAsync(openWorkOrder.GetString("Ident"), this);

                showPictureIdent(ident.GetString("Code"));
                identCode = ident.GetString("Code");
                tbIdent.Text = ident.GetString("Code") + " " + ident.GetString("Name");
                tbSSCC.Enabled = ident.GetBool("isSSCC");
                tbSerialNum.Enabled = ident.GetBool("HasSerialNumber");
                editMode = moveItem != null;

                if (editMode)
                {

                    tbSSCC.Text = moveItem.GetString("SSCC");

                    tbSerialNum.Text = moveItem.GetString("SerialNo");

                    tbPacking.Text = moveItem.GetDouble("Packing").ToString(await CommonData.GetQtyPictureAsync(this));


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

                }


                if (tbSSCC.Enabled && (await CommonData.GetSettingAsync("AutoCreateSSCCProduction", this) == "1"))
                {

                    tbSSCC.Text = await CommonData.GetNextSSCCAsync(this);
                    tbPacking.RequestFocus();

                }
                ProcessSerialNum();
                var _broadcastReceiver = new NetworkStatusBroadcastReceiver();
                _broadcastReceiver.ConnectionStatusChanged += OnNetworkStatusChanged;
                Application.Context.RegisterReceiver(_broadcastReceiver,
                new IntentFilter(ConnectivityManager.ConnectivityAction), ReceiverFlags.NotExported);
                if (App.Settings.tablet)
                {
                    fillItems();
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
                var warehouse = moveHead.GetString("Wharehouse");

                await fillSugestedLocation(warehouse);
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
                tbSSCC.SetBackgroundColor(Android.Graphics.Color.Aqua);
                tbSerialNum.SetBackgroundColor(Android.Graphics.Color.Aqua);
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }


        private void Button5_Click(object sender, EventArgs e)
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
                    bool resultAsync = false;

                    RunOnUiThread(() =>
                    {
                        resultAsync = SaveMoveItem().Result;
                    });

                    if (resultAsync)
                    {
                        var headID = moveHead.GetInt("HeadID");

                        await SelectSubjectBeforeFinish.ShowIfNeeded(headID);

                        try
                        {

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
                                    AlertDialog.Builder alert = new AlertDialog.Builder(this);
                                    alert.SetTitle($"{Resources.GetString(Resource.String.s265)}");
                                    alert.SetMessage($"{Resources.GetString(Resource.String.s218)}" + result);

                                    alert.SetPositiveButton("Ok", (senderAlert, args) =>
                                    {
                                        alert.Dispose();

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
        private void Button4_Click(object sender, EventArgs e)
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

        private void Button3_Click(object sender, EventArgs e)
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

        private void BtSaveOrUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                try
                {
                    LoaderManifest.LoaderManifestLoopResources(this);
                    if (SaveMoveItem().Result)
                    {
                        if (editMode)
                        {
                            StartActivity(typeof(ProductionEnteredPositionsView));
                            Finish();
                        }
                        else
                        {
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
    }
}