using Android.Content;
using Android.Content.PM;
using Android.Media;
using Android.Net;
using Android.Views;
using BarCode2D_Receiver;
using TrendNET.WMS.Core.Data;
using TrendNET.WMS.Device.App;
using TrendNET.WMS.Device.Services;
using WMS.App;
using WMS.Printing;
namespace WMS
{
    [Activity(Label = "PrintingReprintLabels", ScreenOrientation = ScreenOrientation.Portrait)]
    public class PrintingReprintLabels : CustomBaseActivity, IBarcodeResult
    {
        private EditText tbIdent;
        private EditText tbTitle;
        private EditText tbSSCC;
        private EditText tbSerialNum;
        private Spinner cbWarehouse;
        private EditText tbLocation;
        private EditText tbQty;
        private Spinner cbSubject;
        private NameValueObject stock = null;
        private Button btPrint;
        private Button button2;
        private List<ComboBoxItem> warehouseAdapter = new List<ComboBoxItem>();
        private List<ComboBoxItem> subjectsAdapter = new List<ComboBoxItem>();
        SoundPool soundPool;
        int soundPoolId;
        private int tempPositionSubject;
        private int tempPositionWarehouse;
        private EditText tbNumberOfCopies;
        private int numberOfCopies;
        private Barcode2D barcode2D;

        private void color()
        {
            tbIdent.SetBackgroundColor(Android.Graphics.Color.Aqua);
            tbSSCC.SetBackgroundColor(Android.Graphics.Color.Aqua);
            tbSerialNum.SetBackgroundColor(Android.Graphics.Color.Aqua);
            tbLocation.SetBackgroundColor(Android.Graphics.Color.Aqua);
        }


        public async void GetBarcode(string barcode)
        {
            if (tbSSCC.HasFocus)
            {


                tbSSCC.Text = barcode;
                tbSerialNum.RequestFocus();
            }
            else if (tbIdent.HasFocus && barcode != "Scan fail")
            {

                tbIdent.Text = barcode;
                await ProcessIdent();

            }
            else if (tbSerialNum.HasFocus && barcode != "Scan fail")
            {

                tbSerialNum.Text = barcode;
            }
            else if (tbLocation.HasFocus && barcode != "Scan fail")
            {

                tbLocation.Text = barcode;
                await ProcessQty();
                tbQty.RequestFocus();
            }
        }

        private async Task ProcessIdent()
        {
            var ident = tbIdent.Text.Trim();
            var identObj = await CommonData.LoadIdentAsync(ident, this);
            if (identObj != null)
            {
                tbTitle.Text = identObj.GetString("Name");
                tbSSCC.Enabled = identObj.GetBool("isSSCC");
                tbSerialNum.Enabled = identObj.GetBool("HasSerialNumber");

            }
            else
            {
                tbTitle.Text = "";
                tbSSCC.Enabled = false;
                tbSerialNum.Enabled = false;
                tbIdent.RequestFocus();
            }
        }



        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetTheme(Resource.Style.AppTheme_NoActionBar);
            if (App.Settings.tablet)
            {
                base.RequestedOrientation = ScreenOrientation.Landscape;
                base.SetContentView(Resource.Layout.PrintingReprintLabelsTablet);
            }
            else
            {
                base.RequestedOrientation = ScreenOrientation.Portrait;
                base.SetContentView(Resource.Layout.PrintingReprintLabels);
            }
            AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
            var _customToolbar = new CustomToolbar(this, toolbar, Resource.Id.navIcon);
            _customToolbar.SetNavigationIcon(App.Settings.RootURL + "/Services/Logo");
            SetSupportActionBar(_customToolbar._toolbar);
            SupportActionBar.SetDisplayShowTitleEnabled(false);
            tbIdent = FindViewById<EditText>(Resource.Id.tbIdent);
            tbTitle = FindViewById<EditText>(Resource.Id.tbTitle);
            tbSSCC = FindViewById<EditText>(Resource.Id.tbSSCC);
            tbSerialNum = FindViewById<EditText>(Resource.Id.tbSerialNum);
            cbWarehouse = FindViewById<Spinner>(Resource.Id.cbWarehouse);
            tbLocation = FindViewById<EditText>(Resource.Id.tbLocation);
            tbQty = FindViewById<EditText>(Resource.Id.tbQty);
            cbSubject = FindViewById<Spinner>(Resource.Id.cbSubject);
            btPrint = FindViewById<Button>(Resource.Id.btPrint);
            button2 = FindViewById<Button>(Resource.Id.button2);
            tbNumberOfCopies = FindViewById<EditText>(Resource.Id.tbNumberOfCopies);
            color();
            tbTitle.FocusChange += TbTitle_FocusChange;
            btPrint.Click += BtPrint_Click;
            button2.Click += Button2_Click;
            tbSSCC.FocusChange += TbSSCC_FocusChange;
            cbWarehouse.ItemSelected += CbWarehouse_ItemSelected;
            cbSubject.ItemSelected += CbSubject_ItemSelected;
            var warehouses = await CommonData.ListWarehousesAsync();

            warehouses.Items.ForEach(w =>
            {
                warehouseAdapter.Add(new ComboBoxItem { ID = w.GetString("Subject"), Text = w.GetString("Name") });
            });

            var subjects = await CommonData.ListReprintSubjectsAsync();

            subjects.Items.ForEach(s =>
            {
                subjectsAdapter.Add(new ComboBoxItem { ID = s.GetString("ID"), Text = s.GetString("ID") });
            });


            var adapterWarehouses = new ArrayAdapter<ComboBoxItem>(this,
            Android.Resource.Layout.SimpleSpinnerItem, warehouseAdapter);

