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
using Com.Jsibbold.Zoomage;
using Microsoft.AppCenter.Crashes;
using WMS.App;
using TrendNET.WMS.Core.Data;
using TrendNET.WMS.Device.App;
using TrendNET.WMS.Device.Services;
using static Android.App.ActionBar;
using WebApp = TrendNET.WMS.Device.Services.WebApp;
using AndroidX.AppCompat.App;
using AlertDialog = Android.App.AlertDialog;
using Android.Graphics;
namespace WMS
{
    [Activity(Label = "TakeOverSerialOrSSCCEntryTablet", ScreenOrientation = Android.Content.PM.ScreenOrientation.Landscape)]
    public class TakeOverSerialOrSSCCEntryTablet : CustomBaseActivity, IBarcodeResult
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
        private Button button4;
        private Button button6;
        private Button button5;
        private Button button7;
        private TextView lbQty;
        private ListView listData;
        SoundPool soundPool;
        int soundPoolId;
        private NameValueObjectList positions = null;
        private string ident;
        private ZoomageView warehousePNG;
        private List<TakeOverSerialOrSSCCEntryList> data = new List<TakeOverSerialOrSSCCEntryList>();
        private List<TakeoverDocument> items = new List<TakeoverDocument>();
        private List<TakeoverDocument> dataDocuments = new List<TakeoverDocument>();
        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetTheme(Resource.Style.AppTheme_NoActionBar);
            SetContentView(Resource.Layout.TakeOverSerialOrSSCCEntryTablet);
            Window.SetSoftInputMode(Android.Views.SoftInput.AdjustResize);
            await Update();
            AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
            var _customToolbar = new CustomToolbar(this, toolbar, Resource.Id.navIcon);
            _customToolbar.SetNavigationIcon(settings.RootURL + "/Services/Logo");
            SetSupportActionBar(_customToolbar._toolbar);
            SupportActionBar.SetDisplayShowTitleEnabled(false);
            listData = FindViewById<ListView>(Resource.Id.listData);
            fillItems();
            tbIdent = FindViewById<EditText>(Resource.Id.tbIdent);
            tbSSCC = FindViewById<EditText>(Resource.Id.tbSSCC);
            tbSerialNum = FindViewById<EditText>(Resource.Id.tbSerialNum);
            tbLocation = FindViewById<EditText>(Resource.Id.tbLocation);
            tbPacking = FindViewById<EditText>(Resource.Id.tbPacking);
            warehousePNG = FindViewById<ZoomageView>(Resource.Id.warehousePNG);
            tbIdent.InputType = Android.Text.InputTypes.ClassNumber;
            tbSSCC.InputType = Android.Text.InputTypes.ClassNumber;
            tbSerialNum.InputType = Android.Text.InputTypes.ClassNumber;
            TakeoverDocumentAdapter adapter = new TakeoverDocumentAdapter(this, items);
            btSaveOrUpdate = FindViewById<Button>(Resource.Id.btSaveOrUpdate);
            button4 = FindViewById<Button>(Resource.Id.button4);
            button6 = FindViewById<Button>(Resource.Id.button6);
            button5 = FindViewById<Button>(Resource.Id.button5);
            button7 = FindViewById<Button>(Resource.Id.button7);
            lbQty = FindViewById<TextView>(Resource.Id.lbQty);
            soundPool = new SoundPool(10, Stream.Music, 0);
            soundPoolId = soundPool.Load(this, Resource.Raw.beep, 1);
            Barcode2D barcode2D = new Barcode2D();
            barcode2D.open(this, this);
            listData.ItemClick += ListData_ItemClick;
            btSaveOrUpdate.Click += BtSaveOrUpdate_Click;
            button4.Click += Button4_Click;
            button6.Click += Button6_Click;
            button7.Click += Button7_Click;
            button5.Click += Button5_Click;
            warehousePNG.Visibility = ViewStates.Invisible;         
            if (moveHead == null) { throw new ApplicationException("moveHead not known at this point?!"); }
            if (openIdent == null) { throw new ApplicationException("openIdent not known at this point?!"); }   
            try
            {

                string error = "N/A";
                if (openOrder == null)
                {
                    editMode = moveItem != null;
                    if ((moveItem == null) || string.IsNullOrEmpty(moveItem.GetString("LinkKey")))
                    {
                        openOrder = new NameValueObject("OpenOrder");
                                    ident = openOrder.GetString("Ident");

                    }
                    else
                    {
                        editMode = true;
                        openOrder = Services.GetObject("oobl", moveItem.GetString("LinkKey") + "|" + moveItem.GetInt("LinkNo").ToString(), out error);
                        ident = openOrder.GetString("Ident");

                        if (openOrder == null)
                        {
                            Toast.MakeText(this, $"{Resources.GetString(Resource.String.s213)}" + error, ToastLength.Long).Show();
                            return;
                        }
                    }
                }

            }
            catch(Exception error)
            {
                Crashes.TrackError(error);
            }
            docTypes = CommonData.ListDocTypes("I|N");
            tbSSCC.Enabled = openIdent.GetBool("isSSCC");
            tbSerialNum.Enabled = openIdent.GetBool("HasSerialNumber");
            if (moveItem != null)
            {
                tbIdent.Text = moveItem.GetString("IdentName");
                tbSerialNum.Text = moveItem.GetString("SerialNo");
            
                if (CommonData.GetSetting("ShowNumberOfUnitsField") == "1")
                {
                    tbPacking.Text = moveItem.GetDouble("Packing").ToString();
                }
                else if (CommonData.GetSetting("ShowMorePrintsField") == "1")
                {
                    tbPacking.Text = moveItem.GetDouble("Qty").ToString();
                }
                else
                {
                    tbPacking.Text = moveItem.GetDouble("Qty").ToString();
                }

                tbSSCC.Text = moveItem.GetString("SSCC");
                tbLocation.Text = moveItem.GetString("Location");
                btSaveOrUpdate.Text = $"{Resources.GetString(Resource.String.s293)}";
            }
            else
            {
                tbIdent.Text = openIdent.GetString("Code") + " " + openIdent.GetString("Name");
            
            }
            
