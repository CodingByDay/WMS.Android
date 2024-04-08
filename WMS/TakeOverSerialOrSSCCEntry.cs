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

            // Main logic for the entry
            SetUpForm();

            // Stop the loader
            LoaderManifest.LoaderManifestLoopStop(this);

            SetUpUpdate();

        }


        private void SetUpUpdate()
        {
            // This method changes the UI so it shows in a visible way that it is the update screen. - 18.03.2024
            if (Base.Store.isUpdate)
            {
                btSaveOrUpdate.Visibility = ViewStates.Gone;
                btCreate.Text = $"{Resources.GetString(Resource.String.s290)}";
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
                qtyCheck = order.Quantity ?? 0;
                lbQty.Text = $"{Resources.GetString(Resource.String.s155)} ( " + qtyCheck.ToString(CommonData.GetQtyPicture()) + " )";
                tbPacking.Text = qtyCheck.ToString();
                stock = qtyCheck;
                GetConnectedPositions(order.Order, order.Position ?? -1, order.Ident);
                                  
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

        }

        private void BtOverview_Click(object? sender, EventArgs e)
        {

        }

        private void BtFinish_Click(object? sender, EventArgs e)
        {

        }

        private async void BtCreate_Click(object? sender, EventArgs e)
        {
            double parsed;
            if (double.TryParse(tbPacking.Text, out parsed) && stock >= parsed && dataIsCorrect())
            {
                if (!Base.Store.isUpdate)
                {
                    await CreateMethodFromStart();

                } else
                {


                // Update flow. 08.04.2024

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
                            StartActivity(typeof(TakeOverEnteredPositionsView));
                            Finish();
                        }
                    }
                }
                    else
                    {
                        Toast.MakeText(this, $"{Resources.GetString(Resource.String.s270)}", ToastLength.Long).Show();
                    }
                }
            } else
            {
                Toast.MakeText(this, $"{Resources.GetString(Resource.String.s270)}", ToastLength.Long).Show();
            }
        }

        private bool dataIsCorrect()
        {
            // TODO: Add a way to check serial numbers
            string location = tbLocation.Text;
            string serial = tbSerialNum.Text;
            string sscc = tbSSCC.Text;

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
                    moveItem.SetString("LinkKey", element.acKey);
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
                await CreateMethodSame();
            }
        }

        private async Task CreateMethodSame()
        {
            await Task.Run(() =>
            {
                if (connectedPositions.Count == 1)
                {
                    var element = connectedPositions.ElementAt(0);
                    // This solves the problem of updating the item. The problem occurs because of the old way of passing data.
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
                    moveItem.SetString("Location", tbLocation.Text.Trim());
                    moveItem.SetString("Palette", "1");


                    string error;
                    moveItem = Services.SetObject("mi", moveItem, out error);
                    if (moveItem != null && error == string.Empty)
                    {

                        serialOverflowQuantity += Convert.ToDouble(tbPacking.Text.Trim());
                        stock -= serialOverflowQuantity;

                        RunOnUiThread(() =>
                        {
                            lbQty.Text = $"{Resources.GetString(Resource.String.s155)} ( " + stock.ToString(CommonData.GetQtyPicture()) + " )";
                        });

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

                            tbLocation.Text = string.Empty;
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
            throw new NotImplementedException();
        }
    }
        
    


    }

