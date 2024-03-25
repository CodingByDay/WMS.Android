using Stream = Android.Media.Stream;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics.Drawables;
using Android.Media;
using Android.Net;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using BarCode2D_Receiver;
using Com.Barcode;
using Com.Jsibbold.Zoomage;
using Microsoft.AppCenter.Crashes;
using Newtonsoft.Json;

using WMS.App;
using TrendNET.WMS.Core.Data;
using TrendNET.WMS.Device.App;
using TrendNET.WMS.Device.Services;
using static Android.App.ActionBar;
using static Android.App.DownloadManager;
using static Android.Provider.CalendarContract;
using WebApp = TrendNET.WMS.Device.Services.WebApp;

using AndroidX.AppCompat.App;
using AlertDialog = Android.App.AlertDialog;
using Android.Graphics;
namespace WMS
{
    [Activity(Label = "InterWarehouseSerialOrSSCCEntryTablet", ScreenOrientation = Android.Content.PM.ScreenOrientation.Landscape)]
    public class InterWarehouseSerialOrSSCCEntryTablet : CustomBaseActivity
    {
        public string barcode;
        // Definitions
        private EditText tbIdent;
        private EditText tbSSCC;
        private EditText tbSerialNum;
        private EditText tbIssueLocation;
        private EditText tbLocation;
        private EditText tbPacking;
        private EditText tbUnits;
        private TextView lbQty;
        private TextView lbUnits;
        private AdapterLocation adapterNew;
        private Button button1;
        private Button btSaveOrUpdate;
        private Button button3;
        private Button button5;
        private Button button4;
        private Spinner spIssue;
        private Spinner spReceive;
        private List<string> issuer = new List<string>();
        private List<string> receiver = new List<string>();
        private Button button6;
        private NameValueObject moveItemCurrent;
        private NameValueObject moveHead = (NameValueObject)InUseObjects.Get("MoveHead");
        private NameValueObject moveItem = (NameValueObject)InUseObjects.Get("MoveItem");
        private NameValueObjectList docTypes = null;
        private bool target = false;
        private bool editMode = false;
        private EditText lbIdentName;
        private Button finish;
     
        private ListView listData;
        private List<ProductionEnteredPositionViewList> data = new List<ProductionEnteredPositionViewList>();
        SoundPool soundPool;
        int soundPoolId;
        private NameValueObject wh;
        private int selected;
        private List<MorePallets> datax = new List<MorePallets>();
        private ImageView imagePNG;
        private ProgressDialogClass progress;
        private List<LocationClass> items = new List<LocationClass>();
        private Dialog popupDialog;
        private ZoomageView image;
        private Button btnOK;
        private Button btMorePallets;
        private Dialog popupDialogMain;
        private Button btConfirm;
        private Button btExit;
        private EditText tbSSCCpopup;
        private ListView lvCardMore;
        private MorePalletsAdapter adapter;
        private Button btnYes;
        private Button btnNo;
        private bool enabledSerial;
        private bool isBatch;
        private string tbLocationPopupVariable;
        private bool isFirst;
        private int lastHeadID;
        private int currentID;
        private NameValueObject moveItemBatch;
        private Button btConfirmLocation;
        private Button btExitLocation;
        private bool isMorePalletsMode = false;
        private Dialog popupDialogLocation;
        private CustomAutoCompleteAdapter<string> adapterReceive;
        private CustomAutoCompleteAdapter<string> adapterIssue;
        private Button btnYesLocationExit;
        private Button btnNoLocationExit;
        private Dialog popupDialogSure;
        private double totalAmount = 0;
        private Dialog popupDialogConfirm;
        private Button btnYesConfirm;
        private Button btnNoConfirm;
        private string warehouse;
        private string query;
        private ApiResultSet resultQuery;
        private MorePallets instancex;
        private object existsDuplicate;
        private bool isOkayToCallBarcode;

        private void BtSaveOrUpdate_LongClick(object sender, View.LongClickEventArgs e)
        {
            isMorePalletsMode = false;
            tbSSCC.Text = "";
            tbSerialNum.Text = "";
            tbPacking.Text = "";
            tbLocation.Text = "";
            tbIdent.Text = "";
            tbSSCC.RequestFocus();
        }
        private bool FillRelatedBranchIdentData(string text)
        {
            string error;

            var data = Services.GetObject("sscc", text, out error);



            if (data != null)
            {
                var ident = data.GetString("Ident");
                var name = data.GetString("IdentName");


                tbIdent.Text = ident;
                lbIdentName.Text = name;

              
                if (tbIdent.Text != null && lbIdentName.Text != null) { return true; } else { return false; }


            }
            else
            {
                return false;
            }
        }

        private void color()
        {
            tbIdent.SetBackgroundColor(Android.Graphics.Color.Aqua);
            tbSSCC.SetBackgroundColor(Android.Graphics.Color.Aqua);
            tbSerialNum.SetBackgroundColor(Android.Graphics.Color.Aqua);
            tbIssueLocation.SetBackgroundColor(Android.Graphics.Color.Aqua);
            tbLocation.SetBackgroundColor(Android.Graphics.Color.Aqua);

        }

        private MorePallets FillRelatedData(string text)
        {
            return FilData(text, true);
        }

        private async Task<bool> SaveMoveItem()
        {

            if (string.IsNullOrEmpty(tbIdent.Text.Trim()) && string.IsNullOrEmpty(tbSerialNum.Text.Trim()) && string.IsNullOrEmpty(tbPacking.Text.Trim()))
            {
                return true;
            }

            if (tbSSCC.Enabled && string.IsNullOrEmpty(tbSSCC.Text.Trim()))
            {
                RunOnUiThread(() =>
                {
                    string WebError = string.Format($"{Resources.GetString(Resource.String.s254)}");
                    DialogHelper.ShowDialogError(this, this, WebError);
                    tbSSCC.RequestFocus();
                });
              
                return false;
            }

            if (tbSerialNum.Enabled && string.IsNullOrEmpty(tbSerialNum.Text.Trim()))
            {
                RunOnUiThread(() =>
                {
                    string WebError = string.Format($"{Resources.GetString(Resource.String.s270)}");
                    DialogHelper.ShowDialogError(this, this, WebError);

                    tbSerialNum.RequestFocus();
                });
             
                return false;
            }

            if (string.IsNullOrEmpty(tbPacking.Text.Trim()))
            {
                RunOnUiThread(() =>
                {
                    string WebError = string.Format($"{Resources.GetString(Resource.String.s220)}");
                    DialogHelper.ShowDialogError(this, this, WebError);

                    tbPacking.RequestFocus();
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
                            string WebError = string.Format($"{Resources.GetString(Resource.String.s40)} {Resources.GetString(Resource.String.s222)}");
                            DialogHelper.ShowDialogError(this, this, WebError);

                            tbPacking.RequestFocus();
                        });
                       
                        return false;
                    }

                    var stockQty = GetStock(moveHead.GetString("Issuer"), tbIssueLocation.Text.Trim(), tbSSCC.Text.Trim(), tbSerialNum.Text.Trim(), tbIdent.Text.Trim());
                    if (Double.IsNaN(stockQty))
                    {
                        RunOnUiThread(() =>
                        {
                            string WebError = string.Format($"{Resources.GetString(Resource.String.s269)}");
                            DialogHelper.ShowDialogError(this, this, WebError);
                        });
                     

                        //  SelectNext(tbIdent);
                        return false;
                    }
                    if (Math.Abs(qty) > Math.Abs(stockQty))
                    {
                        RunOnUiThread(() =>
                        {
                            string WebError = string.Format($"{Resources.GetString(Resource.String.s278)}");
                            DialogHelper.ShowDialogError(this, this, WebError);

                            tbPacking.RequestFocus();
                        });
                      
                        return false;
                    }
                }
                catch (Exception e)
                {
                    RunOnUiThread(() =>
                    {
                        string WebError = string.Format($"{Resources.GetString(Resource.String.s220)}");
                        DialogHelper.ShowDialogError(this, this, WebError);
                        tbPacking.RequestFocus();
                    });
                  
                    return false;
                }
            }