            lbQty.Text = $"{Resources.GetString(Resource.String.s40)} (" + openOrder.GetDouble("OpenQty").ToString(CommonData.GetQtyPicture()) + ")";

            isPackaging = openIdent.GetBool("IsPackaging");
            if (isPackaging)
            {
                tbSSCC.Enabled = false;
                tbSerialNum.Enabled = false;
                // new Scanner(tbLocation);
                tbLocation.SetBackgroundColor(Android.Graphics.Color.Aqua);
                tbPacking.RequestFocus();
            }
            else
            {
                // if (tbSSCC.Enabled) { new Scanner(tbSSCC); }
                // new Scanner(tbSerialNum);
                // new Scanner(tbLocation);

                if (tbSSCC.Enabled)
                { tbSSCC.SetBackgroundColor(Android.Graphics.Color.Aqua); }
                tbSerialNum.SetBackgroundColor(Android.Graphics.Color.Aqua);
                tbLocation.SetBackgroundColor(Android.Graphics.Color.Aqua);


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
            if (tbSSCC.Enabled && (CommonData.GetSetting("AutoCreateSSCC") == "1"))
            {
                tbSSCC.Text = CommonData.GetNextSSCC();
              
            }
            tbLocation.RequestFocus();
            FillRelatedData();
            tbSerialNum.RequestFocus();       
            tbSerialNum.RequestFocus();
            showPictureIdent(openIdent.GetString("Code"));
            // listData.PerformItemClick(listData, 0, 0);
            FillTheList();
            var _broadcastReceiver = new NetworkStatusBroadcastReceiver();
            _broadcastReceiver.ConnectionStatusChanged += OnNetworkStatusChanged;
            Application.Context.RegisterReceiver(_broadcastReceiver,
            new IntentFilter(ConnectivityManager.ConnectivityAction));
        }
        public bool IsOnline()
        {
            var cm = (ConnectivityManager)GetSystemService(ConnectivityService);
            return cm.ActiveNetworkInfo == null ? false : cm.ActiveNetworkInfo.IsConnected;
        }

