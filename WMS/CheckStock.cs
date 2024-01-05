using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Media;
using Android.Net;
using Android.OS;
using Android.OS.Storage;
using Android.Preferences;
using Android.Views;
using Android.Widget;
using BarCode2D_Receiver;

using Microsoft.AppCenter.Crashes;
using Newtonsoft.Json;
using WMS.App;
using Stream = Android.Media.Stream;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TrendNET.WMS.Device.App;
using TrendNET.WMS.Device.Services;
using AndroidX.AppCompat.App;
using AlertDialog = Android.App.AlertDialog;

using AndroidX.AppCompat.App;

namespace WMS
{
    [Activity(Label = "CheckStock", ScreenOrientation = ScreenOrientation.Portrait, NoHistory = true)]
    public class CheckStock : AppCompatActivity, IBarcodeResult
    {
        private CustomAutoCompleteTextView cbWarehouses;
        private CustomAutoCompleteTextView tbLocation;
        private CustomAutoCompleteTextView tbIdent;
        private Button btShowStock;
        private Button button1;
        SoundPool soundPool;
        int soundPoolId;
        private TextView lbStock;
        private List<ComboBoxItem> spinnerAdapterList = new List<ComboBoxItem>();
        private int temporaryPositionWarehouse;
        private string stock;
        private Button btnOK;

        private List<String> identData = new List<string>();
        private List<string> returnList;
        private List<String> locationData = new List<String>();
        private CustomAutoCompleteAdapter<string> DataAdapterLocation;
        private CustomAutoCompleteAdapter<string> locationAdapter;
        private bool initial = true;
        private List<string> savedIdents;
        private CustomAutoCompleteAdapter<string> tbIdentAdapter;

        public void GetBarcode(string barcode)
        {
            if (tbIdent.HasFocus)
            {
                Sound();
                tbIdent.Text = barcode;
                ProcessStock();

            } else if (tbLocation.HasFocus)
            {
                Sound();
                tbLocation.Text = barcode;
            }
        }
              
        private string LoadStockFromStockSerialNo(string warehouse, string location, string ident)
        {
            try
            {
                string error;
                var stock = Services.GetObjectList("str", out error, warehouse + "|" + location + "|" + ident);
                if (stock == null)
                {

                    string WebError = string.Format("Napaka pri preverjanju zaloge." + error);
                    DialogHelper.ShowDialogError(this, this, WebError);

                    return "";
                }
                else
                {
                    return string.Join("\r\n", stock.Items.Select(x => "L:" + x.GetString("Location") + " = " + x.GetDouble("RealStock").ToString(CommonData.GetQtyPicture())).ToArray());
                }
            }
            catch (Exception err)
            {

                Crashes.TrackError(err);
                return "";

            }
        }


        private void ProcessStock()
        {
            var wh = spinnerAdapterList.ElementAt(temporaryPositionWarehouse);
            if (wh == null)
            {
                string WebError = string.Format("Skladišče ni izbrano.");
                DialogHelper.ShowDialogError(this, this, WebError);

             //   Toast.MakeText(this, WebError, ToastLength.Long).Show(); tbIdent.Text = "";
                return;
            }

            if (!string.IsNullOrEmpty(tbLocation.Text.Trim()))
            {
                if (!CommonData.IsValidLocation(wh.ID, tbLocation.Text.Trim()))
                {
                    string WebError = string.Format("Lokacija ni veljavna");
                    DialogHelper.ShowDialogError(this, this, WebError);
                    return;
                }
            }

            if (string.IsNullOrEmpty(tbIdent.Text.Trim()))
            {
                string WebError = string.Format("Ident ni podan");
                DialogHelper.ShowDialogError(this, this, WebError);

                //Toast.MakeText(this, WebError, ToastLength.Long).Show();
                return;
            }

            stock = LoadStockFromStockSerialNo(wh.ID, tbLocation.Text.Trim(), tbIdent.Text.Trim());
            lbStock.Text = "Zaloga:\r\n" + stock;
            isEmptyStock();
        }


        private void isEmptyStock()
        { 
            if(stock != "")
            {
                lbStock.SetBackgroundColor(Android.Graphics.Color.Green);
            } else
            {
                lbStock.SetBackgroundColor(Android.Graphics.Color.Red);
            }
        }

