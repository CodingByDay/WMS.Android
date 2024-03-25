using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Media;
using Android.Net;
using Android.OS;
using Android.Preferences;
using Android.Runtime;
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
using Xamarin.Essentials;
using static Android.App.DownloadManager;
using WebApp = TrendNET.WMS.Device.Services.WebApp;
using WMS.Printing;
using Timer = System.Timers.Timer;
using Stream = Android.Media.Stream;

using AndroidX.AppCompat.App;
using AlertDialog = Android.App.AlertDialog;
namespace WMS
{
    [Activity(Label = "InventoryProcess", ScreenOrientation = ScreenOrientation.Portrait)]
    public class InventoryProcess : CustomBaseActivity, IBarcodeResult
    {
        private Spinner cbWarehouse;
        private EditText tbLocation;
        private EditText tbIdent;
        private EditText tbTitle;
        private EditText tbSSCC;
        private EditText tbSerialNum;
        private EditText tbPacking;
        private EditText tbUnits;
        private List<ComboBoxItem> warehouseAdapter = new List<ComboBoxItem>();
        private Button btPrint;
        private Button button1;
        private Button btDelete;
        private Button button2;
        private static string selectedWarehouse = "";
        private NameValueObject moveItem = null;
        private TextView lbUnits;
        private TextView lbPacking;
        SoundPool soundPool;
        int soundPoolId;
        private int temporaryPosWarehouse;
        private List<String> identData = new List<string>();
        private CustomAutoCompleteAdapter<string> locationAdapter;
        private List<string> locationData = new List<string>();
        private List<string> returnList;
        private string guided;
        private bool afterSerial;
        private Timer aTimer;
        private string chooseThis;
        private CustomAutoCompleteAdapter<ComboBoxItem> adapterIssue;
        private int counterSelect = 0;
        private int countIdent = 0;
        private NameValueObject moveItemInner = new NameValueObject();
        private double qtyItem;
        private List<string> savedIdents;
        private Row dataObject;
        private TextView warehouseLabel;
        public CustomAutoCompleteAdapter<string> DataAdapterLocation { get; private set; }

        protected async override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetTheme(Resource.Style.AppTheme_NoActionBar);
            // Create your application here
            SetContentView(Resource.Layout.InventoryProcess);
            AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
            var _customToolbar = new CustomToolbar(this, toolbar, Resource.Id.navIcon);
            _customToolbar.SetNavigationIcon(settings.RootURL + "/Services/Logo");
            SetSupportActionBar(_customToolbar._toolbar);
            SupportActionBar.SetDisplayShowTitleEnabled(false);
            cbWarehouse = FindViewById<Spinner>(Resource.Id.cbWarehouse);
            tbLocation = FindViewById<EditText>(Resource.Id.tbLocation);
            tbIdent = FindViewById<EditText>(Resource.Id.tbIdent);
            tbTitle = FindViewById<EditText>(Resource.Id.tbTitle);
            tbSSCC = FindViewById<EditText>(Resource.Id.tbSSCC);
            tbSerialNum = FindViewById<EditText>(Resource.Id.tbSerialNum);
            tbPacking = FindViewById<EditText>(Resource.Id.tbPackingQty);
            tbPacking.Focusable = true;
            tbUnits = FindViewById<EditText>(Resource.Id.tbUnits);
            btPrint = FindViewById<Button>(Resource.Id.btPrint);
            button1 = FindViewById<Button>(Resource.Id.button1);
            btDelete = FindViewById<Button>(Resource.Id.btDelete);
            button2 = FindViewById<Button>(Resource.Id.button2);
            lbUnits = FindViewById<TextView>(Resource.Id.lbUnits);
            lbPacking = FindViewById<TextView>(Resource.Id.lbPacking);
            tbTitle.Focusable= false;
            soundPool = new SoundPool(10, Stream.Music, 0);
            soundPoolId = soundPool.Load(this, Resource.Raw.beep, 1);
            cbWarehouse.ItemSelected += CbWarehouse_ItemSelected;
            btPrint.Click += BtPrint_Click;
            button1.Click += Button1_Click;
            btDelete.Click += BtDelete_Click;
            button2.Click += Button2_Click;
            tbPacking.SetSelectAllOnFocus(true);
            tbIdent.FocusChange += TbIdent_FocusChange;
            tbSSCC.KeyPress += TbSSCC_KeyPress;
            tbIdent.KeyPress += TbIdent_KeyPress;
            Barcode2D barcode2D = new Barcode2D();
            barcode2D.open(this, this);
            await FillWarehouses();
            warehouseLabel = FindViewById<TextView>(Resource.Id.warehouseLabel);
            if (string.IsNullOrEmpty(tbUnits.Text.Trim())) { tbUnits.Text = "1"; }
            if (CommonData.GetSetting("ShowNumberOfUnitsField") == "1")
            {
                lbUnits.Visibility = ViewStates.Visible;
                tbUnits.Visibility = ViewStates.Visible;
            }
            adapterIssue = new CustomAutoCompleteAdapter<ComboBoxItem>(this,
            Android.Resource.Layout.SimpleSpinnerItem, warehouseAdapter);
            adapterIssue.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            cbWarehouse.Adapter = adapterIssue;
            color();
          