            adapterWarehouses.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            cbWarehouse.Adapter = adapterWarehouses;
            var adapterSubjects = new ArrayAdapter<ComboBoxItem>(this,
                       Android.Resource.Layout.SimpleSpinnerItem, subjectsAdapter);

            adapterSubjects.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            cbSubject.Adapter = adapterWarehouses;


            tbIdent.RequestFocus();

            await SetDefault();

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
                    SentrySdk.CaptureException(err);
                }
            }
            else
            {
                LoaderManifest.LoaderManifestLoop(this);
            }
        }

        private async Task SetDefault()
        {
            tbQty.Text = "1";
            tbLocation.Text = await CommonData.GetSettingAsync("DefaultPaletteLocation", this);
        }

        private async void TbTitle_FocusChange(object sender, View.FocusChangeEventArgs e)
        {
            await ProcessIdent();
        }

        private void CbSubject_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            tempPositionSubject = e.Position;
        }

        private void CbWarehouse_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            tempPositionWarehouse = e.Position;
        }

        private bool LoadStock(string warehouse, string location, string sscc, string serialNum, string ident)
        {

            try
            {


                string error;
                stock = Services.GetObject("str", warehouse + "|" + location + "|" + sscc + "|" + serialNum + "|" + ident, out error);
                if (stock == null)
                {
                    string toast = string.Format($"{Resources.GetString(Resource.String.s216)}" + error);
                    Toast.MakeText(this, toast, ToastLength.Long).Show();

                    return false;
                }

                return true;
            }
            catch (Exception err)
            {

                SentrySdk.CaptureException(err);
                return false;

            }
        }


        private async Task ProcessQty()
        {
            btPrint.Enabled = false;

            var warehouse = warehouseAdapter.ElementAt(tempPositionWarehouse);
            if (warehouse == null) { return; }

            var sscc = tbSSCC.Text.Trim();
            if (tbSSCC.Enabled && string.IsNullOrEmpty(sscc)) { return; }

            var serialNo = tbSerialNum.Text.Trim();
            if (tbSerialNum.Enabled && string.IsNullOrEmpty(serialNo)) { return; }

            var ident = tbIdent.Text.Trim();
            if (string.IsNullOrEmpty(ident)) { return; }

            var identObj = await CommonData.LoadIdentAsync(ident, this);
            if (identObj != null)
            {
                ident = identObj.GetString("Code");
                tbIdent.Text = ident;
            }

            if (LoadStock(warehouse.ID, tbLocation.Text.Trim(), sscc, serialNo, ident))
            {
                tbQty.Text = stock.GetDouble("RealStock").ToString(await CommonData.GetQtyPictureAsync(this));
                btPrint.Enabled = true;
            }
            else
            {
                tbQty.Text = "";
            }

            tbQty.RequestFocus();
        }

        private async void TbSSCC_FocusChange(object sender, View.FocusChangeEventArgs e)
        {
            await ProcessIdent();
        }
        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            switch (keyCode)
            {
                // in smartphone
                case Keycode.F3:
                    if (btPrint.Enabled == true)
                    {
                        BtPrint_Click(this, null);
                    }
                    break;
                //return true;


                case Keycode.F8:
                    if (button2.Enabled == true)
                    {
                        Button2_Click(this, null);
                    }
                    break;

            }
            return base.OnKeyDown(keyCode, e);
        }
        private void Button2_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(MainMenu));
            Finish();
        }

        private void BtPrint_Click(object sender, EventArgs e)
        {


            if (String.IsNullOrEmpty(tbIdent.Text) | String.IsNullOrEmpty(tbTitle.Text))
            { return; }

            var qty = 0.0;

            if (!String.IsNullOrEmpty(tbQty.Text))
            {

                qty = Convert.ToDouble(tbQty.Text);

            }





            if (qty <= 0.0)
            {
                string toast = string.Format($"{Resources.GetString(Resource.String.s298)}");
                Toast.MakeText(this, toast, ToastLength.Long).Show();

                return;
            }
            try
            {

                try
                {
                    numberOfCopies = Convert.ToInt32(tbNumberOfCopies.Text);
                    if (numberOfCopies <= 0) { numberOfCopies = 1; }

                }
                catch (Exception)
                {
                    numberOfCopies = 1;
                }

                var nvo = new NameValueObject("ReprintLabels");
                PrintingCommon.SetNVOCommonData(ref nvo);
                nvo.SetInt("Copies", numberOfCopies);
                nvo.SetString("SSCC", tbSSCC.Text);
                nvo.SetString("SerialNum", tbSerialNum.Text);
                nvo.SetString("Location", tbLocation.Text);
                nvo.SetString("Ident", tbIdent.Text);
                nvo.SetString("Title", tbTitle.Text);
                nvo.SetDouble("Qty", qty);

                if (subjectsAdapter.Count > 0)
                {

                    nvo.SetString("Subject", cbSubject.SelectedItem == null ? "" : subjectsAdapter.ElementAt(tempPositionSubject).ID);
                }
                else
                {
                    nvo.SetString("Subject", String.Empty);

                }
                PrintingCommon.SendToServer(nvo);
            }
            finally
            {

                string toast = string.Format($"{Resources.GetString(Resource.String.s299)}");
                Toast.MakeText(this, toast, ToastLength.Long).Show();
                ClearTheScreen();
            }

        }

        private void ClearTheScreen()
        {
            tbIdent.Text = String.Empty;
            tbTitle.Text = String.Empty;
            tbSerialNum.Text = String.Empty;
            tbSSCC.Text = String.Empty;
            tbIdent.RequestFocus();

        }
    }
}