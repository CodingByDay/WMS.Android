using Android.Content;
using Android.Content.PM;
using Android.Graphics.Drawables;
using Android.Media;
using Android.Net;
using Android.Preferences;
using Android.Views;
using AndroidX.AppCompat.View.Menu;
using BarCode2D_Receiver;
using Com.Jsibbold.Zoomage;

using Newtonsoft.Json;
using System.Collections.Concurrent;
using TrendNET.WMS.Device.App;
using TrendNET.WMS.Device.Services;
using WMS.App;
using WMS.ExceptionStore;
using static Android.App.ActionBar;

namespace WMS
{
    [Activity(Label = "CheckStock", ScreenOrientation = ScreenOrientation.Portrait, NoHistory = true)]
    public class CheckStock : CustomBaseActivity, IBarcodeResult
    {
        private CustomAutoCompleteTextView cbWarehouses;
        private CustomAutoCompleteTextView tbLocation;
        private CustomAutoCompleteTextView tbIdent;
        private Button btShowStock;
        private Button button1;
        private SoundPool soundPool;
        private int soundPoolId;
        private Barcode2D barcode2D;
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
        private ListView listData;
        private UniversalAdapter<CheckStockAddonList> dataAdapter;
        private List<CheckStockAddonList> data = new List<CheckStockAddonList>();
        private ImageView imagePNG;
        private Dialog popupDialog;
        private ZoomageView? image;

        public async void GetBarcode(string barcode)
        {
            try
            {
                if (tbIdent.HasFocus)
                {

                    tbIdent.Text = barcode;
                    await ProcessStock();
                    showPictureIdent(tbIdent.Text);
                }
                else if (tbLocation.HasFocus)
                {

                    tbLocation.Text = barcode;
                }
            }
            catch (Exception ex)
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

                popupDialog.Window.SetLayout(LayoutParams.MatchParent, LayoutParams.WrapContent);
                popupDialog.Window.SetBackgroundDrawableResource(Android.Resource.Color.HoloBlueBright);
                image = popupDialog.FindViewById<ZoomageView>(Resource.Id.image);
                image.SetMinimumHeight(500);

                image.SetMinimumWidth(800);

                image.SetImageDrawable(d);

                // Access Popup layout fields like below
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }
        private async Task<string> LoadStockFromStockSerialNo(string warehouse, string location, string ident)
        {
            try
            {
                try
                {
                    var picture = await CommonData.GetQtyPictureAsync(this);
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
                        return string.Join("\r\n", stock.Items.Select(x => "L:" + x.GetString("Location") + " = " + x.GetDouble("RealStock").ToString(picture)).ToArray());
                    }
                }
                catch (Exception err)
                {
                    SentrySdk.CaptureException(err);
                    return "";
                }
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
                return string.Empty;
            }
        }