            var _broadcastReceiver = new NetworkStatusBroadcastReceiver();
            _broadcastReceiver.ConnectionStatusChanged += OnNetworkStatusChanged;
            Application.Context.RegisterReceiver(_broadcastReceiver,
            new IntentFilter(ConnectivityManager.ConnectivityAction));
           
            if (CommonData.GetSetting("AutoCreateSSCC") == "1")
            {
                tbSSCC.RequestFocus();
            }
            else
            {
                tbLocation.RequestFocus();
                await LastTransaction();
            }
        }



        private void TbSSCC_KeyPress(object sender, View.KeyEventArgs e)
        {
           if(e.KeyCode == Keycode.Enter && e.Event.Action == 0) {
                
                if (CommonData.GetSetting("AutoCreateSSCC") == "1" && tbSSCC.Text != string.Empty)
                {      
                    string error;
                    dataObject = GetSSCCData(tbSSCC.Text);
                    if (dataObject.Items != null)
                    {
                        var ident = dataObject.StringValue("acIdent");
                        var loadIdent = CommonData.LoadIdent(ident);
                        string idname = loadIdent.GetString("Name");
                        if (string.IsNullOrEmpty(ident)) { return; }
                        if (loadIdent != null)
                        {
                            tbSerialNum.Enabled = loadIdent.GetBool("HasSerialNumber");
                        }
                        var serial = dataObject.StringValue("acSerialNo");
                        var location = dataObject.StringValue("aclocation");
                        var warehouse = dataObject.StringValue("acWarehouse");
                        var tt = warehouseAdapter;
                        cbWarehouse.SetSelection(warehouseAdapter.IndexOf(warehouseAdapter.Where(x => x.ID == warehouse).FirstOrDefault()), true);
                        tbIdent.Text = ident;
                        tbLocation.Text = location;
                        tbSerialNum.Text = serial;
                        if (loadIdent.GetBool("HasSerialNumber"))
                        {
                            tbSerialNum.Text = serial;
                            tbSerialNum.SetBackgroundColor(Android.Graphics.Color.Aqua);
                        }
                        else
                        {
                            tbSerialNum.SetBackgroundColor(Android.Graphics.Color.Red);
                        }
                        tbLocation.Text = location;
                        tbTitle.Text = idname;

                        
                        e.Handled = true;                      
                    }
                    else
                    {
                        tbSSCC.Text = string.Empty;
                    }

                }       
            } else
            {
                if (e.KeyCode != Keycode.Enter)
                {
                    e.Handled = false;
                } else
                {
                    e.Handled = true;
                    ProcessStock();
                    tbPacking.RequestFocus();
                }
            }
        }

