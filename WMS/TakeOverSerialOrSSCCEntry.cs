using Stream = Android.Media.Stream;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Media;
using Android.Net;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using BarCode2D_Receiver;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using WMS.App;
using TrendNET.WMS.Core.Data;
using TrendNET.WMS.Device.App;
using TrendNET.WMS.Device.Services;
using static Android.App.ActionBar;
using WebApp = TrendNET.WMS.Device.Services.WebApp;

using AndroidX.AppCompat.App;
using AlertDialog = Android.App.AlertDialog;
using Android.Graphics.Drawables;
using Android.Graphics;
using Newtonsoft.Json;
using System;
using Java.IO;
namespace WMS
{
    [Activity(Label = "TakeOverSerialOrSSCCEntry", ScreenOrientation = ScreenOrientation.Portrait)]
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
        private EditText tbLocation;
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
        private ProgressDialogClass progress;

        protected async override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetTheme(Resource.Style.AppTheme_NoActionBar);
            SetContentView(Resource.Layout.TakeOverSerialOrSSCCEntry);
            // Definitions
            AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
            var _customToolbar = new CustomToolbar(this, toolbar, Resource.Id.navIcon);
            _customToolbar.SetNavigationIcon(settings.RootURL + "/Services/Logo");
            SetSupportActionBar(_customToolbar._toolbar);
            SupportActionBar.SetDisplayShowTitleEnabled(false);
            tbIdent = FindViewById<EditText>(Resource.Id.tbIdent);
            tbSSCC = FindViewById<EditText>(Resource.Id.tbSSCC);
            tbSerialNum = FindViewById<EditText>(Resource.Id.tbSerialNum);
            tbLocation = FindViewById<EditText>(Resource.Id.tbLocation);
            tbPacking = FindViewById<EditText>(Resource.Id.tbPacking);
            tbIdent.InputType = Android.Text.InputTypes.ClassNumber;
            tbSSCC.InputType = Android.Text.InputTypes.ClassNumber;
            tbLocation.InputType = Android.Text.InputTypes.ClassNumber;
            btSaveOrUpdate = FindViewById<Button>(Resource.Id.btSaveOrUpdate);
            btCreate = FindViewById<Button>(Resource.Id.btCreate);
            btFinish = FindViewById<Button>(Resource.Id.btFinish);
            btOverview = FindViewById<Button>(Resource.Id.btOverview);
            btBack = FindViewById<Button>(Resource.Id.btBack);
            lbQty = FindViewById<TextView>(Resource.Id.lbQty);
            lbUnits = FindViewById<TextView>(Resource.Id.lbUnits);
            Window.SetSoftInputMode(Android.Views.SoftInput.AdjustResize);
            soundPool = new SoundPool(10, Stream.Music, 0);
            soundPoolId = soundPool.Load(this, Resource.Raw.beep, 1);
            Barcode2D barcode2D = new Barcode2D();
            barcode2D.open(this, this);
            ssccRow = FindViewById<LinearLayout>(Resource.Id.sscc_row);
            serialRow = FindViewById<LinearLayout>(Resource.Id.serial_row);
            btSaveOrUpdate.Click += BtSaveOrUpdate_Click;
            btCreate.Click += BtCreate_Click;
            btFinish.Click += BtFinish_Click;
            btOverview.Click += BtOverview_Click;
            btBack.Click += BtBack_Click;

            // Method calls

            CheckIfApplicationStopingException();

            // Color the fields that can be scanned
            ColorFields();            

            // Stop the loader
            LoaderManifest.LoaderManifestLoopStop(this);

            SetUpProcessDependentButtons();

