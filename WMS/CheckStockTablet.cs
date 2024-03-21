using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Media;
using Android.Net;
using Android.Preferences;
using Android.Views;
using AndroidX.AppCompat.App;
using BarCode2D_Receiver;
using Com.Jsibbold.Zoomage;
using Microsoft.AppCenter.Crashes;
using Newtonsoft.Json;
using TrendNET.WMS.Device.App;
using TrendNET.WMS.Device.Services;
using WMS.App;
using static Android.App.ActionBar;
using Stream = Android.Media.Stream;

namespace WMS
{
    [Activity(Label = "CheckStockTablet", ScreenOrientation = Android.Content.PM.ScreenOrientation.Landscape)]
    public class CheckStockTablet : AppCompatActivity, IBarcodeResult
    {
        private CustomAutoCompleteTextView cbWarehouses;
        private CustomAutoCompleteTextView tbLocation;
        private CustomAutoCompleteTextView tbIdent;
        private Button btShowStock;
        private Button button1;
        private SoundPool soundPool;
        private int soundPoolId;
        private TextView lbStock;
        private List<ComboBoxItem> spinnerAdapterList = new List<ComboBoxItem>();
        private int temporaryPositionWarehouse;
        private string element;
        private string stock;
        private ListView listData;
        private List<CheckStockAddonList> data = new List<CheckStockAddonList>();
        private ImageView imagePNG;
        private Dialog popupDialog;
        private ZoomageView image;
        private Button btnOK;
        private List<String> identData = new List<string>();
        private List<string> returnList;
        private List<String> locationData = new List<String>();
        private int tempPosition;
        private List<string> savedIdents;
        private CustomAutoCompleteAdapter<string> tbIdentAdapter;

        public CustomAutoCompleteAdapter<string> DataAdapterLocation { get; private set; }

        public void GetBarcode(string barcode)
        {
            if (tbIdent.HasFocus)
            {
                Sound();
                tbIdent.Text = barcode;
                ProcessStock();
                showPictureIdent(tbIdent.Text, element);
            }
            else if (tbLocation.HasFocus)
            {
                Sound();
                tbLocation.Text = barcode;
            }
        }

        public override void OnBackPressed()
        {
            HelpfulMethods.releaseLock();

            base.OnBackPressed();
        }

        private string LoadStockFromStockSerialNo(string warehouse, string location, string ident)
        {
            try
            {
                string error;
                var stock = Services.GetObjectList("str", out error, warehouse + "|" + location + "|" + ident);
                if (stock == null)
                {
                    string WebError = string.Format($"{Resources.GetString(Resource.String.s216)}" + error);
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
                string WebError = string.Format($"{Resources.GetString(Resource.String.s245)}");
                DialogHelper.ShowDialogError(this, this, WebError);
                return;
            }

            if (!string.IsNullOrEmpty(tbLocation.Text.Trim()))
            {
                if (!CommonData.IsValidLocation(wh.ID, tbLocation.Text.Trim()))
                {
                    string WebError = string.Format($"{Resources.GetString(Resource.String.s234)}");
                    DialogHelper.ShowDialogError(this, this, WebError);
                    return;
                }
            }

            if (string.IsNullOrEmpty(tbIdent.Text.Trim()))
            {
                string WebError = string.Format($"{Resources.GetString(Resource.String.s235)}");
                DialogHelper.ShowDialogError(this, this, WebError);
                return;
            }

            stock = LoadStockFromStockSerialNo(wh.ID, tbLocation.Text.Trim(), tbIdent.Text.Trim());
            lbStock.Text = $"{Resources.GetString(Resource.String.s155)}:\r\n" + stock;
            isEmptyStock();
        }

        private void isEmptyStock()
        {
            if (stock != "")
            {
                lbStock.SetBackgroundColor(Android.Graphics.Color.Green);
            }
            else
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

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetTheme(Resource.Style.AppTheme_NoActionBar);
            // Create your application here.
            SetContentView(Resource.Layout.CheckStockTablet);
            AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
            var _customToolbar = new CustomToolbar(this, toolbar, Resource.Id.navIcon);
            _customToolbar.SetNavigationIcon(settings.RootURL + "/Services/Logo");
            SetSupportActionBar(_customToolbar._toolbar);
            SupportActionBar.SetDisplayShowTitleEnabled(false);
            cbWarehouses = FindViewById<CustomAutoCompleteTextView>(Resource.Id.cbWarehouses);
            tbLocation = FindViewById<CustomAutoCompleteTextView>(Resource.Id.tbLocation);
            tbIdent = FindViewById<CustomAutoCompleteTextView>(Resource.Id.tbIdent);
            btShowStock = FindViewById<Button>(Resource.Id.btShowStock);
            btShowStock.Click += BtShowStock_Click;
            button1 = FindViewById<Button>(Resource.Id.button1);
            button1.Click += Button1_Click;
            listData = FindViewById<ListView>(Resource.Id.listData);
            lbStock = FindViewById<TextView>(Resource.Id.lbStock);
            CheckStockAddonAdapter adapter = new CheckStockAddonAdapter(this, data);
            listData.Adapter = adapter;
            imagePNG = FindViewById<ImageView>(Resource.Id.imagePNG);
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

            var dw = CommonData.GetSetting("DefaultWarehouse");
            if (!string.IsNullOrEmpty(dw))
            {
                temporaryPositionWarehouse = cbWarehouses.SetItemByString(dw);
                await GetLocationsForGivenWarehouse();
                DataAdapterLocation = new CustomAutoCompleteAdapter<string>(this,
                Android.Resource.Layout.SimpleSpinnerItem, locationData);
                tbLocation.Adapter = null;
                tbLocation.Adapter = DataAdapterLocation;
            }

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
            imagePNG.Visibility = ViewStates.Invisible;

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
            await GetLocationsForGivenWarehouse();
            DataAdapterLocation = new CustomAutoCompleteAdapter<string>(this,
            Android.Resource.Layout.SimpleSpinnerItem, locationData);
            tbLocation.Adapter = null;
            tbLocation.Adapter = DataAdapterLocation;
        }

        public bool IsOnline()
        {
            var cm = (ConnectivityManager)GetSystemService(ConnectivityService);
            return cm.ActiveNetworkInfo == null ? false : cm.ActiveNetworkInfo.IsConnected;
        }

        private List<string> GetCustomSuggestions(string userInput)
        {
            // Provide custom suggestions based on userInput
            // Example: Suggest fruits based on user input

            return savedIdents
                .Where(suggestion => suggestion.ToLower().Contains(userInput.ToLower())).Take(10000)
                .ToList();
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
            tbLocation.Text = locationData.ElementAt(e.Position);

            Toast.MakeText(this, $"{Resources.GetString(Resource.String.s236)}  {locationData.ElementAt(e.Position)}.", ToastLength.Long).Show();
        }

        private void SpinnerIdent_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            tbIdent.Text = identData.ElementAt(e.Position);
            Toast.MakeText(this, $"{Resources.GetString(Resource.String.s236)}  {identData.ElementAt(e.Position)}", ToastLength.Long).Show();
        }