            if (string.IsNullOrEmpty(tbUnits.Text.Trim()))
            {
                RunOnUiThread(() =>
                {
                    string WebError = string.Format($"{Resources.GetString(Resource.String.s270)}");
                    DialogHelper.ShowDialogError(this, this, WebError);
                });
              
                return false;
            }
            else
            {
                try
                {
                    var units = Convert.ToDouble(tbUnits.Text.Trim());
                    if (units == 0.0)
                    {
                        RunOnUiThread(() =>
                        {
                            string WebError = string.Format($"{Resources.GetString(Resource.String.s270)}");
                            DialogHelper.ShowDialogError(this, this, WebError);
                            tbUnits.RequestFocus();
                        });
                     
                        return false;
                    }
                }
                catch (Exception e) { 


                   RunOnUiThread(() =>
                   {
                       string WebError = string.Format($"{Resources.GetString(Resource.String.s270)}");
                       DialogHelper.ShowDialogError(this, this, WebError);
                       tbPacking.RequestFocus();
                   });
            
                    return false;
                }
            }

            if (!CommonData.IsValidLocation(moveHead.GetString("Issuer"), tbIssueLocation.Text.Trim()))
            {
                RunOnUiThread(() =>
                {
                    string WebError = string.Format($"{Resources.GetString(Resource.String.s271)}" + tbLocation.Text.Trim() + $"{Resources.GetString(Resource.String.s272)}" + moveHead.GetString("Issuer") + "!");
                    DialogHelper.ShowDialogError(this, this, WebError);
                    tbIssueLocation.RequestFocus();
                });
             
                return false;
            }

            if (!CommonData.IsValidLocation(moveHead.GetString("Receiver"), tbLocation.Text.Trim()))
            {
                RunOnUiThread(() =>
                {
                    string WebError = string.Format($"{Resources.GetString(Resource.String.s271)}" + tbLocation.Text.Trim() + $"{Resources.GetString(Resource.String.s272)}" + moveHead.GetString("Receiver") + "!");
                    DialogHelper.ShowDialogError(this, this, WebError);
                    tbLocation.RequestFocus();
                });
             
                return false;
            }

