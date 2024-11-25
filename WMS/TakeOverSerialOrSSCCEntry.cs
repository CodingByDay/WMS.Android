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
    public class TakeOverSerialOrSSCCEntry : CustomBaseActivity, IBarcodeResult
    {
        private NameValueObject openIdent = (NameValueObject)InUseObjects.Get("OpenIdent");
        private NameValueObject openOrder = (NameValueObject)InUseObjects.Get("OpenOrder");
        private NameValueObject moveHead = (NameValueObject)InUseObjects.Get("MoveHead");
        private NameValueObject moveItem = (NameValueObject)InUseObjects.Get("MoveItem");
        private NameValueObjectList docTypes = null;
        private bool editMode = false;
        private bool isPackaging = false;
        private EditText tbIdent;
        private EditText tbSSCC;
        private EditText tbSerialNum;
        private EditText tbPacking;
        private Button btSaveOrUpdate;
        private Button btCreate;
        private Button btFinish;
        private Button btOverview;
        private Button btBack;
        private TextView lbQty;
        private TextView lbUnits;
        private List<string> locations = new List<string>();
        SoundPool soundPool;
        int soundPoolId;
        private BarCode2D_Receiver.Barcode2D barcode2D;
        private LinearLayout? ssccRow;
        private LinearLayout? serialRow;
        private Trail? receivedTrail;
        private double qtyCheck;
        private double stock;
        private List<Takeover> connectedPositions = new List<Takeover>();
        private double serialOverflowQuantity = 0;
        private Dialog popupDialogConfirm;
        private Button? btnYesConfirm;
        private Button? btnNoConfirm;
        private ListView listData;
        private UniversalAdapter<TakeoverDocument> dataAdapter;
        private List<TakeoverDocument> items = new List<TakeoverDocument>();
        private NameValueObjectList positions;
        private string tempUnit;
        private List<TakeoverDocument> data = new List<TakeoverDocument>();
        private double packaging;
        private double quantity;
        private ZoomageView warehouseImage;
        private Dialog popupDialog;
        private ZoomageView? image;
        private SearchableSpinner searchableSpinnerLocation;

        protected async override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                SetTheme(Resource.Style.AppTheme_NoActionBar);

                if (App.Settings.tablet)
                {
                    base.RequestedOrientation = ScreenOrientation.Landscape;
                    base.SetContentView(Resource.Layout.TakeOverSerialOrSSCCEntryTablet);
                    warehouseImage = FindViewById<ZoomageView>(Resource.Id.warehousePNG);
                    warehouseImage.Visibility = ViewStates.Invisible;
                    listData = FindViewById<ListView>(Resource.Id.listData);
                    dataAdapter = UniversalAdapterHelper.GetTakeoverSerialOrSSCCEntry(this, data);
                    listData.Adapter = dataAdapter;
                    ShowPictureIdent(openIdent.GetString("Code"));
                }
                else
                {
                    base.RequestedOrientation = ScreenOrientation.Portrait;
                    base.SetContentView(Resource.Layout.TakeOverSerialOrSSCCEntry);
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
                tbIdent.InputType = Android.Text.InputTypes.ClassNumber;
                tbSSCC.InputType = Android.Text.InputTypes.ClassNumber;
                btSaveOrUpdate = FindViewById<Button>(Resource.Id.btSaveOrUpdate);
                btCreate = FindViewById<Button>(Resource.Id.btCreate);
                btFinish = FindViewById<Button>(Resource.Id.btFinish);
                btOverview = FindViewById<Button>(Resource.Id.btOverview);
                btBack = FindViewById<Button>(Resource.Id.btBack);
                lbQty = FindViewById<TextView>(Resource.Id.lbQty);
                lbUnits = FindViewById<TextView>(Resource.Id.lbUnits);
                Window.SetSoftInputMode(Android.Views.SoftInput.AdjustResize);
                barcode2D = new Barcode2D(this, this);
                ssccRow = FindViewById<LinearLayout>(Resource.Id.sscc_row);
                serialRow = FindViewById<LinearLayout>(Resource.Id.serial_row);
                btSaveOrUpdate.Click += BtSaveOrUpdate_Click;
                btCreate.Click += BtCreate_Click;
                btFinish.Click += BtFinish_Click;
                btOverview.Click += BtOverview_Click;
                btBack.Click += BtBack_Click;


                searchableSpinnerLocation = FindViewById<SearchableSpinner>(Resource.Id.searchableSpinnerLocation);

                var locations = await HelperMethods.GetLocationsForGivenWarehouse(moveHead.GetString("Wharehouse"));

                searchableSpinnerLocation.SetItems(locations);
                searchableSpinnerLocation.ColorTheRepresentation(1);
                searchableSpinnerLocation.ShowDropDown();
                // Method calls

                CheckIfApplicationStopingException();

                // Color the fields that can be scanned
                ColorFields();

                // Stop the loader

                SetUpProcessDependentButtons();

                // Main logic for the entry
                await SetUpForm();


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


        private void ShowPictureIdent(string ident)
        {
            try
            {
                Android.Graphics.Bitmap show = Services.GetImageFromServerIdent(moveHead.GetString("Wharehouse"), ident);
                Drawable d = new BitmapDrawable(Resources, show);
                warehouseImage.SetImageDrawable(d);
                warehouseImage.Visibility = ViewStates.Visible;
                warehouseImage.Click += (e, ev) => { ImageClick(d); };
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

                }
            } catch (Exception ex)
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
                popupDialog.KeyPress += PopupDialog_KeyPress;
                popupDialog.Window.SetLayout(LayoutParams.MatchParent, LayoutParams.WrapContent);
                popupDialog.Window.SetBackgroundDrawableResource(Android.Resource.Color.HoloBlueBright);
                image = popupDialog.FindViewById<ZoomageView>(Resource.Id.image);
                image.SetMinimumHeight(500);
                image.SetMinimumWidth(800);
                image.SetImageDrawable(d);
                // Access Pop up layout fields like below
            } catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }


        private async Task FillListAdapter()
        {
            try
            {
                for (int i = 0; i < positions.Items.Count; i++)
                {
                    if (i < positions.Items.Count && positions.Items.Count > 0)
                    {

                        var item = positions.Items.ElementAt(i);
                        var created = item.GetDateTime("DateInserted");
                        var numbering = i + 1;
                        bool setting;

                        if (await CommonData.GetSettingAsync("ShowNumberOfUnitsField", this) == "1")
                        {
                            setting = false;
                        }
                        else
                        {
                            setting = true;
                        }
                        if (setting)
                        {
                            tempUnit = item.GetDouble("Qty").ToString();
                        }
                        else
                        {
                            tempUnit = item.GetDouble("Factor").ToString();
                        }
                        string error;
                        var ident = item.GetString("Ident").Trim();
                        var openIdent = Services.GetObject("id", ident, out error);
                        //  var ident = CommonData.LoadIdent(item.GetString("Ident"));
                        var identName = openIdent.GetString("Name");
                        var date = created == null ? "" : ((DateTime)created).ToString("dd.MM.yyyy");
                        data.Add(new TakeoverDocument
                        {
                            ident = item.GetString("Ident"),
                            serial = item.GetString("SerialNo"),
                            sscc = item.GetString("SSCC"),
                            location = item.GetString("Location"),
                            quantity = tempUnit,
                        });

                                             
                    }
                    else
                    {
                        // UI changes.
                        RunOnUiThread(() =>
                        {
                            string errorWebApp = string.Format($"{Resources.GetString(Resource.String.s247)}");
                            Toast.MakeText(this, errorWebApp, ToastLength.Long).Show();
                        });
                    }
                }

                RunOnUiThread(() =>
                {
                    dataAdapter.NotifyDataSetChanged();
                });
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }


        private async Task FillTheList()
        {
            try
            {
                try
                {

                    positions = await AsyncServices.AsyncServices.GetObjectListAsync("mi", moveHead.GetInt("HeadID").ToString(), this);
                    InUseObjects.Set("TakeOverEnteredPositions", positions);

                    if (positions == null)
                    {
                        // UI changes.
                        RunOnUiThread(() =>
                        {
                            Toast.MakeText(this, $"{Resources.GetString(Resource.String.s213)}", ToastLength.Long).Show();
                        });

                        return;
                    }

                }
                finally
                {
                    await FillListAdapter();
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
                        btSaveOrUpdate.Visibility = ViewStates.Gone;
                        btCreate.Text = $"{Resources.GetString(Resource.String.s290)}";
                    }
                    else if (Base.Store.code2D != null)
                    {
                        btSaveOrUpdate.Visibility = ViewStates.Gone;
                        // 2d code reading process.
                    }
                });
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
                // UI changes.
                RunOnUiThread(() =>
                {
                    tbSSCC.SetBackgroundColor(Android.Graphics.Color.Aqua);
                    tbSerialNum.SetBackgroundColor(Android.Graphics.Color.Aqua);
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

                if (Base.Store.isUpdate)
                {
                    // Update logic ?? it seems to be true.
                    tbIdent.Text = moveItem.GetString("IdentName");
                    tbSerialNum.Text = moveItem.GetString("SerialNo");
                    tbSSCC.Text = moveItem.GetString("SSCC");
                    searchableSpinnerLocation.spinnerTextValueField.Text = moveItem.GetString("Location");
                    tbPacking.Text = moveItem.GetDouble("Qty").ToString();
                    lbQty.Text = $"{Resources.GetString(Resource.String.s83)} \n ( " + moveItem.GetDouble("Qty").ToString() + " )";
                    btCreate.Text = $"{Resources.GetString(Resource.String.s290)}";
                    // Lock down all other fields
                    tbIdent.Enabled = false;
                    tbSerialNum.Enabled = false;
                    tbSSCC.Enabled = false;
                    searchableSpinnerLocation.spinnerTextValueField.Enabled = false;
                    tbPacking.RequestFocus();
                }
                else
                {
                    tbIdent.Text = openIdent.GetString("Code") + " " + openIdent.GetString("Name");
                    // This flow is for idents.
                    var order = Base.Store.OpenOrder;
                    var code2d = Base.Store.code2D;
                    if (order != null)
                    {
                        packaging = order.Packaging ?? 0;
                        quantity = order.Quantity ?? 0;
                    }
                    if (order != null && code2d == null)
                    {

                        if (packaging != -1 && packaging <= quantity)
                        {
                            lbQty.Text = $"{Resources.GetString(Resource.String.s83)} \n ( " + quantity.ToString(await CommonData.GetQtyPictureAsync(this)) + " )";
                            tbPacking.Text = packaging.ToString();
                            stock = quantity;
                        }
                        else
                        {
                            quantity = order.Quantity ?? 0;
                            lbQty.Text = $"{Resources.GetString(Resource.String.s83)} \n ( " + quantity.ToString(await CommonData.GetQtyPictureAsync(this)) + " )";
                            tbPacking.Text = quantity.ToString();
                            stock = quantity;
                        }

                        await GetConnectedPositions(order.Order, order.Position ?? -1, order.Ident);
                        searchableSpinnerLocation.spinnerTextValueField.Text = await CommonData.GetSettingAsync("DefaultPaletteLocation", this);

                    }
                    else if (code2d != null)
                    {

                        tbSerialNum.Text = code2d.charge;
                        qtyCheck = 0;
                        double result;

                        // Try to parse the string to a double
                        if (Double.TryParse(code2d.netoWeight, out result))
                        {
                            qtyCheck = result;
                            lbQty.Text = $"{Resources.GetString(Resource.String.s83)} \n ( " + qtyCheck.ToString(await CommonData.GetQtyPictureAsync(this)) + " )";
                            tbPacking.Text = qtyCheck.ToString();
                            stock = qtyCheck;

                        }

                        tbPacking.RequestFocus();

                        await GetConnectedPositions(code2d.__helper__convertedOrder, code2d.__helper__position, code2d.ident);

                        searchableSpinnerLocation.spinnerTextValueField.Text = await CommonData.GetSettingAsync("DefaultPaletteLocation", this);
                        // Reset the 2d code to nothing
                        Base.Store.code2D = null;

                    }
                    else
                    {
                        // This is the orderless process.
                        qtyCheck = 10000000;
                        searchableSpinnerLocation.spinnerTextValueField.Text = await CommonData.GetSettingAsync("DefaultPaletteLocation", this);
                        lbQty.Text = $"{Resources.GetString(Resource.String.s83)} \n ( " + Resources.GetString(Resource.String.s335) + " )";
                        stock = qtyCheck;

                    }

                }

                isPackaging = openIdent.GetBool("IsPackaging");

                if (isPackaging)
                {
                    ssccRow.Visibility = ViewStates.Gone;
                    serialRow.Visibility = ViewStates.Gone;
                }

                if (!Base.Store.isUpdate)
                {
                    if (ssccRow.Visibility == ViewStates.Visible && (CommonData.GetSetting("AutoCreateSSCC") == "1"))
                    {
                        tbSSCC.Text = CommonData.GetNextSSCC();
                        if (serialRow.Visibility == ViewStates.Visible)
                        {
                            tbSerialNum.RequestFocus();
                        }
                    }
                    else if (ssccRow.Visibility == ViewStates.Visible)
                    {
                        if (tbSSCC.Text == string.Empty)
                        {
                            tbSSCC.RequestFocus();
                        }
                    }
                    else if (serialRow.Visibility == ViewStates.Visible)
                    {
                        if (tbSerialNum.Text == string.Empty)
                        {
                            tbSerialNum.RequestFocus();
                        }
                    }
                    else if (serialRow.Visibility != ViewStates.Visible && ssccRow.Visibility != ViewStates.Visible)
                    {
                        searchableSpinnerLocation.spinnerTextValueField.RequestFocus();
                    }
                }

            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        /// </summary> CONTINUE HERE
        /// <param name="acKey">Številka naročila</param>
        /// <param name="anNo">Pozicija znotraj naročila</param>
        /// <param name="acIdent">Ident</param>
        private async Task GetConnectedPositions(string acKey, int anNo, string acIdent)
        {
            try
            {
                connectedPositions.Clear();
                var sql = "SELECT acName, acSubject, anQty, anNo, acKey, anMaxQty from uWMSOrderItemByKeyIn WHERE acKey = @acKey AND anNo = @anNo AND acIdent = @acIdent";
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
                            connectedPositions.Add(new Takeover
                            {
                                acName = row.StringValue("acName"),
                                acSubject = row.StringValue("acSubject"),
                                anQty = row.DoubleValue("anQty"),
                                anNo = (int)(row.IntValue("anNo") ?? -1),
                                acKey = row.StringValue("acKey"),
                                acIdent = row.StringValue("acIdent"),
                                anMaxQty = row.DoubleValue("anMaxQty")
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

                    StartActivity(typeof(MainMenu));
                    Finish();
                }
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }
        private void BtBack_Click(object? sender, EventArgs e)
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
                StartActivity(typeof(TakeOverEnteredPositionsView));
                Finish();
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
                    popupDialogConfirm.Dismiss();
                    popupDialogConfirm.Hide();

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
                            // UI changes.
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
        /// <summary>
        /// For the purposes of the finish button
        /// </summary>
        /// <returns></returns>
        private async Task<bool> SaveMoveItem()
        {
            try
            {
                if (!Base.Store.isUpdate)
                {

                    double parsed;
                    if (double.TryParse(tbPacking.Text, out parsed))
                    {

                        var isCorrectLocation = await IsLocationCorrect();
                        if (!isCorrectLocation)
                        {
                            RunOnUiThread(() =>
                            {
                                Toast.MakeText(this, $"{Resources.GetString(Resource.String.s333)}", ToastLength.Long).Show();
                            });
                            // Nepravilna lokacija za izbrano skladišče
                            return false;
                        }

                        if (Base.Store.byOrder)
                        {
                            var isDuplicatedSerial = await IsDuplicatedSerialOrAndSSCC(tbSerialNum.Text ?? string.Empty, tbSSCC.Text ?? string.Empty);
                            var fieldsExist = ssccRow.Visibility == ViewStates.Visible || serialRow.Visibility == ViewStates.Visible;

                            if (isDuplicatedSerial && fieldsExist)
                            {
                                RunOnUiThread(() =>
                                {
                                    Toast.MakeText(this, $"{Resources.GetString(Resource.String.s334)}", ToastLength.Long).Show();
                                });
                                // Duplicirana serijska in/ali sscc koda.
                                return false;
                            }
                        }
                        else
                        {

                            string ident = openIdent.GetString("Code");
                            string warehouse = string.Empty;

                            if (Base.Store.isUpdate)
                            {
                                warehouse = moveItem.GetString("Wharehouse");
                            }
                            else
                            {
                                warehouse = moveHead.GetString("Wharehouse");
                            }

                            var isDuplicatedSerial = await IsDuplicatedSerialOrAndSSCCNotByOrder(ident, warehouse, tbSerialNum.Text, tbSSCC.Text);

                            if (isDuplicatedSerial)
                            {
                                RunOnUiThread(() =>
                                {
                                    Toast.MakeText(this, $"{Resources.GetString(Resource.String.s334)}", ToastLength.Long).Show();
                                });
                                // Duplicirana serijska in/ali sscc koda.
                                return false;
                            }
                        }

                        if (connectedPositions.Count == 1 || !Base.Store.byOrder)
                        {
                            var element = new Takeover { };
                            if (Base.Store.byOrder)
                            {
                                element = connectedPositions.ElementAt(0);
                                double parsedSave = Convert.ToDouble(tbPacking.Text.Trim());
                                double maxQty = element.anMaxQty ?? 0;
                                if (parsedSave >= maxQty)
                                {
                                    RunOnUiThread(() =>
                                    {
                                        Toast.MakeText(this, $"{Resources.GetString(Resource.String.s354)}", ToastLength.Long).Show();
                                    });
                                    return false;
                                }

                            }
                            moveItem = new NameValueObject("MoveItem");
                            moveItem.SetInt("HeadID", moveHead.GetInt("HeadID"));

                            if (Base.Store.byOrder)
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
                            moveItem.SetString("Location", searchableSpinnerLocation.spinnerTextValueField.Text.Trim());
                            moveItem.SetString("Palette", "1");

                            string error;

                            moveItem = Services.SetObject("mi", moveItem, out error);

                            if (moveItem != null && error == string.Empty)
                            {
                                InUseObjects.Invalidate("MoveItem");
                                return true;
                            } else
                            {
                                return false;
                            }
                        }
                        else
                        {
                            return false;
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
                else
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
            } catch(Exception ex)
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
                        if (double.TryParse(tbPacking.Text, out parsed))
                        {

                            var isCorrectLocation = await IsLocationCorrect();
                            if (!isCorrectLocation)
                            {
                                // Nepravilna lokacija za izbrano skladišče
                                Toast.MakeText(this, $"{Resources.GetString(Resource.String.s333)}", ToastLength.Long).Show();
                                return;
                            }

                            if (Base.Store.byOrder)
                            {
                                var isDuplicatedSerial = await IsDuplicatedSerialOrAndSSCC(tbSerialNum.Text ?? string.Empty, tbSSCC.Text ?? string.Empty);
                                var fieldsExist = ssccRow.Visibility == ViewStates.Visible || serialRow.Visibility == ViewStates.Visible;

                                if (isDuplicatedSerial && fieldsExist)
                                {
                                    // Duplicirana serijska in/ali sscc koda.
                                    Toast.MakeText(this, $"{Resources.GetString(Resource.String.s334)}", ToastLength.Long).Show();
                                    return;
                                }
                            }
                            else
                            {

                                string ident = openIdent.GetString("Code");
                                string warehouse = string.Empty;

                                if (Base.Store.isUpdate)
                                {
                                    warehouse = moveItem.GetString("Wharehouse");
                                }
                                else
                                {
                                    warehouse = moveHead.GetString("Wharehouse");
                                }

                                var isDuplicatedSerial = await IsDuplicatedSerialOrAndSSCCNotByOrder(ident, warehouse, tbSerialNum.Text, tbSSCC.Text);

                                if (isDuplicatedSerial)
                                {
                                    // Duplicirana serijska in/ali sscc koda.
                                    Toast.MakeText(this, $"{Resources.GetString(Resource.String.s334)}", ToastLength.Long).Show();
                                    return;
                                }
                            }


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

        private async Task<bool> IsDuplicatedSerialOrAndSSCC(string? serial = null, string? sscc = null)
        {
            try
            {
                bool result = false;

                string ident = string.Empty;

                ident = openIdent.GetString("Code");

                var parameters = new List<Services.Parameter>();
                parameters.Add(new Services.Parameter { Name = "acIdent", Type = "String", Value = ident });
                string serialDuplication = await CommonData.GetSettingAsync("NoSerialnoDupOut", this);
                string identType = openIdent.GetString("SerialNo");

                if (serialDuplication == "1")
                {
                    if (identType == "O")
                    {

                        string sql = "SELECT COUNT(*) AS anResult FROM uWMSMoveItemInClick WHERE acIdent = @acIdent";
                        if (serial != null && serial != string.Empty)
                        {
                            parameters.Add(new Services.Parameter { Name = "acSerialno", Type = "String", Value = serial });
                            sql += " AND acSerialNo = @acSerialno";
                        }
                        if (sscc != null && sscc != string.Empty)
                        {
                            parameters.Add(new Services.Parameter { Name = "acSSCC", Type = "String", Value = sscc });
                            sql += " AND acSSCC = @acSSCC;";
                        }

                        var duplicates = Services.GetObjectListBySql(sql, parameters);

                        if (duplicates.Success)
                        {
                            int numberRows = (int)(duplicates.Rows[0].IntValue("anResult") ?? 0);
                            if (numberRows > 0)
                            {
                                result = true;
                            }
                        }
                        return result;
                    } else
                    {
                        return false;
                    }
                } else
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

        private async Task CreateMethodFromStart()
        {
            try
            {
                await Task.Run(async () =>
                {
                    if (connectedPositions.Count == 1 || !Base.Store.byOrder)
                    {
                        var element = new Takeover { };
                        if (Base.Store.byOrder)
                        {
                            element = connectedPositions.ElementAt(0);
                            double parsed = Convert.ToDouble(tbPacking.Text.Trim());
                            double maxQty = element.anMaxQty ?? 0;
                            if (parsed > maxQty)
                            {
                                if (await CommonData.GetSettingAsync("CheckTakeOverOpenQty ", this) == "1")
                                {
                                    RunOnUiThread(() =>
                                    {
                                        Toast.MakeText(this, $"{Resources.GetString(Resource.String.s354)}", ToastLength.Long).Show();
                                    });
                                    return;
                                } else
                                {
                                    var result = await ShowConfirmationDialogAsync();
                                    if (!result)
                                    {
                                        return; // User selected "No", so we exit here. 8.10.2024 Janko Jovičić
                                    }
                                }
                            }

                        }
                        moveItem = new NameValueObject("MoveItem");
                        moveItem.SetInt("HeadID", moveHead.GetInt("HeadID"));

                        if (Base.Store.byOrder)
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
                        moveItem.SetString("Location", searchableSpinnerLocation.spinnerTextValueField.Text.Trim());
                        moveItem.SetString("Palette", "1");



                        string error;

                        moveItem = Services.SetObject("mi", moveItem, out error);

                        if (moveItem != null && error == string.Empty)
                        {
                            RunOnUiThread(() =>
                            {

                                var intent = new Intent(this, typeof(TakeOverIdentEntry));
                                intent.SetFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask);
                                StartActivity(intent);

                            });

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



        private Task<bool> ShowConfirmationDialogAsync()
        {
            var tcs = new TaskCompletionSource<bool>();

            RunOnUiThread(() =>
            {
                AlertDialog.Builder alert = new AlertDialog.Builder(this);
                alert.SetTitle(Resources.GetString(Resource.String.s357));
                alert.SetMessage(Resources.GetString(Resource.String.s356));
                alert.SetPositiveButton(Resources.GetString(Resource.String.s358), (senderAlert, args) =>
                {
                    tcs.SetResult(true); // User clicked "Yes"
                });
                alert.SetNegativeButton(Resources.GetString(Resource.String.s359), (senderAlert, args) =>
                {
                    tcs.SetResult(false); // User clicked "No"
                });

                Dialog dialog = alert.Create();
                dialog.Show();
            });

            return tcs.Task; // Wait for the dialog to return a result
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
                        if (Base.Store.byOrder)
                        {
                            var isDuplicatedSerial = await IsDuplicatedSerialOrAndSSCC(tbSerialNum.Text ?? string.Empty, tbSSCC.Text ?? string.Empty);
                            var fieldsExist = ssccRow.Visibility == ViewStates.Visible || serialRow.Visibility == ViewStates.Visible;
                            if (isDuplicatedSerial && fieldsExist)
                            {
                                // Duplicirana serijska in/ali sscc koda.
                                Toast.MakeText(this, $"{Resources.GetString(Resource.String.s334)}", ToastLength.Long).Show();
                                return;
                            }



                        }
                        else
                        {
                            string ident = openIdent.GetString("Code");
                            string warehouse = string.Empty;

                            if (Base.Store.isUpdate)
                            {
                                warehouse = moveItem.GetString("Wharehouse");

                            }
                            else
                            {
                                warehouse = moveHead.GetString("Wharehouse");
                            }
                            var isDuplicatedSerial = await IsDuplicatedSerialOrAndSSCCNotByOrder(ident, warehouse, tbSerialNum.Text, tbSSCC.Text);
                            if (isDuplicatedSerial)
                            {
                                // Duplicirana serijska in/ali sscc koda.
                                Toast.MakeText(this, $"{Resources.GetString(Resource.String.s334)}", ToastLength.Long).Show();
                                return;
                            }
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

        private async Task<bool> IsDuplicatedSerialOrAndSSCCNotByOrder(string ident, string warehouse, string serial = null, string sscc = null)
        {
            try
            {
                string serialDuplication = await CommonData.GetSettingAsync("NoSerialnoDupOut", this);
                string identType = openIdent.GetString("SerialNo");

                if (serialDuplication == "1")
                {
                    if (identType == "O")
                    {

                        string sql = "SELECT COUNT(*) as anResult FROM uWMSMoveItemInClickNoOrder WHERE acIdent = @acIdent and acWharehouse = @acWharehouse";

                        if (serial != null)
                        {
                            sql += " AND acSerialno = @acSerialno";
                        }

                        if (sscc != null)
                        {
                            sql += " AND acSSCC = @acSSCC";
                        }

                        if (warehouse != null)
                        {
                            sql += " AND acWharehouse = @acWharehouse";
                        }

                        var parameters = new List<Services.Parameter>();

                        parameters.Add(new Services.Parameter { Name = "acIdent", Type = "String", Value = ident });
                        parameters.Add(new Services.Parameter { Name = "acSerialno", Type = "String", Value = serial });
                        parameters.Add(new Services.Parameter { Name = "acSSCC", Type = "String", Value = sscc });
                        parameters.Add(new Services.Parameter { Name = "acWharehouse", Type = "String", Value = warehouse });

                        var duplicates = Services.GetObjectListBySql(sql, parameters);

                        if (duplicates.Success)
                        {
                            int numberOfDuplicates = (int?)duplicates.Rows[0].IntValue("anResult") ?? 0;
                            if (numberOfDuplicates > 0)
                            {
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }

                        return false;
                    }
                }
                return false;
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
                var picture = await CommonData.GetQtyPictureAsync(this);

                await Task.Run(async() =>
                {
                    if (connectedPositions.Count == 1 || !Base.Store.byOrder)
                    {


                        var element = new Takeover { };

                        if (Base.Store.byOrder)
                        {
                            element = connectedPositions.ElementAt(0);
                            double parsed = Convert.ToDouble(tbPacking.Text.Trim());
                            double maxQty = element.anMaxQty ?? 0;
                            if (parsed > maxQty)
                            {
                                if (await CommonData.GetSettingAsync("CheckTakeOverOpenQty ", this) == "1")
                                {
                                    RunOnUiThread(() =>
                                    {
                                        Toast.MakeText(this, $"{Resources.GetString(Resource.String.s354)}", ToastLength.Long).Show();
                                    });
                                    return;
                                }
                                else
                                {
                                    var result = await ShowConfirmationDialogAsync();
                                    if (!result)
                                    {
                                        return; // User selected "No", so we exit here. 8.10.2024 Janko Jovičić
                                    }
                                }
                            }                          
                        }

                        // This solves the problem of updating the item. The problem occurs because of the old way of passing data.
                        moveItem = new NameValueObject("MoveItem");
                        moveItem.SetInt("HeadID", moveHead.GetInt("HeadID"));

                        if (Base.Store.byOrder)
                        {
                            moveItem.SetString("LinkKey", element.acKey);
                            moveItem.SetInt("LinkNo", element.anNo);
                        }
                        else
                        {
                            // update proccess
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
                        moveItem.SetString("Location", searchableSpinnerLocation.spinnerTextValueField.Text.Trim());
                        moveItem.SetString("Palette", "1");



                        string error;
                        moveItem = Services.SetObject("mi", moveItem, out error);
                        if (moveItem != null && error == string.Empty)
                        {

                            if (Base.Store.byOrder)
                            {


                                RunOnUiThread(() =>
                                {

                                    var currentQty = Convert.ToDouble(tbPacking.Text.Trim());
                                    stock -= currentQty;
                                    lbQty.Text = $"{Resources.GetString(Resource.String.s83)} \n ( " + stock.ToString(picture) + " )";


                                    if (stock <= 0)
                                    {

                                        var intent = new Intent(this, typeof(TakeOverIdentEntry));
                                        intent.SetFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask);
                                        StartActivity(intent);

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

                                    if (ssccRow.Visibility == ViewStates.Visible && (CommonData.GetSetting("AutoCreateSSCC") == "1"))
                                    {
                                        tbSSCC.Text = CommonData.GetNextSSCC();
                                        if (serialRow.Visibility == ViewStates.Visible)
                                        {
                                            tbSerialNum.RequestFocus();
                                        }
                                        // If the process stays on the same activity this is needed. 8.13.2024 Janko Jovičić
                                    }


                                    // tbPacking.Text = string.Empty; This seems to be more logical if commented out. User wants to just scan serial codes. 5.15.2024 Janko Jovičić

                                });
                            }

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
                    case Keycode.F2:
                        BtSaveOrUpdate_Click(this, null);
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
                        BtBack_Click(this, null);
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

                        }
                    }
                    else if (tbSerialNum.HasFocus)
                    {
                        if (barcode != "Scan fail")
                        {

                            tbSerialNum.Text = barcode;

                            tbPacking.RequestFocus();

                        }
                    }
                    else if (searchableSpinnerLocation.spinnerTextValueField.HasFocus)
                    {
                        if (barcode != "Scan fail")
                        {

                            searchableSpinnerLocation.spinnerTextValueField.Text = barcode;

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
    }




}