        private async Task LastTransaction()
        {
            await Task.Run(() =>
            {
                try
                {
                    if (settings.lastLocation != string.Empty && settings.lastWarehouse != string.Empty)
                    {
                        RunOnUiThread(() =>
                        {
                            var element = warehouseAdapter.Where(x => x.ID == settings.lastWarehouse).FirstOrDefault();
                            if (element != null)
                            {
                                cbWarehouse.SetSelection(warehouseAdapter.IndexOf(element), true);
                            }
                            tbLocation.Text = settings.lastLocation;
                        });
                    }
                } catch (Exception ex) {
                    Crashes.TrackError(ex);
                } 
            });      
        }

        private async Task FillWarehouses()
        {
           await Task.Run(() =>
           {
               var warehouses = CommonData.ListWarehouses();

               if (warehouses != null)
               {
                   warehouses.Items.ForEach(wh =>
                   {
                       if (checkDocument(wh.GetString("Subject")))
                       {
                           warehouseAdapter.Add(new ComboBoxItem { ID = wh.GetString("Subject"), Text = wh.GetString("Name") });
                       }
                   });
                   if (!string.IsNullOrEmpty(selectedWarehouse))
                   {
                       RunOnUiThread(() =>
                       {
                           ComboBoxItem.Select(cbWarehouse, warehouseAdapter, selectedWarehouse);
                           tbLocation.RequestFocus();
                       });
                     
                   }
               }
           });
        }



        private Row GetSSCCData(string sscc)
        {

            Row row = new Row();
            var parameters = new List<Services.Parameter>();
            parameters.Add(new Services.Parameter { Name = "acSSCC", Type = "String", Value = sscc });
            var query = $"SELECT * FROM uWMSItemBySSCCWarehouse WHERE acSSCC=@acSSCC;";
            var result = Services.GetObjectListBySql(query, parameters);

            if (result.Success && result.Rows.Count > 0)
            {
                MorePallets instance = new MorePallets();
                row = result.Rows[0];                             
            }

            return row;
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

        private void TbIdent_FocusChange(object sender, View.FocusChangeEventArgs e)
        {
        
            ProcessLocation();

            if (CommonData.GetSetting("AutoCreateSSCC") != "1")
            {
                var loadIdent = CommonData.LoadIdent(tbIdent.Text);

                if (loadIdent != null)
                {
                    tbSerialNum.Enabled = loadIdent.GetBool("HasSerialNumber");


                    if (loadIdent.GetBool("HasSerialNumber"))
                    {
                        afterSerial = true;
                    }
                    else
                    {
                        if (tbIdent.Text != string.Empty)
                        {
                            ProcessStock();       
                        }

                    }
                }
            }
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(MainMenu));
            HelpfulMethods.clearTheStack(this);
        }