            // Main logic for the entry
            SetUpForm();
        }


        private void SetUpProcessDependentButtons()
        {
            // This method changes the UI so it shows in a visible way that it is the update screen. - 18.03.2024
            if (Base.Store.isUpdate)
            {
                btSaveOrUpdate.Visibility = ViewStates.Gone;
                btCreate.Text = $"{Resources.GetString(Resource.String.s290)}";
            } else if(Base.Store.code2D!=null)
            {
                btSaveOrUpdate.Visibility = ViewStates.Gone;
                // 2d code reading process.
            }
        }


        private void ColorFields()
        {
            tbSSCC.SetBackgroundColor(Android.Graphics.Color.Aqua);
            tbSerialNum.SetBackgroundColor(Android.Graphics.Color.Aqua);
            tbLocation.SetBackgroundColor(Android.Graphics.Color.Aqua);
        }
        private void SetUpForm()
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
                tbLocation.Text = moveItem.GetString("Location");
                tbPacking.Text = moveItem.GetDouble("Qty").ToString();
                lbQty.Text = $"{Resources.GetString(Resource.String.s155)} ( " + moveItem.GetDouble("Qty").ToString() + " )";
                btCreate.Text = $"{Resources.GetString(Resource.String.s293)}";
                // Lock down all other fields
                tbIdent.Enabled = false;
                tbSerialNum.Enabled = false;
                tbSSCC.Enabled = false;
                tbLocation.Enabled = false;
            }
            else
            {
                tbIdent.Text = openIdent.GetString("Code") + " " + openIdent.GetString("Name");
                // This flow is for idents.
                var order = Base.Store.OpenOrder;
                var code2d = Base.Store.code2D;
                if (order != null)
                {
                    qtyCheck = order.Quantity ?? 0;
                    lbQty.Text = $"{Resources.GetString(Resource.String.s155)} ( " + qtyCheck.ToString(CommonData.GetQtyPicture()) + " )";
                    tbPacking.Text = qtyCheck.ToString();
                    stock = qtyCheck;
                    GetConnectedPositions(order.Order, order.Position ?? -1, order.Ident);
                    tbLocation.Text = CommonData.GetSetting("DefaultPaletteLocation");
                    
                } else if (code2d != null)
                {
                   
                    tbSerialNum.Text = code2d.charge;
                    qtyCheck = 0;
                    double result;

                    // Try to parse the string to a double
                    if (Double.TryParse(code2d.netoWeight, out result))
                    {
                        qtyCheck = result;
                        lbQty.Text = $"{Resources.GetString(Resource.String.s155)} ( " + qtyCheck.ToString(CommonData.GetQtyPicture()) + " )";
                        tbPacking.Text = qtyCheck.ToString();
                        stock = qtyCheck;

                    }

                    GetConnectedPositions(code2d.__helper__convertedOrder, code2d.__helper__position, code2d.ident);

                    tbLocation.Text = CommonData.GetSetting("DefaultPaletteLocation");
                    // Reset the 2d code to nothing
                    Base.Store.code2D = null;

                    tbPacking.RequestFocus();
                    tbPacking.SelectAll();
                }                 
                else
                {
                    // This is the orderless process.
                    qtyCheck = 10000000;
                    tbLocation.Text = CommonData.GetSetting("DefaultPaletteLocation");
                    lbQty.Text = $"{Resources.GetString(Resource.String.s155)} ( " + Resources.GetString(Resource.String.s335) + " )";                   
                    stock = qtyCheck;
                    tbPacking.RequestFocus();
                    tbPacking.SelectAll();
                }

            }

            isPackaging = openIdent.GetBool("IsPackaging");

            if (isPackaging)
            {
                ssccRow.Visibility = ViewStates.Gone;
                serialRow.Visibility = ViewStates.Gone;
            }
   
        }

        /// </summary>
        /// <param name="acKey">Številka naročila</param>
        /// <param name="anNo">Pozicija znotraj naročila</param>
        /// <param name="acIdent">Ident</param>
        private void GetConnectedPositions(string acKey, int anNo, string acIdent, string acLocation = null)
        {
            connectedPositions.Clear();
            var sql = "SELECT * from uWMSOrderItemByKeyIn WHERE acKey = @acKey AND anNo = @anNo AND acIdent = @acIdent";
            var parameters = new List<Services.Parameter>();
            parameters.Add(new Services.Parameter { Name = "acKey", Type = "String", Value = acKey });
            parameters.Add(new Services.Parameter { Name = "anNo", Type = "Int32", Value = anNo });
            parameters.Add(new Services.Parameter { Name = "acIdent", Type = "String", Value = acIdent });
            if (acLocation != null)
            {
                parameters.Add(new Services.Parameter { Name = "acLocation", Type = "String", Value = acLocation });
                sql += " AND acLocation = @acLocation;";
            }
            var subjects = Services.GetObjectListBySql(sql, parameters);
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
                        connectedPositions.Add(new Takeover
                        {
                            acName = row.StringValue("acName"),
                            acSubject = row.StringValue("acSubject"),
                            acSerialNo = row.StringValue("acSerialNo"),
                            acSSCC = row.StringValue("acSSCC"),
                            anQty = row.DoubleValue("anQty"),
                            aclocation = row.StringValue("aclocation"),
                            anNo = (int)(row.IntValue("anNo") ?? -1),
                            acKey = row.StringValue("acKey"),
                            acIdent = row.StringValue("acIdent")
                        });
                    }
                }
            }
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
        private void BtBack_Click(object? sender, EventArgs e)
        {
            StartActivity(typeof(MainMenu));
            HelpfulMethods.clearTheStack(this);
            Finish();
        }

        private void BtOverview_Click(object? sender, EventArgs e)
        {
            StartActivity(typeof(TakeOverEnteredPositionsView));
            HelpfulMethods.clearTheStack(this);
            Finish();
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
                        if (WebApp.Get("mode=finish&stock=add&print=" + Services.DeviceUser() + "&id=" + headID.ToString(), out result))
                        {
                            if (result.StartsWith("OK!"))
                            {
                                RunOnUiThread(() =>
                                {
                                    progress.StopDialogSync();
                                    var id = result.Split('+')[1];
                                    AlertDialog.Builder alert = new AlertDialog.Builder(this);
                                    alert.SetTitle($"{Resources.GetString(Resource.String.s263)}");
                                    alert.SetMessage($"{Resources.GetString(Resource.String.s264)}" + id);
                                    alert.SetPositiveButton("Ok", (senderAlert, args) =>
                                    {
                                        alert.Dispose();
                                        System.Threading.Thread.Sleep(500);
                                        StartActivity(typeof(MainMenu));
                                        HelpfulMethods.clearTheStack(this);
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
                                        System.Threading.Thread.Sleep(500);
                                        StartActivity(typeof(MainMenu));
                                        HelpfulMethods.clearTheStack(this);
                                    });
                                    Dialog dialog = alert.Create();
                                    dialog.Show();
                                });
                            }
                        }
                        else
                        {
                            DialogHelper.ShowDialogError(this, this, $"{Resources.GetString(Resource.String.s218)}" + result);
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

        private async void BtCreate_Click(object? sender, EventArgs e)
        {
            if (!Base.Store.isUpdate)
            {

                double parsed;
                if (double.TryParse(tbPacking.Text, out parsed) && stock >= parsed)
                {

                    var isCorrectLocation = IsLocationCorrect();
                    if (!isCorrectLocation)
                    {
                        // Nepravilna lokacija za izbrano skladišče
                        Toast.MakeText(this, $"{Resources.GetString(Resource.String.s333)}", ToastLength.Long).Show();
                        return;
                    }

                    // Only if its an ordered takeover. 12.04.2024 Janko

                    var isDuplicatedSerial = IsDuplicatedSerialOrAndSSCC(tbSerialNum.Text ?? string.Empty, tbSSCC.Text ?? string.Empty);

                    if (isDuplicatedSerial)
                    {
                        // Duplicirana serijska in/ali sscc koda.
                        Toast.MakeText(this, $"{Resources.GetString(Resource.String.s334)}", ToastLength.Long).Show();
                        return;
                    }

                    // Only if its an ordered takeover. 12.04.2024 Janko

                    await CreateMethodFromStart();                                     
                }
                else
                {
                    Toast.MakeText(this, $"{Resources.GetString(Resource.String.s270)}", ToastLength.Long).Show();
                }

            } else
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

        private bool IsDuplicatedSerialOrAndSSCC(string? serial = null, string? sscc = null)
        {
            bool result = false;

            string ident = string.Empty;
    
            ident = openIdent.GetString("Code");
            
            var parameters = new List<Services.Parameter>();
            parameters.Add(new Services.Parameter { Name = "acIdent", Type = "String", Value = ident });

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

            if(duplicates.Success)
            {
                int numberRows = (int)(duplicates.Rows[0].IntValue("anResult") ?? 0);
                if(numberRows>0)
                {
                    result = true;
                }
            }

            return result;
        }


        private bool IsLocationCorrect()
        {
            // TODO: Add a way to check serial numbers
            string location = tbLocation.Text;

            if (!CommonData.IsValidLocation(moveHead.GetString("Wharehouse"), location))
            {
                return false;
            } else
            {
                return true;
            }
        }

        private async Task CreateMethodFromStart()
        {
            await Task.Run(() =>
            {
                if (connectedPositions.Count == 1)
                {
                    var element = connectedPositions.ElementAt(0);
                    moveItem = new NameValueObject("MoveItem");
                    moveItem.SetInt("HeadID", moveHead.GetInt("HeadID"));
                    moveItem.SetString("LinkKey", element.acKey); // here
                    moveItem.SetInt("LinkNo", element.anNo);
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

                            StartActivity(typeof(TakeOverIdentEntry));
                            Finish();
                          
                        });

                    }
                }
                else
                {
                    return;
                }
            });
        }

        private async void BtSaveOrUpdate_Click(object sender, EventArgs e)
        {
            double parsed;
            if(double.TryParse(tbPacking.Text, out parsed) && stock>=parsed)
            {
                var isCorrectLocation = IsLocationCorrect();
                if (!isCorrectLocation)
                {
                    // Nepravilna lokacija za izbrano skladišče
                    Toast.MakeText(this, $"{Resources.GetString(Resource.String.s333)}", ToastLength.Long).Show();
                    return;
                }
                if (Base.Store.byOrder)
                {
                    var isDuplicatedSerial = IsDuplicatedSerialOrAndSSCC(tbSerialNum.Text ?? string.Empty, tbSSCC.Text ?? string.Empty);

                    if (isDuplicatedSerial)
                    {
                        // Duplicirana serijska in/ali sscc koda.
                        Toast.MakeText(this, $"{Resources.GetString(Resource.String.s334)}", ToastLength.Long).Show();
                        return;
                    }
                } else
                {
                    string ident = openIdent.GetString("Code");
                    string warehouse = string.Empty;                                         
                    if(Base.Store.isUpdate)
                    {
                        warehouse = moveItem.GetString("Wharehouse");

                    } else
                    {
                        warehouse = moveHead.GetString("Wharehouse");
                    }
                    var isDuplicatedSerial = IsDuplicatedSerialOrAndSSCCNotByOrder(ident, tbSerialNum.Text, tbSSCC.Text);
                    if (isDuplicatedSerial)
                    {
                        // Duplicirana serijska in/ali sscc koda.
                        Toast.MakeText(this, $"{Resources.GetString(Resource.String.s334)}", ToastLength.Long).Show();
                        return;
                    }
                }
                await CreateMethodSame();
            }
        }

        private bool IsDuplicatedSerialOrAndSSCCNotByOrder(string ident, string serial = null, string sscc = null)
        {
            string serialDuplication = CommonData.GetSetting("NoSerialnoDupOut");
            string identType = openIdent.GetString("SerialNo");
         
            if (serialDuplication == "1")
            {
                if (identType == "O")
                {
                    string sql = "SELECT COUNT(*) as anResult FROM uWMSMoveItemInClickNoOrder WHERE acIdent = @acIdent";
                    if(serial!=null)
                    {
                        sql += " AND acSerialno = @acSerialno";
                    }
                    if(sscc!=null)
                    {
                        sql += " AND acSSCC = @acSSCC;";
                    }
                    var parameters = new List<Services.Parameter>();
                    parameters.Add(new Services.Parameter { Name = "acIdent", Type = "String", Value = ident });
                    parameters.Add(new Services.Parameter { Name = "acSerialno", Type = "String", Value = serial });
                    parameters.Add(new Services.Parameter { Name = "acSSCC", Type = "String", Value = sscc });
                    var duplicates = Services.GetObjectListBySql(sql, parameters);
                    if(duplicates.Success)
                    {
                        int numberOfDuplicates = (int?) duplicates.Rows[0].IntValue("anResult") ?? 0;
                        if(numberOfDuplicates>0)
                        {
                            return true;
                        } else
                        {
                            return false;
                        }
                    }
                    return false;
                } 
            }
            return false;
        }

        private async Task CreateMethodSame()
        {
            await Task.Run(() =>
            {
                if (connectedPositions.Count == 1 || !Base.Store.byOrder)
                {
                    var element = new Takeover { };

                    if(Base.Store.byOrder)
                    {
                        element = connectedPositions.ElementAt(0);
                    }

                    // This solves the problem of updating the item. The problem occurs because of the old way of passing data.
                    moveItem = new NameValueObject("MoveItem");
                    moveItem.SetInt("HeadID", moveHead.GetInt("HeadID"));

                    if (Base.Store.byOrder)
                    {
                        moveItem.SetString("LinkKey", element.acKey);
                        moveItem.SetInt("LinkNo", element.anNo);
                    } else
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

                        if(Base.Store.byOrder)
                        {

                            var currentQty = Convert.ToDouble(tbPacking.Text.Trim());
                            stock -= currentQty;
                                        
                            RunOnUiThread(() =>
                            {
                                lbQty.Text = $"{Resources.GetString(Resource.String.s155)} ( " + stock.ToString(CommonData.GetQtyPicture()) + " )";
                            });

                        }

                        // Check to see if the maximum is already reached.
                        if (stock <= 0)
                        {                         
                            StartActivity(typeof(TakeOverIdentEntry));
                            Finish();                           
                        }

                        RunOnUiThread(() =>
                        {
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
                        });
                    }
                }
                else
                {
                    return;
                }
            });
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
                    Crashes.TrackError(err);
                }
            }
            else
            {
                LoaderManifest.LoaderManifestLoop(this);
            }
        }

        private void Sound()
        {
            soundPool.Play(soundPoolId, 1, 1, 0, 0, 1);
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

                    }
                }
                else if (tbSerialNum.HasFocus)
                {
                    if (barcode != "Scan fail")
                    {
                        Sound();

                        tbSerialNum.Text = barcode;

                        tbPacking.RequestFocus();

                    }
                }
                else if (tbLocation.HasFocus)
                {
                    if (barcode != "Scan fail")
                    {
                        Sound();

                        tbLocation.Text = barcode;

                    }
                }
            }
            catch (Exception ex)
            {
                Crashes.TrackError(ex);
                Toast.MakeText(this, $"{Resources.GetString(Resource.String.s225)}", ToastLength.Long).Show();
            }
        }
    }
        
    


    }

