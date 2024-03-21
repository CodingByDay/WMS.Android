using Android.App;
using Android.Content;
using Android.Net;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Microsoft.AppCenter.Crashes;
using WMS.App;
using Stream = Android.Media.Stream;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TrendNET.WMS.Core.Data;
using TrendNET.WMS.Device.App;
using TrendNET.WMS.Device.Services;

using AndroidX.AppCompat.App;using AlertDialog = Android.App.AlertDialog;namespace WMS
{
    [Activity(Label = "MorePallets")]
    public class MorePalletsClass : AppCompatActivity, IBarcodeResult
    {
        //lvCardMore
        //btConfirm
        //btLogin
        private ListView lvCardMore;
        private Button btConfirm;
        private Button btExit;
        private Button btLogin;
        private List<MorePallets> data = new List<MorePallets>();
        private string identMain;
        private NameValueObject moveHead = (NameValueObject)InUseObjects.Get("MoveHead");
        private EditText tbSSCC;

        private NameValueObject moveItem = (NameValueObject)InUseObjects.Get("MoveItem");
        private bool enabledSerial;

        public string IdentName { get; private set; }

        public void GetBarcode(string barcode)
        {
            if (barcode != "Scan fail")
            {

                FilData(barcode);
            }
        }
       

       
        private void FilData(string barcode)
        {
            if(!String.IsNullOrEmpty(barcode))
            {
                string error;

                var dataObject = Services.GetObject("sscc", barcode, out error);
                if (dataObject != null)
                {
                         var ident = dataObject.GetString("Ident");
                         var name = dataObject.GetString("IdentName");
                         var serial = dataObject.GetString("SerialNo");
                         var location = dataObject.GetString("Location");
                         MorePallets pallets = new MorePallets();
                         pallets.Ident = ident;
                         pallets.Name = name;
                         pallets.Quantity = barcode;               
                         pallets.SSCC = barcode;
                         pallets.Serial = serial;
                         var loadIdent = CommonData.LoadIdent(ident);

                         
                         enabledSerial = loadIdent.GetBool("HasSerialNumber");


#nullable enable        
                    MorePallets? obj = ProcessQty(pallets, location);
#nullable disable
                    /* Adds an object to the list. */
                    if(obj is null)
                    {
                        Toast.MakeText(this, $"{Resources.GetString(Resource.String.s225)}", ToastLength.Long).Show();
                    } else
                    {
                        data.Add(obj);
                        // Added to the list

                    }
                }
                else
                {
                    return;
                }
            }
        }
        private MorePallets ProcessQty(MorePallets obj, string location)
        {
            var sscc = obj.SSCC;
            if (string.IsNullOrEmpty(sscc)) { 
                return null;
            }

            var serialNo = obj.Serial;
            if (enabledSerial&&string.IsNullOrEmpty(serialNo)) {
                return null;
            }

            var ident = obj.Ident;
            if (string.IsNullOrEmpty(ident)) { 
                return null;
            }

            var identObj = CommonData.LoadIdent(ident);
            var isEnabled = identObj.GetBool("HasSerialNumber");

            if (!CommonData.IsValidLocation(moveHead.GetString("Issuer"), location))
            {
                string SuccessMessage = string.Format($"{Resources.GetString(Resource.String.s273)}" + location + $"{Resources.GetString(Resource.String.s27)}" + moveHead.GetString("Issuer") + "'!");
                Toast.MakeText(this, SuccessMessage, ToastLength.Long).Show();
              

                return null;
            }

            var stockQty = GetStock(moveHead.GetString("Issuer"), location, sscc, serialNo, ident, isEnabled);
            if (!Double.IsNaN(stockQty))
            {
                obj.Quantity = stockQty.ToString(CommonData.GetQtyPicture());
                
            }
            else
            {
                Toast.MakeText(this, $"{Resources.GetString(Resource.String.s225)}", ToastLength.Long).Show();
            }
            return obj;

        }

        private double GetStock(string warehouse, string location, string sscc, string serialNum, string ident, bool serialEnabled)
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


        private Double LoadStockFromStockSerialNo(string warehouse, string location, string sscc, string serialNum, string ident)
        {

            try
            {
                string error;
                var stock = Services.GetObject("str", warehouse + "|" + location + "|" + sscc + "|" + serialNum + "|" + ident, out error);
                if (stock == null)
                {
                    string SuccessMessage = string.Format($"{Resources.GetString(Resource.String.s216)}" + error);
                    Toast.MakeText(this, SuccessMessage, ToastLength.Long).Show();

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
            catch (Exception err)
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


        protected override void OnCreate(Bundle savedInstanceState)
        {
                base.OnCreate(savedInstanceState);
                SetTheme(Resource.Style.AppTheme_NoActionBar);
                SetContentView(Resource.Layout.MorePalletsClass);
                AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
                var _customToolbar = new CustomToolbar(this, toolbar, Resource.Id.navIcon);
                _customToolbar.SetNavigationIcon(settings.RootURL + "/Services/Logo");
                SetSupportActionBar(_customToolbar._toolbar);
                SupportActionBar.SetDisplayShowTitleEnabled(false);
                lvCardMore = FindViewById<ListView>(Resource.Id.lvCardMore);
                btConfirm = FindViewById<Button>(Resource.Id.btConfirm);
                btExit = FindViewById<Button>(Resource.Id.btExit);
                MorePalletsAdapter adapter = new MorePalletsAdapter(this, data);
                lvCardMore.Adapter = adapter;
                tbSSCC = FindViewById<EditText>(Resource.Id.tbSSCC);
                tbSSCC.KeyPress += TbSSCC_KeyPress;
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

        private void TbSSCC_KeyPress(object sender, View.KeyEventArgs e)
        {
            e.Handled = false;
            if (e.Event.Action == KeyEventActions.Down && e.KeyCode == Keycode.Enter)
            {
                // Add your logic here 
                FilData(tbSSCC.Text);
            }
        }
    }
}