        private async Task ProcessStock()
        {
            try
            {
                var wh = spinnerAdapterList.ElementAt(temporaryPositionWarehouse);
                if (wh == null)
                {
                    return;
                }

                if (!string.IsNullOrEmpty(tbLocation.Text.Trim()))
                {
                    if (!await CommonData.IsValidLocationAsync(wh.ID, tbLocation.Text.Trim(), this))
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

                stock = await LoadStockFromStockSerialNo(wh.ID, tbLocation.Text.Trim(), tbIdent.Text.Trim());
                lbStock.Text = $"{Resources.GetString(Resource.String.s155)}:\r\n" + stock;
                isEmptyStock();
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private void isEmptyStock()
        {
            try
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
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private void color()
        {
            try
            {
                tbIdent.SetBackgroundColor(Android.Graphics.Color.Aqua);
                tbLocation.SetBackgroundColor(Android.Graphics.Color.Aqua);
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
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
                return false;
            }
        }

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                SetTheme(Resource.Style.AppTheme_NoActionBar);

                // Create your application here.
                if (App.Settings.tablet)
                {
                    base.RequestedOrientation = ScreenOrientation.Landscape;
                    base.SetContentView(Resource.Layout.CheckStockTablet);
                    imagePNG = FindViewById<ImageView>(Resource.Id.imagePNG);
                    listData = FindViewById<ListView>(Resource.Id.listData);
                    dataAdapter = UniversalAdapterHelper.GetCheckStock(this, data);
                    listData.Adapter = dataAdapter;
                }
                else
                {
                    base.RequestedOrientation = ScreenOrientation.Portrait;
                    base.SetContentView(Resource.Layout.CheckStock);
                }

                AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
                var _customToolbar = new CustomToolbar(this, toolbar, Resource.Id.navIcon);
                _customToolbar.SetNavigationIcon(App.Settings.RootURL + "/Services/Logo");
                SetSupportActionBar(_customToolbar._toolbar);
                SupportActionBar.SetDisplayShowTitleEnabled(false);

                cbWarehouses = FindViewById<CustomAutoCompleteTextView>(Resource.Id.cbWarehouses);
                tbLocation = FindViewById<CustomAutoCompleteTextView>(Resource.Id.tbLocation);
                tbIdent = FindViewById<CustomAutoCompleteTextView>(Resource.Id.tbIdent);

                btShowStock = FindViewById<Button>(Resource.Id.btShowStock);
                btShowStock.Click += BtShowStock_Click;

                button1 = FindViewById<Button>(Resource.Id.button1);
                button1.Click += Button1_Click;
                lbStock = FindViewById<TextView>(Resource.Id.lbStock);

                color();


                barcode2D = new Barcode2D(this, this);
                // First load the warehouses.
                var whs = await CommonData.ListWarehousesAsync();
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

                var dw = await CommonData.GetSettingAsync("DefaultWarehouse", this);
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
                new IntentFilter(ConnectivityManager.ConnectivityAction), ReceiverFlags.NotExported);

                cbWarehouses.ItemClick += CbWarehouses_ItemClick;
                tbLocation.ItemClick += TbLocation_ItemClick;
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }


        private void showPictureIdent(string ident)
        {
            try
            {
                try
                {
                    string wh = spinnerAdapterList.ElementAt(temporaryPositionWarehouse).ID;
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
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }
        private void TbLocation_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            try
            {

            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private async void CbWarehouses_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            try
            {
                if (e.Position != 0)
                {
                    temporaryPositionWarehouse = e.Position;
                }
                await GetLocationsForGivenWarehouse(spinnerAdapterList.ElementAt(temporaryPositionWarehouse).ID);
                DataAdapterLocation = new CustomAutoCompleteAdapter<string>(this,
                Android.Resource.Layout.SimpleSpinnerItem, locationData);
                tbLocation.Adapter = null;
                tbLocation.Adapter = DataAdapterLocation;
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private void UpdateSuggestions(string userInput)
        {
            try
            {
                // Provide custom suggestions based on user input
                List<string> suggestions = GetCustomSuggestions(userInput);
                // Clear the existing suggestions and add the new ones
                tbIdentAdapter.Clear();
                tbIdentAdapter.AddAll(suggestions);
                tbIdentAdapter.NotifyDataSetChanged();
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private List<string> GetCustomSuggestions(string userInput)
        {
            try
            {
                if (savedIdents != null)
                {
                    // In order to improve performance try to implement paralel processing. 23.05.2024 Janko Jovičić

                    var lowerUserInput = userInput.ToLower();
                    var result = new ConcurrentBag<string>();

                    Parallel.ForEach(savedIdents, suggestion =>
                    {
                        if (suggestion.ToLower().Contains(lowerUserInput))
                        {
                            result.Add(suggestion);
                        }
                    });

                    return result.Take(100).ToList();
                }

                // Service not yet loaded. 6.6.2024 J.J
                return new List<string>();
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
                return new List<string>();
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

        private void SpinnerLocation_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            try
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
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private void SpinnerIdent_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            try
            {
                tbIdent.Text = identData.ElementAt(e.Position);
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        public override void OnBackPressed()
        {
            try
            {
                base.OnBackPressed();
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private void Button1_Click(object sender, System.EventArgs e)
        {
            try
            {
                this.Finish();
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private async void BtShowStock_Click(object sender, System.EventArgs e)
        {
            try
            {
                data.Clear();
                await ProcessStock();
                if (App.Settings.tablet)
                {
                    await fillItemsOfList();
                    showPictureIdent(tbIdent.Text);
                }
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private async Task fillItemsOfList()
        {
            try
            {
                var wh = spinnerAdapterList.ElementAt(temporaryPositionWarehouse);
                string error;
                var stock = Services.GetObjectList("str", out error, wh.ID + "||" + tbIdent.Text);
                var picture = await CommonData.GetQtyPictureAsync(this);
                // return string.Join("\r\n", stock.Items.Select(x => "L:" + x.GetString("Location") + " = " + x.GetDouble("RealStock").ToString(CommonData.GetQtyPicture())).ToArray());
                stock.Items.ForEach(x =>
                {
                    data.Add(new CheckStockAddonList
                    {
                        Ident = x.GetString("Ident"),
                        Location = x.GetString("Location"),
                        Quantity = x.GetDouble("RealStock").ToString(picture)
                    });
                });
                dataAdapter.NotifyDataSetChanged();
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private async Task GetLocationsForGivenWarehouse(string warehouse)
        {
            try
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
                        DialogHelper.ShowDialogError(this, this, $"{Resources.GetString(Resource.String.s225)}");
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
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private async void CbWarehouses_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            try
            {
                Spinner spinner = (Spinner)sender;
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
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }
    }
}