        private void color()
        {

            tbIdent.SetBackgroundColor(Android.Graphics.Color.Aqua);
            tbLocation.SetBackgroundColor(Android.Graphics.Color.Aqua);
        }

        private void Sound()
        {
            soundPool.Play(soundPoolId, 1, 1, 0, 0, 1);
        }

        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            switch (keyCode)
            {
                // Setting F2 to method ProccesStock()
                case Keycode.F2:
                    BtShowStock_Click(this, null);
                    break;

                case Keycode.F8:
                    Button1_Click(this, null);
                    break;
                // return true;

            }
            return base.OnKeyDown(keyCode, e);
        }
        protected async override void OnCreate(Bundle savedInstanceState)
        {

            base.OnCreate(savedInstanceState);
            
            // Create your application here.
            SetContentView(Resource.Layout.CheckStock);
            AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);

            // Initialize the custom toolbar with the ImageView ID
            var _customToolbar = new CustomToolbar(this, toolbar, Resource.Id.navIcon);
            // Set the navigation icon with the URL
            _customToolbar.SetNavigationIcon(settings.RootURL + "/Services/Logo");


            cbWarehouses = FindViewById<CustomAutoCompleteTextView>(Resource.Id.cbWarehouses);       
            tbLocation = FindViewById<CustomAutoCompleteTextView>(Resource.Id.tbLocation);
            tbIdent = FindViewById<CustomAutoCompleteTextView>(Resource.Id.tbIdent);


            btShowStock = FindViewById<Button>(Resource.Id.btShowStock);
            btShowStock.Click += BtShowStock_Click;
            button1 = FindViewById<Button>(Resource.Id.button1);
            button1.Click += Button1_Click;
            lbStock = FindViewById<TextView>(Resource.Id.lbStock);
      
            color();
            soundPool = new SoundPool(10, Stream.Music, 0);
            soundPoolId = soundPool.Load(this, Resource.Raw.beep, 1);
            Barcode2D barcode2D = new Barcode2D();
            barcode2D.open(this, this);
            // First load the warehouses.
            var whs = CommonData.ListWarehouses();
            whs.Items.ForEach(wh =>
            {
                spinnerAdapterList.Add(new ComboBoxItem { ID = wh.GetString("Subject"), Text = wh.GetString("Name") });
            });
            lbStock = FindViewById<TextView>(Resource.Id.lbStock);
            var adapterWarehouse = new CustomAutoCompleteAdapter<ComboBoxItem>(this,
            Android.Resource.Layout.SimpleSpinnerItem, spinnerAdapterList);
            adapterWarehouse.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            cbWarehouses.Adapter = adapterWarehouse;
            identData = Caching.Caching.SavedList;
            ISharedPreferences sharedPreferences = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
            ISharedPreferencesEditor editor = sharedPreferences.Edit();
            string savedIdentsJson = sharedPreferences.GetString("idents", "");
            if (!string.IsNullOrEmpty(savedIdentsJson))
            {
                // Deserialize the JSON string back to a List<string>
                savedIdents = JsonConvert.DeserializeObject<List<string>>(savedIdentsJson);
                // Now you have your list of idents in the savedIdents variable
            }

            tbIdentAdapter = new CustomAutoCompleteAdapter<string>(this, Android.Resource.Layout.SimpleDropDownItem1Line, new List<string>());
            tbIdent.Adapter = tbIdentAdapter;
            tbIdent.TextChanged += (sender, e) =>
            {
                string userInput = e.Text.ToString();
                UpdateSuggestions(userInput);
            };

            var dw = CommonData.GetSetting("DefaultWarehouse");
            if (!string.IsNullOrEmpty(dw))
            {
                temporaryPositionWarehouse = cbWarehouses.SetItemByString(dw);
                await GetLocationsForGivenWarehouse(spinnerAdapterList.ElementAt(temporaryPositionWarehouse).Text);
                DataAdapterLocation = new CustomAutoCompleteAdapter<string>(this,
                Android.Resource.Layout.SimpleSpinnerItem, locationData);
                tbLocation.Adapter = null;
                tbLocation.Adapter = DataAdapterLocation;
            }


         
            var _broadcastReceiver = new NetworkStatusBroadcastReceiver();
            _broadcastReceiver.ConnectionStatusChanged += OnNetworkStatusChanged;
            Application.Context.RegisterReceiver(_broadcastReceiver,
            new IntentFilter(ConnectivityManager.ConnectivityAction));