            try
            {
                
                if (moveItem == null) { moveItem = new NameValueObject("MoveItem"); }

                var dTest = moveHead.GetInt("HeadID");

                moveItem.SetInt("HeadID", moveHead.GetInt("HeadID"));
                moveItem.SetString("LinkKey", "");
                moveItem.SetInt("LinkNo", 0);
                moveItem.SetString("Ident", tbIdent.Text.Trim());
                moveItem.SetString("SSCC", tbSSCC.Text.Trim());
                moveItem.SetString("SerialNo", tbSerialNum.Text.Trim());
                moveItem.SetDouble("Packing", Convert.ToDouble(tbPacking.Text.Trim()));
                moveItem.SetDouble("Factor", Convert.ToDouble(tbUnits.Text.Trim()));
                moveItem.SetDouble("Qty", Convert.ToDouble(tbPacking.Text.Trim()) * Convert.ToDouble(tbUnits.Text.Trim()));
                moveItem.SetInt("Clerk", Services.UserID());
                moveItem.SetString("Location", tbLocation.Text.Trim());
                moveItem.SetString("IssueLocation", tbIssueLocation.Text.Trim());


                
                

                var t = true;
                string error;
                moveItem = Services.SetObject("mi", moveItem, out error);

            
                if (moveItem == null)
                {
                    RunOnUiThread(() =>
                    {
                        string WebError = string.Format($"{Resources.GetString(Resource.String.s213)}" + error);
                        DialogHelper.ShowDialogError(this, this, WebError);
                       
                    });
                    return false;
                }
                else
                {
                    InUseObjects.Invalidate("MoveItem");
                    return true;
                }
            }
            catch(Exception err)
            {
                Crashes.TrackError(err);
                return false;
            }
        }

        private async Task<bool> SaveMoveItemBatch(MorePallets obj)
        {
            try
            {       
                double doubleQuantity = Convert.ToDouble(obj.Quantity.Trim());
                moveItemBatch = new NameValueObject("MoveItem");
                moveItemBatch.SetInt("HeadID", moveHead.GetInt("HeadID"));
                moveItemBatch.SetString("LinkKey", "");
                moveItemBatch.SetInt("LinkNo", 0);
                moveItemBatch.SetString("Ident", obj.Ident.Trim());
                moveItemBatch.SetString("SSCC", obj.SSCC.Trim());
                moveItemBatch.SetString("SerialNo", obj.Serial.Trim());
                moveItemBatch.SetDouble("Packing", Convert.ToDouble(obj.Quantity.Trim()));
                moveItemBatch.SetDouble("Factor", Convert.ToDouble(tbUnits.Text.Trim()));
                moveItemBatch.SetDouble("Qty", Convert.ToDouble(doubleQuantity) * Convert.ToDouble(tbUnits.Text.Trim()));
                moveItemBatch.SetInt("Clerk", Services.UserID());
                moveItemBatch.SetString("Location", tbLocation.Text);
                moveItemBatch.SetString("IssueLocation", obj.Location.Trim());
                string error;
                moveItem = Services.SetObject("mi", moveItemBatch, out error);
           
                if (moveItem == null)
                {

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
                Crashes.TrackError(err);
                return false;
            }
        }

        private void ProcessQty()
        {
            var sscc = tbSSCC.Text.Trim();
            if (tbSSCC.Enabled && string.IsNullOrEmpty(sscc)) 
            {               
                return;           
            }

            var serialNo = tbSerialNum.Text.Trim();
            if (tbSerialNum.Enabled && string.IsNullOrEmpty(serialNo)) { 
                
                return;         
            }

            var ident = tbIdent.Text.Trim();
            if (string.IsNullOrEmpty(ident)) {
                
                return;           
            }

            var identObj = CommonData.LoadIdent(ident);
            if (identObj != null)
            {
                ident = identObj.GetString("Code");
                tbIdent.Text = ident;
            }

            if (!CommonData.IsValidLocation(moveHead.GetString("Issuer"), tbIssueLocation.Text.Trim()))
            {
                string SuccessMessage = string.Format($"{Resources.GetString(Resource.String.s273)}" + tbIssueLocation.Text.Trim() + $"{Resources.GetString(Resource.String.s27)}" + moveHead.GetString("Issuer") + "'!");
                DialogHelper.ShowDialogError(this, this, SuccessMessage);
                tbIssueLocation.RequestFocus();
                return;
            }


            var stockQty = GetStock(moveHead.GetString("Issuer"), tbIssueLocation.Text.Trim(), sscc, serialNo, ident);
            string WebError = string.Format(stockQty.ToString());

            if (!Double.IsNaN(stockQty))
            {
                tbPacking.Text = stockQty.ToString(CommonData.GetQtyPicture());
                lbQty.Text = $"{Resources.GetString(Resource.String.s40)} (" + stockQty.ToString(CommonData.GetQtyPicture()) + ")";
            }
            else
            {
                tbPacking.Text = "";
                lbQty.Text = $"{Resources.GetString(Resource.String.s40)} (?)";
            }
           
        }



        private async Task GetLocationsForGivenWarehouseReceiver(string warehouse)
        {
            try
            {
                await Task.Run(() =>
                {
                    List<string> result = new List<string>();
                    string error;
                    var locations = Services.GetObjectList("lo", out error, warehouse);
                    if (locations == null)
                    {
                        DialogHelper.ShowDialogError(this, this, $"{Resources.GetString(Resource.String.s225)}");

                    }
                    else
                    {
                        locations.Items.ForEach(x =>
                        {
                            var location = x.GetString("LocationID");
                            receiver.Add(location);
                        });

                    }
                });
            } catch
            {
                return;
            }

        }
        private Double LoadStockFromLocation(string warehouse, string location, string ident)
        {

            try
            {
                string error;
                var stock = Services.GetObject("str", warehouse + "|" + location + "|" + ident, out error);
                if (stock == null)
                {
                    string SuccessMessage = string.Format($"{Resources.GetString(Resource.String.s216)}" + error);
                    DialogHelper.ShowDialogError(this, this, SuccessMessage);
                    return Double.NaN;
                }
                else
                {
                    return stock.GetDouble("RealStock");
                }
            }
            catch (Exception err)
            {

                Crashes.TrackError(err);
                return Double.NaN;

            }
        }
        private async Task FinishMethod()
        {
            await Task.Run(async () =>
            {
                var resultAsync = SaveMoveItem().Result;
                if (resultAsync)
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
                        if (WebApp.Get("mode=finish&stock=move&print=" + Services.DeviceUser() + "&id=" + headID.ToString(), out result))
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
                                        StartActivity(typeof(MainMenuTablet));
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
                                        StartActivity(typeof(MainMenuTablet));

                                    });



                                    Dialog dialog = alert.Create();
                                    dialog.Show();
                                });

                            }
                        }
                        else
                        {
                            string SuccessMessage = string.Format($"{Resources.GetString(Resource.String.s218)}");
                            DialogHelper.ShowDialogError(this, this, SuccessMessage);
                        }
                    }
                    finally
                    {
                        RunOnUiThread(() =>
                        {
                            progress.StopDialogSync();

                        });
                    }
                }
            });
        }

        private double GetStock(string warehouse, string location, string sscc, string serialNum, string ident)
        {
            var wh = CommonData.GetWarehouse(warehouse);
            if (!wh.GetBool("HasStock"))
                if (tbSerialNum.Enabled)
                {
            
                    return LoadStockFromPAStockSerialNo(warehouse, ident, serialNum);
                }
                else
                {
                    return LoadStockFromPAStock(warehouse, ident);
                }

            else
            {
                return LoadStockFromStockSerialNo(warehouse, location, sscc, serialNum, ident);
            }

        }
      

        private Double LoadStockFromStockSerialNo(string warehouse, string location, string sscc, string serialNum, string ident)
        {

            try
            {
                string error;
                var stock = Services.GetObject("str", warehouse + "|" + location + "|" + sscc + "|" + serialNum + "|" + ident, out error);
                if (stock == null)
                {
                    string SuccessMessage = string.Format($"{Resources.GetString(Resource.String.s216)}" + error);
                    DialogHelper.ShowDialogError(this, this, SuccessMessage);

                    return Double.NaN;
                }
                else
                {
                    
                    return stock.GetDouble("RealStock");
                }
            }
            catch(Exception ex) 
            {
                Crashes.TrackError(ex);
                return Double.NaN;

            }
        }


        private Double LoadStockFromPAStock(string warehouse, string ident)
        {
            try
            {
                string error;
                var stock = Services.GetObject("pas", warehouse + "|" + ident, out error);
                if (stock == null)
                {
                    string SuccessMessage = string.Format($"{Resources.GetString(Resource.String.s216)}" + error);
                    return Double.NaN;
                }
                else
                {
                    return stock.GetDouble("Qty");
                }
            }
            catch(Exception err)
            {
                Crashes.TrackError(err);
                return Double.NaN;
            }
        }

        private Double LoadStockFromPAStockSerialNo(string warehouse, string ident, string serialNo)
        {

            try
            {

                string error;
                var stock = Services.GetObject("pass", warehouse + "|" + ident + "|" + serialNo, out error);
                if (stock == null)
                {
                    string SuccessMessage = string.Format($"{Resources.GetString(Resource.String.s216)}" + error);
                    return Double.NaN;
                }
                else
                {
                    return stock.GetDouble("Qty");
                }
            }
            catch (Exception err)
            {
                Crashes.TrackError(err);
                return Double.NaN;
            }
        }

        private void Sound()
        {
            soundPool.Play(soundPoolId, 1, 1, 0, 0, 1);

        }

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetTheme(Resource.Style.AppTheme_NoActionBar);
            SetContentView(Resource.Layout.InterWarehouseSerialOrSSCCEntryTablet);
            AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
            var _customToolbar = new CustomToolbar(this, toolbar, Resource.Id.navIcon);
            _customToolbar.SetNavigationIcon(settings.RootURL + "/Services/Logo");
            SetSupportActionBar(_customToolbar._toolbar);
            SupportActionBar.SetDisplayShowTitleEnabled(false);
            tbIdent = FindViewById<EditText>(Resource.Id.tbIdent);
            tbSSCC = FindViewById<EditText>(Resource.Id.tbSSCC);
            tbSerialNum = FindViewById<EditText>(Resource.Id.tbSerialNum);
            tbIssueLocation = FindViewById<EditText>(Resource.Id.tbIssueLocation);
            tbLocation = FindViewById<EditText>(Resource.Id.tbLocation);
            tbPacking = FindViewById<EditText>(Resource.Id.tbPacking);
            tbUnits = FindViewById<EditText>(Resource.Id.tbUnits);


            tbSSCC.InputType = Android.Text.InputTypes.ClassNumber;
  
            tbUnits.InputType = Android.Text.InputTypes.ClassNumber;
            listData = FindViewById<ListView>(Resource.Id.listData);
            // labels
            lbQty = FindViewById<TextView>(Resource.Id.lbQty);
            lbUnits = FindViewById<TextView>(Resource.Id.lbUnits);
            adapterNew = new AdapterLocation(this, items);
            listData.Adapter = adapterNew;
            // Buttons
            btSaveOrUpdate = FindViewById<Button>(Resource.Id.btSaveOrUpdate);
            wh = new NameValueObject();
            tbIdent.KeyPress += TbIdent_KeyPress;
            tbPacking.KeyPress += TbPacking_KeyPress;

            tbPacking.FocusChange += TbPacking_FocusChange;
            button1 = FindViewById<Button>(Resource.Id.button1);
            button3 = FindViewById<Button>(Resource.Id.button3);
            button5 = FindViewById<Button>(Resource.Id.button5);
            button4 = FindViewById<Button>(Resource.Id.button4);
            button6 = FindViewById<Button>(Resource.Id.button6);
            imagePNG = FindViewById<ImageView>(Resource.Id.imagePNG);
            spIssue = FindViewById<Spinner>(Resource.Id.spIssue);
            spReceive = FindViewById<Spinner>(Resource.Id.spReceive);
            color();        
            tbIdent.KeyPress += TbIdent_KeyPress1;
            spReceive.ItemSelected += SpReceive_ItemSelected;
            spIssue.ItemSelected += SpIssue_ItemSelected;
            listData.ItemClick += ListData_ItemClick;

            button6.Click += Button6_Click;
            button4.Click += Button4_Click;
            button5.Click += Button5_Click;
            button3.Click += Button3_Click;
            button1.Click += Button1_Click;
            btSaveOrUpdate.Click += BtSaveOrUpdate_Click;
            imagePNG.Visibility = ViewStates.Invisible;
            lbIdentName = FindViewById<EditText>(Resource.Id.lbIdentName);
            soundPool = new SoundPool(10, Stream.Music, 0);
            soundPoolId = soundPool.Load(this, Resource.Raw.beep, 1);  
            tbLocation.SetSelectAllOnFocus(true);
            tbSSCC.SetSelectAllOnFocus(true);
            btMorePallets = FindViewById<Button>(Resource.Id.btMorePallets);
            btMorePallets.Click += BtMorePallets_Click;
            btSaveOrUpdate.LongClick += BtSaveOrUpdate_LongClick;
            button3.LongClick += Button3_LongClick;
            if (InterWarehouseBusinessEventSetup.success == true)
            {
                string toast = string.Format(moveHead.GetString("Issuer"));
                Toast.MakeText(this, toast, ToastLength.Long).Show();
              
            }

            if (moveHead == null) { throw new ApplicationException("moveHead not known at this point?!"); }

            docTypes = CommonData.ListDocTypes("I|N");

            if (moveItem != null)
            {
                tbSerialNum.Text = moveItem.GetString("SerialNo");
                tbPacking.Text = moveItem.GetDouble("Packing").ToString();
                tbUnits.Text = moveItem.GetDouble("Factor").ToString();
                tbSSCC.Text = moveItem.GetString("SSCC");
                tbIdent.Text = moveItem.GetString("Ident");
                ProcessIdent();
                tbLocation.Text = moveItem.GetString("IssueLocation");
                tbIssueLocation.Text = moveItem.GetString("IssueLocation");
                editMode = true;
                tbSSCC.Enabled = false;
            }

            if (editMode)
            {
                tbPacking.RequestFocus();
            }
            else
            {
                tbSSCC.RequestFocus();
            }

            if (string.IsNullOrEmpty(tbUnits.Text.Trim())) { tbUnits.Text = "1"; }

            if (CommonData.GetSetting("ShowNumberOfUnitsField") == "1")
            {
                lbUnits.Visibility = ViewStates.Visible;
                tbUnits.Visibility = ViewStates.Visible;
            }
            await GetLocationsForGivenWarehouseIssuer(moveHead.GetString("Issuer"));
            await GetLocationsForGivenWarehouseReceiver(moveHead.GetString("Receiver"));
            adapterIssue = new CustomAutoCompleteAdapter<String>(this,
            Android.Resource.Layout.SimpleSpinnerItem, issuer);
            adapterIssue.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            spIssue.Adapter = adapterIssue;
            adapterReceive = new CustomAutoCompleteAdapter<String>(this, Android.Resource.Layout.SimpleSpinnerItem, receiver);
            adapterReceive.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            spReceive.Adapter = adapterReceive;
            tbSSCC.LongClick += TbSSCC_LongClick;
            tbSSCC.RequestFocus(); 
            tbPacking.InputType = Android.Text.InputTypes.ClassNumber;         
            spIssue.SetSelection(receiver.IndexOf("P01"), true);
            spReceive.SetSelection(receiver.IndexOf("P01"), true);
            showPicture();
            var _broadcastReceiver = new NetworkStatusBroadcastReceiver();
            _broadcastReceiver.ConnectionStatusChanged += OnNetworkStatusChanged;
            Application.Context.RegisterReceiver(_broadcastReceiver,
            new IntentFilter(ConnectivityManager.ConnectivityAction));


            tbSSCC.EditorAction += (sender, e) =>
            {
                if (e.ActionId == Android.Views.InputMethods.ImeAction.Done ||
                    e.Event.Action == Android.Views.KeyEventActions.Down)
                {
                    losesFocusSSCC();
                }
                else
                {
                    e.Handled = false;
                }
            };


            if(editMode)
            {
                btMorePallets.Visibility = ViewStates.Gone;
            }
        }

        private void Button3_LongClick(object sender, View.LongClickEventArgs e)
        {
            isMorePalletsMode = false;
            tbSSCC.Text = "";
            tbSerialNum.Text = "";
            tbPacking.Text = "";
            tbLocation.Text = "";
            tbIdent.Text = "";
            tbSSCC.RequestFocus();
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


        private bool UpdateTheHeadDocument(string text)
        {
            // Call the API endpoint here.
            return true;
        }

       

        private void BtMorePallets_Click(object sender, EventArgs e)
        {
            // StartActivity(typeof(MorePalletsClass));
            popupDialogMain = new Dialog(this);
            popupDialogMain.SetContentView(Resource.Layout.MorePalletsClass);
            popupDialogMain.Window.SetSoftInputMode(SoftInput.AdjustResize);
            popupDialogMain.Show();
            popupDialogMain.Window.SetLayout(LayoutParams.MatchParent, LayoutParams.WrapContent);
            btConfirm = popupDialogMain.FindViewById<Button>(Resource.Id.btConfirm);
            btExit = popupDialogMain.FindViewById<Button>(Resource.Id.btExit);
            tbSSCCpopup = popupDialogMain.FindViewById<EditText>(Resource.Id.tbSSCC);
            tbSSCCpopup.SetBackgroundColor(Android.Graphics.Color.Aqua);
            tbSSCCpopup.KeyPress += TbSSCCpopup_KeyPress;
            lvCardMore = popupDialogMain.FindViewById<ListView>(Resource.Id.lvCardMore);
            lvCardMore.ItemLongClick += LvCardMore_ItemLongClick;
            adapter = new MorePalletsAdapter(this, datax);
            lvCardMore.Adapter = adapter;
            tbSSCCpopup.LongClick += TbSSCCpopup_LongClick;
            lvCardMore.ItemSelected += LvCardMore_ItemSelected;
            btConfirm.Click += BtConfirm_Click;
            btExit.Click += BtExit_Click;
            tbSSCCpopup.RequestFocus();
        }

        private void TbSSCCpopup_LongClick(object sender, View.LongClickEventArgs e)
        {
            tbSSCCpopup.Text = ""; 
        }

        private void BtExit_Click(object sender, EventArgs e)
        {
            popupDialogMain.Dismiss();
            popupDialogMain.Hide();
        }
        private void BtConfirm_Click(object sender, EventArgs e)
        {
            if (datax.Count != 0)
            {
                string formatedString = $"{datax.Count} {Resources.GetString(Resource.String.s274)}";
                tbSSCC.Text = formatedString;
                tbSerialNum.Text = "...";
                tbIssueLocation.Text = "...";
                tbIdent.Text = "...";
                tbPacking.Text = "...";
                tbLocation.RequestFocus();
                isMorePalletsMode = true;
                isBatch = true;
                popupDialogMain.Dismiss();
                popupDialogMain.Hide();
                TransportObjectsToBackgroundListView(datax);
            } else
            {
                popupDialogMain.Dismiss();
                popupDialogMain.Hide();
            }

        }


        private async void SavePositions(List<MorePallets> datax)
        {
            progress = new ProgressDialogClass();
            progress.ShowDialogSync(this, $"{Resources.GetString(Resource.String.s262)}");
            foreach (var x in datax)
            {
           
                    await SaveMoveItemBatch(x);
         
            }
            progress.StopDialogSync();
        }

        private async void TransportObjectsToBackgroundListView(List<MorePallets> datax)
        {
            items.Clear();
            foreach (var current in datax)
            {
                await AddCurrentDataObject(current);
            }
        }




        private async Task AddCurrentDataObject(MorePallets currentDataObject)
        {
            // This async method will add the object to the list and will not block the UI.
            await Task.Run(() =>
            {
                // Update on the UI thread.
                RunOnUiThread(() =>
                {
                    var qty = Convert.ToDouble(currentDataObject.Quantity);
                    var formatted = qty.ToString(CommonData.GetQtyPicture());
                    items.Add(new LocationClass(currentDataObject.Ident, formatted, currentDataObject.Location));
                    /* Should work without problems */
                    listData.Adapter = null;
                    listData.Adapter = adapterNew;       
                });                     
           });
        }

        private void LvCardMore_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            var index = e.Position;
            var element = datax.ElementAt(index);
            string formated = $"{Resources.GetString(Resource.String.s236)} {element.SSCC}.";
            Toast.MakeText(this, formated, ToastLength.Long).Show();
        }

        private void LvCardMore_ItemLongClick(object sender, AdapterView.ItemLongClickEventArgs e)
        {
            var index = e.Position;
            DeleteFromTouch(index);
        }
        private void DeleteFromTouch(int index)
        {
            popupDialog = new Dialog(this);
            popupDialog.SetContentView(Resource.Layout.YesNoPopUp);
            popupDialog.Window.SetSoftInputMode(SoftInput.AdjustResize);
            popupDialog.Show();

            popupDialog.Window.SetLayout(LayoutParams.MatchParent, LayoutParams.WrapContent);
            popupDialog.Window.SetBackgroundDrawable(new ColorDrawable(Color.ParseColor("#081a45")));

            // Access Pop-up layout fields like below
            btnYes = popupDialog.FindViewById<Button>(Resource.Id.btnYes);
            btnNo = popupDialog.FindViewById<Button>(Resource.Id.btnNo);
            btnYes.Click += (e, ev) => { Yes(index); };
            btnNo.Click += (e, ev) => { No(index); };
        }

        private void No(int index)
        {
            popupDialog.Dismiss();
            popupDialog.Hide();
        }

        private void Yes(int index)
        {
            datax.RemoveAt(index);
            lvCardMore.Adapter = null;
            lvCardMore.Adapter = adapter;
            popupDialog.Dismiss();
            popupDialog.Hide();
        }
        private MorePallets ProcessQtyWithParams(MorePallets obj, string location)
        {
            var sscc = obj.SSCC;
            if (string.IsNullOrEmpty(sscc))
            {
                return null;
            }

            var serialNo = obj.Serial;
            if (enabledSerial && string.IsNullOrEmpty(serialNo))
            {
                return null;
            }

            var ident = obj.Ident;
            if (string.IsNullOrEmpty(ident))
            {
                return null;
            }

            var identObj = CommonData.LoadIdent(ident);
            var isEnabled = identObj.GetBool("HasSerialNumber");

            if (!CommonData.IsValidLocation(moveHead.GetString("Issuer"), location))
            {
                string SuccessMessage = string.Format($"{Resources.GetString(Resource.String.s273)}" + location + $"{Resources.GetString(Resource.String.s27)}" + moveHead.GetString("Issuer") + "'!");
                DialogHelper.ShowDialogError(this, this, SuccessMessage);


                return null;
            }

            var stockQty = GetStockWithParams(moveHead.GetString("Issuer"), location, sscc, serialNo, ident, isEnabled);
            if (!Double.IsNaN(stockQty))
            {
                obj.Quantity = stockQty.ToString(CommonData.GetQtyPicture());

            }
            else
            {
                DialogHelper.ShowDialogError(this, this, $"{Resources.GetString(Resource.String.s225)}");

            }
            return obj;

        }


        private double GetStockWithParams(string warehouse, string location, string sscc, string serialNum, string ident, bool serialEnabled)
        {
            var wh = CommonData.GetWarehouse(warehouse);
            if (!wh.GetBool("HasStock"))
                if (serialEnabled)
                {
                    return LoadStockFromPAStockSerialNo(warehouse, ident, serialNum);
                }
                else
                {
                    return LoadStockFromPAStock(warehouse, ident);
                }

            else
            {
                return LoadStockFromStockSerialNo(warehouse, location, sscc, serialNum, ident);
            }

        }
        private void TransportOneObject(string sscc)
        {
            if (!String.IsNullOrEmpty(sscc))
            {
                string error;
                var dataObject = Services.GetObject("sscc", sscc, out error);
                if (dataObject != null)
                {
                    var ident = dataObject.GetString("Ident");
                    var loadIdent = CommonData.LoadIdent(ident);
                    var name = dataObject.GetString("IdentName");
                    var serial = dataObject.GetString("SerialNo");
                    var qty = dataObject.GetString("Location");

                    var location = dataObject.GetString("Location");
                    MorePallets pallets = new MorePallets();
                    pallets.Ident = ident;
                    string idname = loadIdent.GetString("Name");
                    pallets.Location = location;
                    if (idname.Length > 20)
                    {
                        pallets.Name = idname.Trim().Substring(0, 20);
                    }
                    else
                    {
                        pallets.Name = idname;
                    }

                    pallets.Quantity = qty;
                    pallets.SSCC = sscc;
                    pallets.Serial = serial;
                 
                    pallets.friendlySSCC = pallets.SSCC;
                    

                    enabledSerial = loadIdent.GetBool("HasSerialNumber");


#nullable enable        
                    MorePallets? obj = ProcessQtyWithParams(pallets, location);
#nullable disable
                    /* Adds an object to the list. */

                    if (obj is null)
                    {
                        Toast.MakeText(this, $"{Resources.GetString(Resource.String.s240)}", ToastLength.Long).Show();
                    }
                    else
                    {
                        datax.Add(obj);                    
                    }
                }
                else
                {
                    return;
                }
            }
        }
        private MorePallets FilData(string barcode, bool transport)
        {
            instancex = new MorePallets();

            if (!String.IsNullOrEmpty(barcode) && barcode != "Scan fail")
            {
                string error;
                warehouse = moveHead.GetString("Issuer");
                var dataObject = Services.GetObject("sscc", barcode, out error);

                existsDuplicate = datax.Where(x => x.SSCC == barcode).FirstOrDefault();
                if (existsDuplicate != null)
                {
                    return null;
                }

                var parameters = new List<Services.Parameter>();
                parameters.Add(new Services.Parameter { Name = "acSSCC", Type = "String", Value = barcode });
                parameters.Add(new Services.Parameter { Name = "acWarehouse", Type = "String", Value = warehouse });
                query = $"SELECT * FROM uWMSItemBySSCCWarehouse WHERE acSSCC = @acSSCC AND acWarehouse = @acWarehouse";
                resultQuery = Services.GetObjectListBySql(query, parameters);

                if (resultQuery.Success && resultQuery.Rows.Count > 0)
                {
                    MorePallets instance = new MorePallets();
                    Row row = resultQuery.Rows[0];
                    string identValue = row.StringValue("acIdent");
                    string name = row.StringValue("acName");
                    double? qty = row.DoubleValue("anQty");
                    string location = row.StringValue("aclocation");
                    string serial = row.StringValue("acSerialNo");
                    string sscc = row.StringValue("acSSCC");
                    string warehouse = row.StringValue("acWarehouse");
                    instance.Ident = identValue;
                    instance.Name = name;
                    instance.Quantity = qty.ToString();
                    instance.Location = location;
                    instance.Serial = serial;
                    instance.SSCC = sscc;
                    instance.friendlySSCC = sscc.Substring(sscc.Length - 5);
                    datax.Add(instance);
                    if (!transport)
                    {
                        adapter.NotifyDataSetChanged();
                    }
                    return instance;
                }
                else
                {
                    return null;
                }
            }
            return null;
        }
        private void TbSSCCpopup_KeyPress(object sender, View.KeyEventArgs e)
        {
            e.Handled = false;
            if (e.KeyCode == Keycode.Enter && !String.IsNullOrEmpty(tbSSCCpopup.Text))
            {         
                FilData(tbSSCCpopup.Text, false);
                // Add your logic here 
                adapter.NotifyDataSetChanged();
            }
        }

      
        private void TbSSCC_LongClick(object sender, View.LongClickEventArgs e)
        {
            tbSSCC.Text = "";
            tbSerialNum.Text = "";
            tbPacking.Text = "";
            tbIssueLocation.Text = "";
            tbLocation.Text = "";
            tbSSCC.RequestFocus();
        }

     

        private void TbSerialNum_FocusChange(object sender, View.FocusChangeEventArgs e)
        {
            //losesFocusSSCC();
        }

        private void SpIssue_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            var element = e.Position;
            var item = issuer.ElementAt(element);

            tbIssueLocation.Text = item;
        }

        private void SpReceive_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            var element = e.Position;
            var item = receiver.ElementAt(element);

            tbLocation.Text = item;
        }

    

        private void TbIdent_KeyPress1(object sender, View.KeyEventArgs e)
        {
            if(e.KeyCode == Keycode.Enter)
            {
                ProcessIdent();
                e.Handled = true;
            } else
            {
                e.Handled = false;
            }
        }

        private void ListData_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            selected = e.Position;
            var item = items.ElementAt(selected);   
        }


        private async Task GetLocationsForGivenWarehouseIssuer(string warehouse)
        {
            try
            {
                await Task.Run(() =>
                {
                    List<string> result = new List<string>();
                    string error;
                    var issuerLocs = Services.GetObjectList("lo", out error, warehouse);
                    var debi = issuerLocs.Items.Count();
                    if (issuerLocs == null)
                    {
                        DialogHelper.ShowDialogError(this, this, $"{Resources.GetString(Resource.String.s225)}");
                    }
                    else
                    {
                        issuerLocs.Items.ForEach(x =>
                        {
                            var location = x.GetString("LocationID");
                            issuer.Add(location);
                        });
                    }
                });
            } catch
            {
                return;
            }
        }

   
        private void TbPacking_FocusChange(object sender, View.FocusChangeEventArgs e)
        {
            if (!editMode)
            {
                ProcessQty();
            }
        }

        private void TbLocation_KeyPress(object sender, View.KeyEventArgs e)
        {
            e.Handled = false;
            if (e.Event.Action == KeyEventActions.Down && e.KeyCode == Keycode.Enter)
            {
                // add your logic here 
                ProcessQty();
                e.Handled = true;
            }           
        }

        private void FillTheIdentLocationList(string ident)
        {         
            var wh = moveHead.GetString("Receiver");
            var list = GetIdentLocationList.fillItemsOfList(wh, ident);
            Fill(list);
        }

        private void Fill(List<TakeoverDocument> list)
        {
            foreach (var obj in list)
            {
                items.Add(new LocationClass { ident = obj.ident, location = obj.location, quantity = obj.quantity});
    
            }
            listData.Adapter = null;
            AdapterLocation adapter = new AdapterLocation(this, items);
            listData.Adapter = adapter;
        }

        private void TbPacking_KeyPress(object sender, View.KeyEventArgs e)
        {
            if (!editMode)
            {
                e.Handled = false;
                if (e.Event.Action == KeyEventActions.Down && e.KeyCode == Keycode.Enter)
                {
                    // Add your logic here 
                    ProcessIdent();
                    e.Handled = true;
                }
            }
            e.Handled = false;
        }

        private void TbIdent_KeyPress(object sender, View.KeyEventArgs e)
        {
            e.Handled = false;
            if (e.Event.Action == KeyEventActions.Down && e.KeyCode == Keycode.Enter)
            {
                // add your logic here 
                ProcessIdent();
                e.Handled = true;
            }
        }


        private void showPicture()
        {
            try
            {
                Android.Graphics.Bitmap show = Services.GetImageFromServer(moveHead.GetString("Receiver"));
                Drawable d = new BitmapDrawable(Resources, show);
                imagePNG.SetImageDrawable(d);
                imagePNG.Visibility = ViewStates.Visible;
                imagePNG.Click += (e, ev) => { ImageClick(d); };

            }
            catch (Exception error)
            {
                var log = error;
                return;
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
            popupDialog.Window.SetBackgroundDrawable(new ColorDrawable(Color.ParseColor("#081a45")));
            image = popupDialog.FindViewById<ZoomageView>(Resource.Id.image);
            image.SetMinimumHeight(500);
            image.SetMinimumWidth(800);
            image.SetImageDrawable(d);
            // Access Pop-up layout fields like below
     
        }

        private void PopupDialog_KeyPress(object sender, DialogKeyEventArgs e)
        {
           if(e.KeyCode == Keycode.Back)
            {
                popupDialog.Dismiss();
                popupDialog.Hide();
                popupDialog.Window.Dispose();
            }
        }

        public override void OnBackPressed()
        {

      

            base.OnBackPressed();
        }


        private void Button1_Click(object sender, EventArgs e)
        {
            var qty = tbPacking.Text;
            if (qty.Trim().StartsWith("-"))
            {
                qty = qty.Trim().Substring(1);
            }
            else
            {
                qty = "-" + qty;
            }
            tbPacking.Text = qty;
        }

        private async void BtSaveOrUpdate_Click(object sender, EventArgs e)
        {
            if (!tbIdent.Text.Contains("..."))
            {
                var resultAsync = SaveMoveItem().Result;
                if (resultAsync)
                {
                    if (editMode)
                    {
                        StartActivity(typeof(InterWarehouseEnteredPositionsViewTablet));
                        HelpfulMethods.clearTheStack(this);
                    }
                    else
                    {
                        StartActivity(typeof(InterWarehouseSerialOrSSCCEntryTablet));
                        HelpfulMethods.clearTheStack(this);
                    }
                    this.Finish();
                }
            } else
            {
                if (!editMode)
                {
                    if (String.IsNullOrEmpty(tbLocation.Text))
                    {
                        Toast.MakeText(this, $"{Resources.GetString(Resource.String.s241)}", ToastLength.Long).Show();
                        return;
                    }


                    SavePositions(datax);
                    StartActivity(typeof(InterWarehouseSerialOrSSCCEntryTablet));
                    this.Finish();

                }
            }
        }

        private async void Button3_Click(object sender, EventArgs e)
        {
            if (tbIdent.Text != "...")
            {
                var resultAsync = SaveMoveItem().Result;
                if (resultAsync)
                {
                    if (editMode)
                    {
                        StartActivity(typeof(InterWarehouseEnteredPositionsViewTablet));
                        HelpfulMethods.clearTheStack(this);
                    }
                    else
                    {
                        StartActivity(typeof(InterWarehouseSerialOrSSCCEntryTablet));
                        HelpfulMethods.clearTheStack(this);
                    }

                }
            } else
            {
                if (!editMode)
                {
                    if (String.IsNullOrEmpty(tbLocation.Text))
                    {
                        Toast.MakeText(this, $"{Resources.GetString(Resource.String.s241)}", ToastLength.Long).Show();
                        return;
                    }


                    SavePositions(datax);
                    StartActivity(typeof(InterWarehouseSerialOrSSCCEntryTablet));
                    this.Finish();

                }
            }
        }

        private async void Button5_Click(object sender, EventArgs e)
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
            if (!isBatch)
            {
                await FinishMethod();
            }
            else
            {
                await FinishMethodBatch();
            }
        }

     

        private async Task<bool> SaveMoveItemWithParams(MorePallets objectItem)
        {

            if (string.IsNullOrEmpty(objectItem.Ident.Trim()) && string.IsNullOrEmpty(objectItem.Serial) && string.IsNullOrEmpty(objectItem.Quantity.Trim()))
            {
                return true;
            }

            if (string.IsNullOrEmpty(objectItem.SSCC))
            {         
                return false;
            }


            else
            {
                try
                {
                    var qty = Convert.ToDouble(objectItem.Quantity.Trim());
                    if (qty == 0.0)
                    {
                        return false;
                    }

                    var stockQty = GetStock(moveHead.GetString("Issuer"), objectItem.Location.Trim(), objectItem.SSCC.Trim(), objectItem.Serial.Trim(), objectItem.Ident.Trim());
                    if (Double.IsNaN(stockQty))
                    {
                        //  SelectNext(tbIdent);
                        return false;
                    }
                    if (Math.Abs(qty) > Math.Abs(stockQty))
                    {

                        return false;
                    }
                }
                catch (Exception e)
                {

                    return false;
                }
            }

            if (string.IsNullOrEmpty(tbUnits.Text.Trim()))
            {
                return false;
            }
            else
            {
                try
                {
                    var units = Convert.ToDouble(tbUnits.Text.Trim());
                    if (units == 0.0)
                    {
                        RunOnUiThread(() =>
                        {
                            string WebError = string.Format($"{Resources.GetString(Resource.String.s270)}");
                            DialogHelper.ShowDialogError(this, this, WebError);

                            tbUnits.RequestFocus();
                        });

                        return false;
                    }
                }
                catch (Exception)
                {


                    RunOnUiThread(() =>
                    {
                        string WebError = string.Format($"{Resources.GetString(Resource.String.s270)}");
                        DialogHelper.ShowDialogError(this, this, WebError);

                        tbPacking.RequestFocus();
                    });

                    return false;
                }
            }

            if (!CommonData.IsValidLocation(moveHead.GetString("Issuer"), objectItem.Location.Trim()))
            {
                RunOnUiThread(() =>
                {
                    string WebError = string.Format($"{Resources.GetString(Resource.String.s271)}" + tbLocation.Text.Trim() + $"{Resources.GetString(Resource.String.s272)}" + moveHead.GetString("Issuer") + "!");
                    DialogHelper.ShowDialogError(this, this, WebError);
                    tbIssueLocation.RequestFocus();
                });

                return false;
            }

            if (!CommonData.IsValidLocation(moveHead.GetString("Receiver"), tbLocation.Text.Trim()))
            {
                RunOnUiThread(() =>
                {
                    string WebError = string.Format($"{Resources.GetString(Resource.String.s271)}" + tbLocation.Text.Trim() + $"{Resources.GetString(Resource.String.s272)}" + moveHead.GetString("Receiver") + "!");
                    DialogHelper.ShowDialogError(this, this, WebError);
                    tbLocation.RequestFocus();
                });

                return false;
            }

            try
            {
                InUseObjects.Invalidate("MoveItem");           
                moveItemCurrent = new NameValueObject("MoveItem");
                moveHead= (NameValueObject)InUseObjects.Get("MoveHead");
                moveItemCurrent.SetInt("HeadID", moveHead.GetInt("HeadID"));
                // updateTheHead();
                moveItemCurrent.SetInt("HeadID", moveHead.GetInt("HeadID"));               
                var number = moveHead.GetInt("HeadID");
                moveItemCurrent.SetString("LinkKey", "");
                moveItemCurrent.SetInt("LinkNo", 0);
                moveItemCurrent.SetString("Ident", objectItem.Ident.Trim());
                moveItemCurrent.SetString("SSCC", objectItem.SSCC.Trim());
                moveItemCurrent.SetString("SerialNo", objectItem.Serial.Trim());
                moveItemCurrent.SetDouble("Packing", Convert.ToDouble(objectItem.Quantity.Trim()));
                moveItemCurrent.SetDouble("Factor", Convert.ToDouble(tbUnits.Text.Trim()));
                moveItemCurrent.SetDouble("Qty", Convert.ToDouble(objectItem.Quantity.Trim()) * Convert.ToDouble(tbUnits.Text.Trim()));
                moveItemCurrent.SetInt("Clerk", Services.UserID());
                moveItemCurrent.SetString("Location", tbLocation.Text.Trim());
                moveItemCurrent.SetString("IssueLocation", objectItem.Location.Trim());

                string error;
                moveItemCurrent = Services.SetObject("mi", moveItem, out error);
           

                if (moveItemCurrent == null)
                {
                    RunOnUiThread(() =>
                    {
                        string WebError = string.Format($"{Resources.GetString(Resource.String.s213)}" + error);
                        DialogHelper.ShowDialogError(this, this, WebError);
       
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

                Crashes.TrackError(err);
                return false;

            }
        }

        private async Task FinishMethodBatch()
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
                            if (WebApp.Get("mode=finish&stock=move&print=" + Services.DeviceUser() + "&id=" + headID.ToString(), out result))
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
                                                Thread.Sleep(500);                                          
                                                StartActivity(typeof(MainMenuTablet));                                                                                   
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
                                          

                                        });

                                        Dialog dialog = alert.Create();
                                        dialog.Show();
                                    });

                                }
                            }
                            else
                            {
                                string SuccessMessage = string.Format($"{Resources.GetString(Resource.String.s218)}");
                                DialogHelper.ShowDialogError(this, this, SuccessMessage);
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
        private void losesFocusSSCC()
        {
            datax.Clear();

            var dataResponse = FillRelatedData(tbSSCC.Text);

            tbSerialNum.Text = "";
            tbPacking.Text = "";
            tbIssueLocation.Text = "";
            tbLocation.Text = "";
            tbSSCC.Text = tbSSCC.Text;
 
            data.Clear();
           
       
            if (dataResponse != null)
            {
                var loadIdent = CommonData.LoadIdent(dataResponse.Ident);
                string idname = loadIdent.GetString("Name");
                tbIdent.Text = dataResponse.Ident;
                lbIdentName.Text = idname;
                tbSerialNum.Text = dataResponse.Serial;
                tbIssueLocation.Text = dataResponse.Location;
                
                if (!String.IsNullOrEmpty(idname))
                {
                    ProcessQty();
                    tbSerialNum.RequestFocus();
                }
                else
                {
                    tbSSCC.Text = String.Empty;
                    return;
                }
            }
         
        }

       
        private void Button4_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(InterWarehouseEnteredPositionsViewTablet));
            HelpfulMethods.clearTheStack(this); 

        }


        private void Button6_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(MainMenuTablet));
            HelpfulMethods.clearTheStack(this);
        }





        private void ProcessIdent()
        {
            var ident = CommonData.LoadIdent(tbIdent.Text.Trim());
            if (ident == null)
            {
                tbIdent.Text = "";
                lbIdentName.Text = "";
                return;
            }

            if (CommonData.GetSetting("IgnoreStockHistory") != "1")
            {

                try
                {


                    string error;
                    var recommededLocation = Services.GetObject("rl", ident.GetString("Code") + "|" + moveHead.GetString("Receiver"), out error);
                    if (recommededLocation != null)
                    {
                        tbLocation.Text = recommededLocation.GetString("Location");
                    }
                }
                catch { }
            }

            lbIdentName.Text = ident.GetString("Name");
            tbSSCC.Enabled = ident.GetBool("isSSCC");
            tbSerialNum.Enabled = ident.GetBool("HasSerialNumber");
            FillTheIdentLocationList(ident.GetString("Code"));

        }

        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            switch (keyCode)
            {
                // in smartphone
                case Keycode.F1:
                    if (button1.Enabled == true)
                    {
                        Button1_Click(this, null);
                    }
                    break;
              


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
                    if (button5.Enabled == true)
                    {
                        Button5_Click(this, null);
                    }
                    break;


                case Keycode.F8:
                    if (button6.Enabled == true)
                    {
                        Button6_Click(this, null);
                    }
                    break;


            }
            return base.OnKeyDown(keyCode, e);
        }



    }
}