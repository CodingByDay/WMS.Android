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
using static Android.App.ActionBar;
using AlertDialog = Android.App.AlertDialog;
using WebApp = TrendNET.WMS.Device.Services.WebApp;

namespace WMS
{
    [Activity(Label = "InterWarehouseSerialOrSSCCEntry", ScreenOrientation = ScreenOrientation.Portrait)]
    public class InterWarehouseSerialOrSSCCEntry : CustomBaseActivity, IBarcodeResult
    {
        private EditText? tbIdent;
        private EditText? tbSSCC;
        private EditText? tbSerialNum;
        private EditText? tbIssueLocation;
        private EditText? tbLocation;
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

        protected override void OnCreate(Bundle savedInstanceState)
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
            tbIssueLocation = FindViewById<EditText>(Resource.Id.tbIssueLocation);
            tbLocation = FindViewById<EditText>(Resource.Id.tbLocation);
            tbPacking = FindViewById<EditText>(Resource.Id.tbPacking);
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
            tbIssueLocation.KeyPress += TbIssueLocation_KeyPress;
            tbLocation.KeyPress += TbLocation_KeyPress;
            tbPacking.FocusChange += TbPacking_FocusChange;
            // Method calls

            CheckIfApplicationStopingException();

            // Color the fields that can be scanned
            ColorFields();

            SetUpProcessDependentButtons();

            // Main logic for the entry
            SetUpForm();
        }

        private async void TbPacking_FocusChange(object? sender, View.FocusChangeEventArgs e)
        {
            if (e.HasFocus)
            {
                await LoadStock(tbIssueLocation.Text, tbIdent.Text, moveHead.GetString("Issuer"), tbSSCC.Text, tbSerialNum.Text);
            }
        }

        private void ImageClick(Drawable d)
        {
            popupDialog = new Dialog(this);
            popupDialog.SetContentView(Resource.Layout.WarehousePicture);
            popupDialog.Window.SetSoftInputMode(SoftInput.AdjustResize);
            popupDialog.Show();
            popupDialog.KeyPress += PopupDialog_KeyPress;
            popupDialog.Window.SetLayout(LayoutParams.MatchParent, LayoutParams.WrapContent);
            popupDialog.Window.SetBackgroundDrawableResource(Android.Resource.Color.HoloBlueBright);
            image = popupDialog.FindViewById<ZoomageView>(Resource.Id.image);
            image.SetMinimumHeight(500);
            image.SetMinimumWidth(800);
            image.SetImageDrawable(d);

        }
        private void PopupDialog_KeyPress(object sender, DialogKeyEventArgs e)
        {
            if (e.KeyCode == Keycode.Back)
            {
                popupDialog.Dismiss();
                popupDialog.Hide();
                popupDialog.Window.Dispose();
            }
        }

        private void Fill(List<LocationClass> list)
        {
            foreach (var obj in list)
            {
                items.Add(new LocationClass { ident = obj.ident, location = obj.location, quantity = obj.quantity });

            }
        }