        private void showPictureIdent(string ident, string wh)
        {
            try
            {
                Android.Graphics.Bitmap show = Services.GetImageFromServerIdent(wh, ident);

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

        private void showPicture(string wh)
        {
            try
            {
                Android.Graphics.Bitmap show = Services.GetImageFromServer(wh);
                Drawable d = new BitmapDrawable(Resources, show);
                imagePNG.SetImageDrawable(d);
                imagePNG.Visibility = ViewStates.Visible;
                imagePNG.Click += (e, ev) => { ImageClick(d); };
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
            popupDialog.Window.SetLayout(LayoutParams.MatchParent, LayoutParams.WrapContent);
            popupDialog.Window.SetBackgroundDrawable(new ColorDrawable(Color.ParseColor("#081a45")));
            image = popupDialog.FindViewById<ZoomageView>(Resource.Id.image);
            image.SetMinimumHeight(500);
            image.SetMinimumWidth(800);
            image.SetImageDrawable(d);
            // Access Popup layout fields like below
        }

        private void Button1_Click(object sender, System.EventArgs e)
        {
            Finish();
        }

        private void BtShowStock_Click(object sender, System.EventArgs e)
        {
            data.Clear();
            ProcessStock();
            fillItemsOfList();
        }

        private void fillItemsOfList()
        {
            var wh = spinnerAdapterList.ElementAt(temporaryPositionWarehouse);
            string error;
            var stock = Services.GetObjectList("str", out error, wh.ID + "||" + tbIdent.Text);
            // return string.Join("\r\n", stock.Items.Select(x => "L:" + x.GetString("Location") + " = " + x.GetDouble("RealStock").ToString(CommonData.GetQtyPicture())).ToArray());
            stock.Items.ForEach(x =>
            {
                data.Add(new CheckStockAddonList
                {
                    Ident = x.GetString("Ident"),
                    Location = x.GetString("Location"),
                    Quantity = x.GetDouble("RealStock").ToString(CommonData.GetQtyPicture())
                });
            });
        }

        private async Task GetLocationsForGivenWarehouse()
        {
            await Task.Run(() =>
            {
                locationData.Clear();
                List<string> result = new List<string>();
                string error;
                var issuerLocs = Services.GetObjectList("lo", out error, spinnerAdapterList.ElementAt(temporaryPositionWarehouse).Text);
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
                        locationData.Add(location);
                    });
                }
            });
        }
    }
}