        private void BtDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (moveItem != null)
                {
                    string result;
                    if (WebApp.Get("mode=delMoveItem&item=" + moveItem.GetInt("ItemID").ToString(), out result))
                    {
                        if (result == "OK!")
                        {
                            Toast.MakeText(this, "Pozicija pobrisana!", ToastLength.Long).Show();

                            StartActivity(typeof(InventoryProcess));
                            HelpfulMethods.clearTheStack(this);

                        }
                        else
                        {
                            Toast.MakeText(this, $"{Resources.GetString(Resource.String.s212)}" + result, ToastLength.Long).Show();

                            return;
                        }
                    }
                    else
                    {
                        Toast.MakeText(this, $"{Resources.GetString(Resource.String.s213)}" + result, ToastLength.Long).Show();

                        return;
                    }
                }

            }
            catch(Exception ex) 
            {
                Crashes.TrackError(ex);
            }
        }

        private string LoadStock(string warehouse, string location, string sscc, string serialNum, string ident)
        {
        
                string error;
                NameValueObject stock = new NameValueObject();
                if (!String.IsNullOrEmpty(serialNum) && !String.IsNullOrEmpty(sscc))
                {
                    stock = Services.GetObject("str", warehouse + "|" + location + "|" + sscc + "|" + serialNum + "|" + ident, out error);
                }
                else if (!String.IsNullOrEmpty(sscc) && String.IsNullOrEmpty(serialNum))
                {
                    stock = Services.GetObject("str", warehouse + "|" + location + "|" + sscc + "||" + ident, out error);

                }
                else if (String.IsNullOrEmpty(sscc) && !String.IsNullOrEmpty(serialNum))
                {
                    stock = Services.GetObject("str", warehouse + "|" + location + "||" + serialNum + "|" + ident, out error);

                }
                else
                {
                    stock = Services.GetObject("str", warehouse + "|" + location + "|||" + ident, out error);

                }

                return stock.GetDouble("RealStock").ToString(CommonData.GetQtyPicture());

        }



        private bool checkDocument(string id)
        {
            try
            {
                string result;
                if (WebApp.Get("mode=getInventoryHead&wh=" + id, out result))
                {
                    int headID = -1;
                    try
                    {
                        headID = Convert.ToInt32(result);

                    }
                    catch (Exception ex)
                    {

                        return false;
                    }

                    if (headID < 0)
                    {

                        return false;
                    }
                }
                return true;

            }
            catch
            {
                return false;
            }
        }


        private void Button1_Click(object sender, EventArgs e)
        {
            double packing, units, qty;
            ComboBoxItem warehouse;
            string location;
            string ident;
            string serNo;
            string sscc;
            if (!CheckData(out packing, out units, out qty, out warehouse, out location, out ident, out serNo, out sscc)) { return; }
            try
            {
                string result;
                if (WebApp.Get("mode=getInventoryHead&wh=" + warehouse.ID, out result))
                {
                    int headID = -1;

                    try
                    {
                        headID = Convert.ToInt32(result);
                    }

                    catch (Exception ex)
                    {
                        Toast.MakeText(this, $"{Resources.GetString(Resource.String.s216)}" + ex.Message + "): " + result, ToastLength.Long).Show();
                        return;
                    }

                    if (headID < 0)
                    {
                        cbWarehouse.SetSelection(-1);
                        Toast.MakeText(this, $"{Resources.GetString(Resource.String.s285)}", ToastLength.Long).Show();
                        return;
                    }

                    if (moveItem == null)
                    {
                        moveItem = Services.GetObject("miissl", headID.ToString() + "|" + ident + "|" + serNo + "|" + sscc + "|" + location, out result);
                    }

                    if (moveItem == null)
                    {
                        moveItem = new NameValueObject("MoveItem");
                    }

                    moveItem.SetInt("HeadID", headID);
                    moveItem.SetString("LinkKey", "");
                    moveItem.SetInt("LinkNo", 0);
                    moveItem.SetString("Ident", ident);
                    moveItem.SetString("SerialNo", serNo);
                    moveItem.SetDouble("Packing", packing);
                    moveItem.SetDouble("Factor", units);
                    moveItem.SetDouble("Qty", qty);
                    moveItem.SetString("SSCC", sscc);
                    moveItem.SetString("Location", location);
                    moveItem.SetInt("Clerk", Services.UserID());
                    moveItem = Services.SetObject("mi", moveItem, out result);

                    if (moveItem == null)
                    {
                        Toast.MakeText(this, $"{Resources.GetString(Resource.String.s213)}" + result, ToastLength.Long).Show();

                        return;
                    }
                    else
                    {
                        selectedWarehouse = warehouse.ID;
                        StartActivity(typeof(InventoryProcess));
                        HelpfulMethods.clearTheStack(this);
                        settings.lastWarehouse = warehouse.ID;
                        settings.lastLocation= location;            
                    }
                }
                else
                {
                    Toast.MakeText(this, $"{Resources.GetString(Resource.String.s213)}" + result, ToastLength.Long).Show();
                    return;
                }
            }
            catch(Exception error)
            {
                Crashes.TrackError(error);
                return;
            }
        }

        private void BtPrint_Click(object sender, EventArgs e)
        {
            double packing, units, qty;
            ComboBoxItem warehouse;
            string location;
            string ident;
            string serNo;
            string sscc;
            if (!CheckData(out packing, out units, out qty, out warehouse, out location, out ident, out serNo, out sscc)) { return; }
            try
            {

                var nvo = new NameValueObject("PrintInventoryProcess");
                PrintingCommon.SetNVOCommonData(ref nvo);
                nvo.SetDouble("Packing", packing);
                nvo.SetDouble("Factor", units);
                nvo.SetDouble("Qty", qty);
                nvo.SetString("Warehouse", warehouse.ID);
                nvo.SetString("Location", location);
                nvo.SetString("Ident", ident);
                nvo.SetString("SerialNo", serNo);
                nvo.SetString("SSCC", sscc);
                PrintingCommon.SendToServer(nvo);
            }
            catch(Exception error)
            {
                Crashes.TrackError(error);
                return;
            }

        }
       

        private async void CbWarehouse_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            temporaryPosWarehouse = e.Position;
            warehouseLabel.Text = $"{Resources.GetString(Resource.String.s28)}: " + warehouseAdapter.ElementAt(temporaryPosWarehouse);
        }

        private void ProcessIdent()
        {
            var ident = tbIdent.Text.Trim();
            if (string.IsNullOrEmpty(ident)) { return; }
            var identObj = CommonData.LoadIdent(ident);
            if (identObj != null)
            {
                tbIdent.Text = identObj.GetString("Code");
                tbTitle.Text = identObj.GetString("Name");
                tbSSCC.Enabled = identObj.GetBool("isSSCC");
                tbSerialNum.Enabled = identObj.GetBool("HasSerialNumber");
                if (tbSSCC.Enabled || tbSerialNum.Enabled)
                {
                    tbSSCC.RequestFocus();
                } else
                {
                    tbSSCC.Visibility = ViewStates.Invisible;
                    tbSerialNum.Visibility = ViewStates.Invisible;
                }
               
            }
            else
            {
                tbIdent.Text = "";
                Toast.MakeText(this, "Ident ni pravilen.", ToastLength.Long).Show();

            }
        }

        private void ClearData()
        {
            tbSSCC.Text = "";
            tbSerialNum.Text = "";
            tbPacking.Text = "";
            tbIdent.Text = "";
            tbTitle.Text = "";
        }

        private void ProcessStock()
        {
            var warehouse = warehouseAdapter.ElementAt(temporaryPosWarehouse);
            if (warehouse == null)
            {
                Toast.MakeText(this, $"{Resources.GetString(Resource.String.s245)}", ToastLength.Long).Show();
                ClearData();
                return;
            }

            var sscc = tbSSCC.Text.Trim();
            if (tbSSCC.Enabled && string.IsNullOrEmpty(sscc))
            {
                Toast.MakeText(this, $"{Resources.GetString(Resource.String.s270)}", ToastLength.Long).Show();
                return;
            }

            var serialNum = tbSerialNum.Text.Trim();
            if (tbSerialNum.Enabled && string.IsNullOrEmpty(serialNum))
            {
                Toast.MakeText(this, $"{Resources.GetString(Resource.String.s270)}", ToastLength.Long).Show();

                return;
            }

            var ident = tbIdent.Text.Trim();
            if (string.IsNullOrEmpty(ident))
            {
                Toast.MakeText(this, $"{Resources.GetString(Resource.String.s270)}", ToastLength.Long).Show();

                return;
            }
            try
            {
                var location = tbLocation.Text.Trim();
                if (!CommonData.IsValidLocation(warehouse.ID, location))
                {
                    Toast.MakeText(this, $"{Resources.GetString(Resource.String.s258)} '" + location + $"' {Resources.GetString(Resource.String.s272)} '" + warehouse.ID + "'!", ToastLength.Long).Show();
                    return;
                }
                string result;
                int headID = -1;

                if (WebApp.Get("mode=getInventoryHead&wh=" + cbWarehouse.SelectedItem, out result))
                {
                    try
                    {
                        headID = Convert.ToInt32(result);
                    }
                    catch (Exception ex)
                    {
                        headID = -1;
                    }
                    if (headID < 0)
                    {
                        headID = -1;
                    }
                }

                if (tbSSCC.Enabled && tbSerialNum.Enabled)
                {
                    result = LoadStock(warehouse.ID, location, sscc, serialNum, ident);
                    moveItemInner = Services.GetObject("miissl", headID.ToString() + "|" + ident + "|" + serialNum + "|" + sscc + "|" + location, out result);
                }
                else if (tbSSCC.Enabled && !tbSerialNum.Enabled)
                {
                    result = LoadStock(warehouse.ID, location, sscc, string.Empty, ident);
                    moveItemInner = Services.GetObject("miissl", headID.ToString() + "|" + ident + "||" + sscc + "|" + location, out result);

                }
                else if (!tbSSCC.Enabled && tbSerialNum.Enabled)
                {
                    result = LoadStock(warehouse.ID, location, string.Empty, tbSerialNum.Text, ident);
                    moveItemInner = Services.GetObject("miissl", headID.ToString() + "|" + ident + "|" + serialNum + "||" + location, out result);
                }
                else
                {
                    result = LoadStock(warehouse.ID, location, string.Empty, string.Empty, ident);
                    moveItemInner = Services.GetObject("miissl", headID.ToString() + "|" + ident + "|||" + location, out result);
                }
                if (moveItemInner!=null)
                {
                    qtyItem = moveItemInner.GetDouble("Qty");
                }
            
                double q = dataObject.DoubleValue("anQty") ?? 0;
                if (dataObject!=null)
                {
                    lbPacking.Text = $"{Resources.GetString(Resource.String.s155)} ({q.ToString(CommonData.GetQtyPicture())})";
                    tbPacking.Text =  q.ToString(CommonData.GetQtyPicture());

                } else if(result!=null)
                {
                    lbPacking.Text = $"{Resources.GetString(Resource.String.s155)} ({result})";
                    tbPacking.Text = result;
                }


               
            
            }
            catch(Exception err)
            {
                Crashes.TrackError(err);
            }
        }
        public void GetBarcode(string barcode)
        {
            if (tbSSCC.HasFocus)
            {
                    Sound();
                    tbSSCC.Text = barcode;
                    string error;
                    dataObject = GetSSCCData(tbSSCC.Text);
                    if (dataObject != null)
                    {
                        var ident = dataObject.StringValue("acIdent");
                        var loadIdent = CommonData.LoadIdent(ident);
                        string idname = loadIdent.GetString("Name");
                        if (string.IsNullOrEmpty(ident)) { return; }
                        if (loadIdent != null)
                        {
                            tbSerialNum.Enabled = loadIdent.GetBool("HasSerialNumber");
                        }
                        var serial = dataObject.StringValue("acSerialNo");
                        var location = dataObject.StringValue("aclocation");
                        var warehouse = dataObject.StringValue("acWarehouse");
                        tbIdent.Text = ident;
                        tbLocation.Text = location;
                        tbSerialNum.Text = serial;
                        if (loadIdent.GetBool("HasSerialNumber"))
                        {
                            tbSerialNum.Text = serial;
                            tbSerialNum.SetBackgroundColor(Android.Graphics.Color.Aqua);
                        }
                        else
                        {
                            tbSerialNum.SetBackgroundColor(Android.Graphics.Color.Red);
                        }
                        tbLocation.Text = location;
                        tbTitle.Text = idname;
                        ProcessStock();
                        tbPacking.RequestFocus();                    
                }
                else
                {
                    tbSSCC.Text = string.Empty;
                }                        
            }
            else if (tbLocation.HasFocus)
            {
                Sound();
                tbLocation.Text = barcode;
                ProcessLocation();

            }
            else if (tbSerialNum.HasFocus)
            {
                Sound();
                tbSerialNum.Text = barcode;
                ProcessStock();
            }
            else if (tbIdent.HasFocus)
            {
                Sound();
                tbIdent.Text = barcode;
                ProcessIdent();
            }
        }


        private void color()
        {
            tbIdent.SetBackgroundColor(Android.Graphics.Color.Aqua);
            tbSSCC.SetBackgroundColor(Android.Graphics.Color.Aqua);
            tbSerialNum.SetBackgroundColor(Android.Graphics.Color.Aqua);
            tbLocation.SetBackgroundColor(Android.Graphics.Color.Aqua);
            tbIdent.SetBackgroundColor(Android.Graphics.Color.Aqua);
            tbSSCC.SetBackgroundColor(Android.Graphics.Color.Aqua);
            tbSerialNum.SetBackgroundColor(Android.Graphics.Color.Aqua);
            tbPacking.SetBackgroundColor(Android.Graphics.Color.Aqua);
        }

        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            switch (keyCode)
            {

                case Keycode.F2:
                    if (btPrint.Enabled == true)
                    {
                        BtPrint_Click(this, null);
                    }
                    break;

                case Keycode.F3:
                    if (button1.Enabled == true)
                    {
                        Button1_Click(this, null);
                    }
                    break;


                case Keycode.F4:
                    if (btDelete.Enabled == true)
                    {
                        BtDelete_Click(this, null);
                    }
                    break;

                case Keycode.F8:
                    Button2_Click(this, null);
                    break;

            }
            return base.OnKeyDown(keyCode, e);
        }

        private bool CheckData(out double packing, out double units, out double qty, out ComboBoxItem warehouse, out string location, out string ident, out string serNo, out string sscc)
        {
            packing = 0.0;
            units = 0.0;
            qty = 0.0;
            ident = null;
            serNo = null;
            sscc = null;
            warehouse = null;
            location = null;

            warehouse = warehouseAdapter.ElementAt(temporaryPosWarehouse);
            if (warehouse == null)
            {
                Toast.MakeText(this, $"{Resources.GetString(Resource.String.s245)}", ToastLength.Long).Show();
                return false;
            }

            location = tbLocation.Text.Trim();
            if (!CommonData.IsValidLocation(warehouse.ID, location))
            {
                Toast.MakeText(this, $"{Resources.GetString(Resource.String.s258)} '" + location + $"' {Resources.GetString(Resource.String.s272)} '" + warehouse.ID + "'!", ToastLength.Long).Show();
                return false;
            }

            sscc = tbSSCC.Text.Trim();
            if (tbSSCC.Enabled && string.IsNullOrEmpty(sscc))
            {
                Toast.MakeText(this, $"{Resources.GetString(Resource.String.s270)}", ToastLength.Long).Show();
                return false;
            }

            ident = tbIdent.Text.Trim();
            if (string.IsNullOrEmpty(ident))
            {
                Toast.MakeText(this, $"{Resources.GetString(Resource.String.s270)}", ToastLength.Long).Show();
                return false;
            }

            serNo = tbSerialNum.Text.Trim();
            if (tbSerialNum.Enabled && string.IsNullOrEmpty(serNo))
            {
                Toast.MakeText(this, $"{Resources.GetString(Resource.String.s270)}", ToastLength.Long).Show();
                return false;
            }

            if (CommonData.LoadIdent(ident) == null) return false;

            try
            {
                packing = Convert.ToDouble(tbPacking.Text);
            }

            catch (Exception ex)

            {
                Toast.MakeText(this, $"{Resources.GetString(Resource.String.s270)}" + ex.Message, ToastLength.Long).Show();
                return false;
            }


            try
            {
                units = Convert.ToDouble(tbUnits.Text);
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, $"{Resources.GetString(Resource.String.s270)}" + ex.Message, ToastLength.Long).Show();
                return false;
            }

            qty = units * packing;

            return true;
        }

        private void ProcessLocation()
        {
            var warehouse = warehouseAdapter.ElementAt(temporaryPosWarehouse);
            if (warehouse == null)
            {
                Toast.MakeText(this, $"{Resources.GetString(Resource.String.s245)}", ToastLength.Long).Show();

                cbWarehouse.RequestFocus();
                return;
            }

            var location = tbLocation.Text.Trim();
            if (!CommonData.IsValidLocation(warehouse.ID, location))
            {
                Toast.MakeText(this, $"{Resources.GetString(Resource.String.s258)} '" + location + $"' {Resources.GetString(Resource.String.s272)} '" + warehouse.ID + "'!", ToastLength.Long).Show();

                tbLocation.RequestFocus();
                return;
            }

        }

        private void Sound() 
        {
            soundPool.Play(soundPoolId, 1, 1, 0, 0, 1);
        }


    }
}