        private async Task Update()
        {
            try
            {
                await Task.Run(() =>
                {
                    if (openOrder != null && openIdent != null && moveHead != null)
                    {
                        string error;
                        var openOrders = Services.GetObjectList("oo", out error, openIdent.GetString("Code") + "|" + moveHead.GetString("DocumentType"));
                        openOrder = openOrders.Items.Where(x => x.GetString("LinkNo") == openOrder.GetString("LinkNo")).FirstOrDefault();
                    }
                });
            }
            catch (Exception err)
            {
                Crashes.TrackError(err);
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
        private void fillListAdapter()
        {

            for (int i = 0; i < positions.Items.Count; i++)
            {
                if (i < positions.Items.Count && positions.Items.Count > 0)
                {
                    var item = positions.Items.ElementAt(i);
                    var created = item.GetDateTime("DateInserted");
                    var numbering = i + 1;
                    bool setting;

                    if (CommonData.GetSetting("ShowNumberOfUnitsField") == "1")
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
                    dataDocuments.Add(new TakeoverDocument
                    {
                        ident = item.GetString("Ident"),
                        serial = item.GetString("SerialNo"),
                        sscc = item.GetString("SSCC"),
                        location = item.GetString("Location"),
                        quantity =tempUnit,
                    });
                    
                }
                else
                {
                    string errorWebApp = string.Format($"{Resources.GetString(Resource.String.s247)}");
                    Toast.MakeText(this, errorWebApp, ToastLength.Long).Show();
                }

            }
            TakeoverDocumentAdapter adapter = new TakeoverDocumentAdapter(this, dataDocuments);
            listData.Adapter = null;    listData.Adapter = adapter; ;
        }
        private void FillTheList()
        { 
                try
                {

                    if (positions == null)
                    {
                        var error = "";

                        if (positions == null)
                        {
                            positions = Services.GetObjectList("mi", out error, moveHead.GetInt("HeadID").ToString());
                            InUseObjects.Set("TakeOverEnteredPositions", positions);
                        }
                        if (positions == null)
                        {
                            Toast.MakeText(this, $"{Resources.GetString(Resource.String.s213)}" + error, ToastLength.Long).Show();

                            return;
                        }
                    }            
                }
                finally
                {
                    fillListAdapter();
                }
        }

        private void WarehousePNG_Click(object sender, EventArgs e)
        {
            popupDialog = new Dialog(this);
            popupDialog.SetContentView(Resource.Layout.WarehousePicture);
            popupDialog.Window.SetSoftInputMode(SoftInput.AdjustResize);
            popupDialog.Show();
            popupDialog.Window.SetLayout(LayoutParams.MatchParent, LayoutParams.WrapContent);
            popupDialog.Window.SetBackgroundDrawable(new ColorDrawable(Color.ParseColor("#081a45")));
            image = popupDialog.FindViewById<ZoomageView>(Resource.Id.image);
            popupDialog.KeyPress += PopupDialog_KeyPress;
            
        }


        private void PopupDialog_KeyPress(object sender, DialogKeyEventArgs e)
        {
            if (e.KeyCode == Keycode.Back)
            {
                popupDialog.Dismiss();
                popupDialog.Hide();
             
            }
        }


        public override void OnBackPressed()
        {         
            base.OnBackPressed();
        }

     

 

        private void Fill(List<TakeoverDocument> list)
        {
            foreach(TakeoverDocument obj in list)
            {
                items.Add(obj);
            }

            listData.Adapter = null;



            TakeoverDocumentAdapter adapter = new TakeoverDocumentAdapter(this, items);
            listData.Adapter = adapter;
            
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
                                        Thread.Sleep(500);
                                        StartActivity(typeof(MainMenuTablet));

                                    });



                                    Dialog dialog = alert.Create();
                                    dialog.Show();
                                });

                            }
                        }
                        else
                        {
                            Toast.MakeText(this, $"{Resources.GetString(Resource.String.s218)}" + result, ToastLength.Long).Show();
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

      
        private void showPicture()
        {
            try
            {
                Android.Graphics.Bitmap show = Services.GetImageFromServer(moveHead.GetString("Wharehouse"));
                var debug = moveHead.GetString("Wharehouse");
                Drawable d = new BitmapDrawable(Resources, show);
                warehousePNG.SetImageDrawable(d);
                warehousePNG.Visibility = ViewStates.Visible;
                warehousePNG.Click += (e, ev) => { ImageClick(d); };

            } catch(Exception)
            {
                return;
            }            
        }



        private void showPictureIdent(string ident)
        {
            try
            {
                Bitmap show = Services.GetImageFromServerIdent(moveHead.GetString("Wharehouse"), ident);
                var debug = moveHead.GetString("Wharehouse");
                Drawable d = new BitmapDrawable(Resources, show);
                warehousePNG.SetImageDrawable(d);
                warehousePNG.Visibility = ViewStates.Visible;
                warehousePNG.Click += (e, ev) => { ImageClick(d); };
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
            popupDialog.KeyPress += PopupDialog_KeyPress;

            popupDialog.Window.SetLayout(LayoutParams.MatchParent, LayoutParams.WrapContent);
            popupDialog.Window.SetBackgroundDrawable(new ColorDrawable(Color.ParseColor("#081a45")));
            image = popupDialog.FindViewById<ZoomageView>(Resource.Id.image);
            image.SetMinimumHeight(500);
            image.SetMinimumWidth(800);
            image.SetImageDrawable(d);
            // Access Pop up layout fields like below
        }

        private void BtnOK_Click1(object sender, EventArgs e)
        { 

            popupDialog.Dismiss();
            popupDialog.Dispose();
           
        }

        private void ListData_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            //selected = e.Position;
            //var item = items.ElementAt(selected);
            //tbLocation.Text = item.location;
            //listData.SetItemChecked(selected, true);
            //listData.SetSelection(selected);
        }
  
        private void fillItems()
        {
         
           string error;
            var stock = Services.GetObjectList("str", out error, moveHead.GetString("Wharehouse") + "||" + ident); /* Defined at the beggining of the activity. */
            var number = stock.Items.Count();

          
            if (stock != null)
            {
                stock.Items.ForEach(x =>
                {
                    data.Add(new TakeOverSerialOrSSCCEntryList
                    {
                        Ident = x.GetString("Ident"),
                        Location = x.GetString("Location"),
                        Qty = x.GetDouble("RealStock").ToString(),
                        SerialNumber = x.GetString("SerialNo")
                       
                    });
                });

            }

            TakeOverSerialOrSSCCEntryAdapter adapter = new TakeOverSerialOrSSCCEntryAdapter(this, data);

            listData.Adapter = null;
            listData.Adapter = adapter;

   
        }



        private void Button5_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(TakeOverEnteredPositionsViewTablet));
            HelpfulMethods.clearTheStack(this);
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

        private void Sound()
        {
            soundPool.Play(soundPoolId, 1, 1, 0, 0, 1);
        }



        private void Button7_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(MainMenuTablet));
            HelpfulMethods.clearTheStack(this);
        }


        private async void Button4_Click(object sender, EventArgs e)
        {
            var resultAsync = SaveMoveItem().Result;
            if (resultAsync)
            {
                StartActivity(typeof(TakeOverIdentEntryTablet));
                HelpfulMethods.clearTheStack(this);
            }
        }


        private async void BtSaveOrUpdate_Click(object sender, EventArgs e)
        {
            var resultAsync = SaveMoveItem().Result;
            if (resultAsync)
            {
                if (editMode)
                {
                    StartActivity(typeof(TakeOverEnteredPositionsViewTablet));
                    HelpfulMethods.clearTheStack(this);
                }
                else
                {
                    StartActivity(typeof(TakeOverSerialOrSSCCEntryTablet));
                    HelpfulMethods.clearTheStack(this);
                }

            }
        }

        private static bool? checkTakeOverOpenQty = null;
        private int selected;
        private ProgressDialogClass progress;
        private Dialog popupDialog;
        private ZoomageView image;
        private Button btnOK;
        private string tempUnit;
        private Dialog popupDialogConfirm;
        private Button btnYesConfirm;
        private Button btnNoConfirm;

        private async void Button6_Click(object sender, EventArgs e)
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
            await FinishMethod();
        }

        private void FillRelatedData()
        {
            string error;

            var data = Services.GetObject("sscc", tbSSCC.Text, out error);
            if (data != null)
            {
                if (tbSerialNum.Enabled == true)
                {
                    var serial = data.GetString("SerialNo");
                    tbSerialNum.Text = serial;

                }
                else
                {
                    return;
                }
            }
            else
            {
                return;
            }
        }
        private bool CheckTakeOverOpenQty()
        {
            if (checkTakeOverOpenQty == null)
            {


                try
                {
                    string error;
                    var useObj = Services.GetObject("ctooqUse", "", out error);
                    checkTakeOverOpenQty = useObj == null ? false : useObj.GetBool("Use");
                }
                catch (Exception err)
                {

                    Crashes.TrackError(err);
                    return false;

                }
            }
            return (bool)checkTakeOverOpenQty;
        }
        


        private async Task<bool> SaveMoveItem()
        {
            if (string.IsNullOrEmpty(tbSSCC.Text.Trim()) && string.IsNullOrEmpty(tbSerialNum.Text.Trim()) && string.IsNullOrEmpty(tbPacking.Text.Trim()))
            {

                return true;
            }

            if (tbSSCC.Enabled && string.IsNullOrEmpty(tbSSCC.Text.Trim()))
            {
                RunOnUiThread(() =>
                {
                    string errorWebAppIssued = string.Format($"{Resources.GetString(Resource.String.s270)}");
                    Toast.MakeText(this, errorWebAppIssued, ToastLength.Long).Show();
                    tbSSCC.RequestFocus();
                });

                return false;
            }

            if (tbSerialNum.Enabled && openIdent.GetBool("HasSerialNumber"))
            {
                if (string.IsNullOrEmpty(tbSerialNum.Text.Trim()))
                {
                    RunOnUiThread(() =>
                    {
                        string errorWebAppIssued = string.Format($"{Resources.GetString(Resource.String.s270)}");
                        Toast.MakeText(this, errorWebAppIssued, ToastLength.Long).Show();
                        tbSerialNum.RequestFocus();
                    });

                    return false;
                }
            }

            if (string.IsNullOrEmpty(tbPacking.Text.Trim()))
            {
                RunOnUiThread(() =>
                {
                    string errorWebAppIssued = string.Format($"{Resources.GetString(Resource.String.s270)}");
                    Toast.MakeText(this, errorWebAppIssued, ToastLength.Long).Show();
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
                            string errorWebAppIssued = string.Format($"{Resources.GetString(Resource.String.s270)}");
                            Toast.MakeText(this, errorWebAppIssued, ToastLength.Long).Show();
                            tbPacking.RequestFocus();
                        });

                        return false;
                    }

                    if (moveHead.GetBool("ByOrder") && !isPackaging && CheckTakeOverOpenQty())
                    {
                        var tolerance = openIdent.GetDouble("TolerancePercent");
                        var max = Math.Abs(openOrder.GetDouble("OpenQty") * (1.0 + tolerance / 100));
                        if (Math.Abs(qty) > max)
                        {
                            RunOnUiThread(() =>
                            {
                                string errorWebAppIssued = string.Format($"{Resources.GetString(Resource.String.s40)} (" + qty.ToString(CommonData.GetQtyPicture()) + ") ne sme presegati max. količine (" + max.ToString(CommonData.GetQtyPicture()) + ")!");
                                Toast.MakeText(this, errorWebAppIssued, ToastLength.Long).Show();

                                tbPacking.RequestFocus();
                            });

                            return false;
                        }
                    }
                }
                catch (Exception)
                {
                    RunOnUiThread(() =>
                    {
                        string errorWebAppIssued = string.Format($"{Resources.GetString(Resource.String.s220)}");
                        Toast.MakeText(this, errorWebAppIssued, ToastLength.Long).Show();

                        tbPacking.RequestFocus();
                    });

                    return false;
                }
            }



            if (!CommonData.IsValidLocation(moveHead.GetString("Wharehouse"), tbLocation.Text.Trim()))
            {
                RunOnUiThread(() =>
                {
                    string errorWebAppIssued = string.Format($"{Resources.GetString(Resource.String.s258)} '" + tbLocation.Text.Trim() + $"' {Resources.GetString(Resource.String.s272)} '" + moveHead.GetString("Wharehouse") + "'!");
                    Toast.MakeText(this, errorWebAppIssued, ToastLength.Long).Show();

                    tbLocation.RequestFocus();
                });

                return false;
            }


            try
            {


                if (moveItem == null) { moveItem = new NameValueObject("MoveItem"); }
                moveItem.SetInt("HeadID", moveHead.GetInt("HeadID"));
                moveItem.SetString("LinkKey", openOrder.GetString("Key"));
                moveItem.SetInt("LinkNo", openOrder.GetInt("No"));
                moveItem.SetString("Ident", openIdent.GetString("Code"));
                moveItem.SetString("SSCC", tbSSCC.Text.Trim());
                moveItem.SetString("SerialNo", tbSerialNum.Text.Trim());

                if (CommonData.GetSetting("ShowNumberOfUnitsField") == "1")
                {
                    moveItem.SetDouble("Packing", Convert.ToDouble(tbPacking.Text.Trim()));
                    moveItem.SetDouble("Factor", 1);
                    moveItem.SetDouble("Qty", Convert.ToDouble(tbPacking.Text.Trim()));
                    moveItem.SetInt("MorePrints", 0);
                }
                else if (CommonData.GetSetting("ShowMorePrintsField") == "1")
                {
                    moveItem.SetDouble("Packing", 0.0);
                    moveItem.SetDouble("Factor", 1.0);
                    moveItem.SetDouble("Qty", Convert.ToDouble(tbPacking.Text.Trim()));
                    moveItem.SetInt("MorePrints", 1);
                }
                else
                {
                    moveItem.SetDouble("Packing", 0.0);
                    moveItem.SetDouble("Factor", 1.0);
                    moveItem.SetDouble("Qty", Convert.ToDouble(tbPacking.Text.Trim()));
                    moveItem.SetInt("MorePrints", 0);
                }

                moveItem.SetInt("Clerk", Services.UserID());
                moveItem.SetString("Location", tbLocation.Text.Trim());

                moveItem.SetBool("PrintNow", CommonData.GetSetting("ImmediatePrintOnReceive") == "1");
                moveItem.SetInt("UserID", Services.UserID());
                moveItem.SetString("DeviceID", WMSDeviceConfig.GetString("ID", ""));

                string error;
                moveItem = Services.SetObject("mi", moveItem, out error);
                if (moveItem == null)
                {
                    RunOnUiThread(() =>
                    {
                        string errorWebAppIssued = string.Format($"{Resources.GetString(Resource.String.s213)}" + error);
                        Toast.MakeText(this, errorWebAppIssued, ToastLength.Long).Show();

                        tbLocation.RequestFocus();
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

        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
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
                    if (button4.Enabled == true)
                    {
                        Button4_Click(this, null);
                    }
                    break;

                case Keycode.F4:
                    if (button6.Enabled == true)
                    {
                        Button6_Click(this, null);
                    }
                    break;

                case Keycode.F5:
                    if (button5.Enabled == true)
                    {
                        Button5_Click(this, null);
                    }
                    break;

                case Keycode.F8:
                    if (button7.Enabled == true)
                    {
                        Button7_Click(this, null);
                    }
                    break;

            }
            return base.OnKeyDown(keyCode, e);
        }

        public void GetBarcode(string barcode)
        {
            if (tbSSCC.HasFocus)
            {
                Sound();
                tbSSCC.Text = barcode;
                tbSerialNum.RequestFocus();


            }
            else if (tbSerialNum.HasFocus)
            {
                Sound();
                tbSerialNum.Text = barcode;
                tbLocation.RequestFocus();

            }
            else if (tbLocation.HasFocus)
            {

                Sound();
                tbSerialNum.Text = barcode;
                tbPacking.RequestFocus();
            }
        }
    }


}