            cbWarehouses.ItemClick += CbWarehouses_ItemClick;
            tbLocation.ItemClick += TbLocation_ItemClick;

        }

        private void TbLocation_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
           
        }

        private async void CbWarehouses_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {

            if (e.Position != 0)
            {

                temporaryPositionWarehouse = e.Position;
            }
            await GetLocationsForGivenWarehouse(spinnerAdapterList.ElementAt(temporaryPositionWarehouse).Text);
            DataAdapterLocation = new CustomAutoCompleteAdapter<string>(this,
            Android.Resource.Layout.SimpleSpinnerItem, locationData);
            tbLocation.Adapter = null;
            tbLocation.Adapter = DataAdapterLocation;
        }

        private void UpdateSuggestions(string userInput)
        {
            // Provide custom suggestions based on user input
            List<string> suggestions = GetCustomSuggestions(userInput);
            // Clear the existing suggestions and add the new ones
            tbIdentAdapter.Clear();
            tbIdentAdapter.AddAll(suggestions);
            tbIdentAdapter.NotifyDataSetChanged();
        }

        private List<string> GetCustomSuggestions(string userInput)
        {
            // Provide custom suggestions based on userInput
            // Example: Suggest fruits based on user input

            return savedIdents
                .Where(suggestion => suggestion.ToLower().Contains(userInput.ToLower())).Take(10000)
                .ToList();
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
        private void SpinnerLocation_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            if (!initial)
            {
                tbLocation.Text = locationData.ElementAt(e.Position);
            }
            else
            {
                tbLocation.Text = string.Empty;
                initial = false;
            }
        }
        private void SpinnerIdent_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            tbIdent.Text = identData.ElementAt(e.Position);
        }

    
        public override void OnBackPressed()
        {
            HelpfulMethods.releaseLock();
            base.OnBackPressed();
        }


        private void Button1_Click(object sender, System.EventArgs e)
        {
            this.Finish();
        }

        private void BtShowStock_Click(object sender, System.EventArgs e)
        {
            ProcessStock();
        }
        private async Task GetLocationsForGivenWarehouse(string warehouse)
        {
            await Task.Run(() =>
            {
                locationAdapter = new CustomAutoCompleteAdapter<string>(this,
                    Android.Resource.Layout.SimpleSpinnerItem, locationData);

                locationData.Clear();
                List<string> result = new List<string>();
                string error;
                var issuerLocs = Services.GetObjectList("lo", out error, spinnerAdapterList.ElementAt(temporaryPositionWarehouse).Text);
                var debi = issuerLocs.Items.Count();
                if (issuerLocs == null)
                {
                    // Toast.MakeText(this, "Prišlo je do napake", ToastLength.Long).Show();
                    DialogHelper.ShowDialogError(this, this, "Prišlo je do napake");

                }
                else
                {
                    issuerLocs.Items.ForEach(x =>
                    {
                        var location = x.GetString("LocationID");
                        locationData.Add(location);
                        // Notify the adapter state change!
                    });
                }
            });
        }
        private async void CbWarehouses_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            Spinner spinner = (Spinner)sender;
            if (e.Position != 0)
            {
                string toast = string.Format("Izbrali ste: {0}", spinner.GetItemAtPosition(e.Position));
                Toast.MakeText(this, toast, ToastLength.Long).Show();
                temporaryPositionWarehouse = e.Position;
            }
            Toast.MakeText(this, "Pripravljamo listu lokacija.", ToastLength.Long).Show();
            await GetLocationsForGivenWarehouse(spinnerAdapterList.ElementAt(temporaryPositionWarehouse).Text);
            Toast.MakeText(this, "Lista lokacija pripravljena.", ToastLength.Long).Show();
            DataAdapterLocation = new CustomAutoCompleteAdapter<string>(this,
            Android.Resource.Layout.SimpleSpinnerItem, locationData);
            tbLocation.Adapter = null;
            tbLocation.Adapter = DataAdapterLocation;

        }
    }
}