        private void ListData_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            selected = e.Position;
            var item = items.ElementAt(selected);
        }
        private async Task FillTheIdentLocationList(string ident)
        {
            var wh = moveHead.GetString("Receiver");
            var list = await AdapterStore.getStockForWarehouseAndIdent(wh, ident);
            Fill(list);
        }

        private async void TbLocation_KeyPress(object? sender, View.KeyEventArgs e)
        {
            e.Handled = false;
        }

        private void TbIssueLocation_KeyPress(object? sender, View.KeyEventArgs e)
        {
            e.Handled = false;
        }

        private void TbSerialNum_KeyPress(object? sender, View.KeyEventArgs e)
        {
            e.Handled = false;
        }

        private async void TbSSCC_KeyPress(object? sender, View.KeyEventArgs e)
        {
            if (e.KeyCode == Keycode.Enter && e.Event.Action == KeyEventActions.Down)
            {
                await FillDataBySSCC(tbSSCC.Text);
            }
            e.Handled = false;
        }

        private async void TbIdent_KeyPress(object? sender, View.KeyEventArgs e)
        {
            if (e.KeyCode == Keycode.Enter && e.Event.Action == KeyEventActions.Down)
            {
                await ProcessIdent(false);
            }
            e.Handled = false;
        }

        private void BtExit_Click(object? sender, EventArgs e)
        {
            StartActivity(typeof(MainMenu));
            Finish();
        }

        private void BtOverview_Click(object? sender, EventArgs e)
        {
            StartActivity(typeof(InterWarehouseEnteredPositionsView));
            Finish();
        }

        private void BtFinish_Click(object? sender, EventArgs e)
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


        private void BtnNoConfirm_Click(object sender, EventArgs e)
        {
            popupDialogConfirm.Dismiss();
            popupDialogConfirm.Hide();
        }

        private async void BtnYesConfirm_Click(object sender, EventArgs e)
        {
            try
            {
                LoaderManifest.LoaderManifestLoopResources(this);
                await FinishMethod();
            } catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
            } finally
            {
                LoaderManifest.LoaderManifestLoopStop(this);
            }
        }



        private async Task FinishMethod()
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
                catch(Exception ex) 
                {
                    SentrySdk.CaptureException(ex);
                }
            });
        }


        private async void BtCreate_Click(object? sender, EventArgs e)
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
            } catch(Exception ex)
            {
                SentrySdk.CaptureException(ex);
            } finally
            {
                LoaderManifest.LoaderManifestLoopStop(this);
            }
        }

        private async Task LoadStock(string location, string ident, string warehouse, string sscc = null, string serial = null)
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


        private async Task <bool> IsLocationCorrect()
        {
            string location = tbLocation.Text;

            if (await CommonData.IsValidLocationAsync(moveHead.GetString("Issuer"), location, this) && await CommonData.IsValidLocationAsync(moveHead.GetString("Receiver"), location, this))
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        private async void BtSaveOrUpdate_Click(object? sender, EventArgs e)
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
            } catch(Exception ex)
            {
                SentrySdk.CaptureException(ex);
            } finally
            {
                LoaderManifest.LoaderManifestLoopStop(this);
            }
        }

        private void CheckIfApplicationStopingException()
        {
            if (moveItem == null && moveHead == null)
            {
                // Destroy the activity
                Finish();
                StartActivity(typeof(MainMenu));
            }
        }


        private async Task CreateMethodFromStart()
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
                moveItem.SetString("Location", tbLocation.Text.Trim());
                moveItem.SetString("IssueLocation", tbIssueLocation.Text.Trim());
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




        private async void SetUpForm()
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
                tbIssueLocation.Text = moveItem.GetString("IssueLocation");
                tbLocation.Text = moveItem.GetString("Location");
                lbQty.Text = $"{Resources.GetString(Resource.String.s83)} ( " + moveItem.GetDouble("Qty").ToString() + " )";
                tbIdent.Enabled = false;
                tbSerialNum.Enabled = false;
                tbSSCC.Enabled = false;
                tbIssueLocation.Enabled = false;
                tbLocation.Enabled = false;
                stock = moveItem.GetDouble("Qty");
                tbPacking.RequestFocus();
                tbPacking.SelectAll();
            }

        }



        public async void GetBarcode(string barcode)
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
                    tbIssueLocation.RequestFocus();
                }
                else if (tbIssueLocation.HasFocus)
                {

                    tbIssueLocation.Text = barcode;
                    await LoadStock(tbIssueLocation.Text, tbIdent.Text, moveHead.GetString("Issuer"), tbSSCC.Text, tbSerialNum.Text);
                    tbLocation.RequestFocus();
                }
                else if (tbLocation.HasFocus)
                {

                    tbLocation.Text = barcode;
                    tbPacking.RequestFocus();
                }
            }

        }

        private async Task FillDataBySSCC(string sscc)
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
                    tbIssueLocation.Text = ssccResult.Rows[0].StringValue("aclocation");
                    tbSerialNum.Text = ssccResult.Rows[0].StringValue("acSerialNo");
                    tbSSCC.Text = ssccResult.Rows[0].StringValue("acSSCC").ToString();
                    lbQty.Text = $"{Resources.GetString(Resource.String.s83)} ( " + ssccResult.Rows[0].DoubleValue("anQty").ToString() + " )";
                    tbPacking.Text = ssccResult.Rows[0].DoubleValue("anQty").ToString();
                    stock = ssccResult.Rows[0].DoubleValue("anQty");
                    tbPacking.RequestFocus();
                    tbPacking.SelectAll();
                }
                else
                {
                    Toast.MakeText(this, Resources.GetString(Resource.String.s337), ToastLength.Long).Show();
                }
            });
        }


        private async Task ProcessIdent(bool update)
        {
            activityIdent = await CommonData.LoadIdentAsync(tbIdent.Text.Trim(), this);

            if (activityIdent != null)
            {
                if (!activityIdent.GetBool("isSSCC"))
                {
                    ssccRow.Visibility = ViewStates.Gone;
                }

                if (!activityIdent.GetBool("HasSerialNumber"))
                {
                    serialRow.Visibility = ViewStates.Gone;
                }

                if (activityIdent == null)
                {
                    tbIdent.Text = "";
                    lbIdentName.Text = "";
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
                            tbLocation.Text = recommededLocation.GetString("Location");
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
                    tbSSCC.Enabled = activityIdent.GetBool("isSSCC");
                    tbSerialNum.Enabled = activityIdent.GetBool("HasSerialNumber");
                }
                else
                {
                    lbIdentName.Enabled = false;
                }

                await FillTheIdentLocationList(activityIdent.GetString("Code"));


            }

        }


        private void ColorFields()
        {
            tbIdent.SetBackgroundColor(Android.Graphics.Color.Aqua);
            tbSSCC.SetBackgroundColor(Android.Graphics.Color.Aqua);
            tbSerialNum.SetBackgroundColor(Android.Graphics.Color.Aqua);
            tbLocation.SetBackgroundColor(Android.Graphics.Color.Aqua);
            tbIssueLocation.SetBackgroundColor(Android.Graphics.Color.Aqua);
        }


        private void SetUpProcessDependentButtons()
        {
            // This method changes the UI so it shows in a visible way that it is the update screen. - 18.03.2024
            if (Base.Store.isUpdate)
            {
                btSaveOrUpdate.Visibility = ViewStates.Gone;
                btCreate.Text = $"{Resources.GetString(Resource.String.s290)}";
            }

        }


        private void OnNetworkStatusChanged(object? sender, EventArgs e)
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


        public bool IsOnline()
        {
            var cm = (ConnectivityManager)GetSystemService(ConnectivityService);
            return cm.ActiveNetworkInfo == null ? false : cm.ActiveNetworkInfo.IsConnected;
        }

    }
}