using Android.Content;
using Android.Content.PM;
using Android.Media;
using Android.Net;
using Android.Text;
using Android.Views;
using BarCode2D_Receiver;
using Newtonsoft.Json;
using TrendNET.WMS.Core.Data;
using TrendNET.WMS.Device.App;
using TrendNET.WMS.Device.Services;
using WMS.App;
using WMS.ExceptionStore;
using static EventBluetooth;
using AlertDialog = Android.App.AlertDialog;
using Exception = System.Exception;

namespace WMS
{


    [Activity(Label = "WMS")]
    public class IssuedGoodsIdentEntryWithTrail : CustomBaseActivity, IBarcodeResult
    {

        private NameValueObject moveHead = (NameValueObject)InUseObjects.Get("MoveHead");
        private NameValueObject openOrder = (NameValueObject)InUseObjects.Get("OpenOrder");
        private NameValueObject trailFilters = (NameValueObject)InUseObjects.Get("TrailFilters");
        private EditText tbOrder;
        private EditText tbReceiver;
        private EditText tbIdentFilter;
        private EditText tbLocationFilter;
        private ListView ivTrail;
        private Button btConfirm;
        private Button btBack;
        private Button btDisplayPositions;
        private Button btLogout;
        SoundPool soundPool;
        int soundPoolId;
        private Barcode2D barcode2D;
        private List<Trail> trails = new List<Trail>();
        private TrailAdapter adapterObj;
        public int selected;
        private string password;
        private Trail chosen;
        private IEnumerable<NameValueObject> openOrderLocal;
        private NameValueObject openIdent;

        public bool isBound = false;

        private EventBluetooth send;
        private MyOnItemLongClickListener listener;
        private ApiResultSet result;
        private NameValueObjectList NameValueObjectVariableList;
        private ListView listData;
        private UniversalAdapter<Trail> dataAdapter;

        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            try
            {
                switch (keyCode)
                {
                    // in smartphone
                    case Keycode.F3:
                        if (btConfirm.Enabled == true)
                        {
                            BtConfirm_Click(this, null);
                        }
                        break;
                    // return true;
                    case Keycode.F4:
                        if (btDisplayPositions.Enabled == true)
                        {
                            BtDisplayPositions_Click(this, null);
                        }
                        break;
                    case Keycode.F8:
                        if (btLogout.Enabled == true)
                        {
                            BtLogout_Click(this, null);
                        }
                        break;

                }
                return base.OnKeyDown(keyCode, e);
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
                return false;
            }
        }

        public void GetBarcode(string barcode)
        {
            try
            {
                try
                {
                    if (barcode != "Scan fail" && barcode != "")
                    {
                        if (tbIdentFilter.HasFocus)
                        {

                            if (HelperMethods.is2D(barcode))
                            {
                                Parser2DCode parser2DCode = new Parser2DCode(barcode.Trim());

                                chooseIdent(parser2DCode);
                            }
                            else if (!CheckIdent(barcode) && barcode.Length > 17 && barcode.Contains("400"))
                            {
                                var ident = barcode.Substring(0, barcode.Length - 16);
                                tbIdentFilter.Text = ident;
                                RunOnUiThread(() => { adapterObj.Filter(trails, true, tbIdentFilter.Text, false, ivTrail); });
                                if (adapterObj.returnNumberOfItems() == 0)
                                {
                                    tbIdentFilter.Text = string.Empty;
                                } 
                                chooseIdentOnly(ident);
                            }
                            else
                            {
                                tbIdentFilter.Text = barcode;
                                RunOnUiThread(() => { adapterObj.Filter(trails, true, tbIdentFilter.Text, false, ivTrail); });
                                if (adapterObj.returnNumberOfItems() == 0)
                                {
                                    tbIdentFilter.Text = string.Empty;
                                } 

                            }
                        }
                        else if (tbLocationFilter.HasFocus)
                        {

                            tbLocationFilter.Text = barcode;
                            RunOnUiThread(() => { adapterObj.Filter(trails, false, tbLocationFilter.Text, false, ivTrail); });
                            if (adapterObj.returnNumberOfItems() == 0)
                            {
                                tbIdentFilter.Text = string.Empty;
                            }
                        }
                    }


                    var dataToUpdate = adapterObj.returnData();
                    listener.updateData(dataToUpdate);
                }
                catch (Exception err)
                {
                    SentrySdk.CaptureException(err);
                    return;
                }
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }




        private bool CheckIdent(string barcode)
        {
            try
            {
                if (string.IsNullOrEmpty(barcode)) { return false; }
                try
                {
                    string error;
                    openIdent = Services.GetObject("id", barcode, out error);
                    if (openIdent != null)
                    {
                        return true;
                    }
                }
                catch (Exception)
                {
                    return false;
                }

                return false;
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
                return false;
            }
        }


        private void chooseIdent(Parser2DCode code)
        {
            try
            {
                string convertedIdent = string.Empty;
                string ident = string.Empty;
                string error;
                openIdent = Services.GetObject("id", code.ident, out error);

                if (openIdent != null)
                {
                    convertedIdent = openIdent.GetString("Code");
                    ident = convertedIdent;

                }
                else
                {
                    return;
                }

                RunOnUiThread(() => { adapterObj.Filter(trails, true, ident, false, ivTrail); });
                int numberOfHits = adapterObj.returnNumberOfItems();
                if (numberOfHits == 0)
                {

                    RunOnUiThread(() => { adapterObj.Filter(trails, true, string.Empty, false, ivTrail); });
                    return;

                }
                else if (numberOfHits == 1)
                {
                    Trail trail = adapterObj.returnData().ElementAt(0);

                    if (trail.Location == string.Empty)
                    {
                        Toast.MakeText(this, $"{Resources.GetString(Resource.String.s230)}", ToastLength.Long).Show();
                        return;
                    }

                    if (SaveMoveHeadObjectMode(trail))
                    {
                        Intent i = new Intent(Application.Context, typeof(IssuedGoodsSerialOrSSCCEntry));
                        code.__helper__position = trail.No;
                        code.__helper__convertedOrder = trail.Key;
                        code.ident = trail.Ident;
                        Base.Store.code2D = code;
                        StartActivity(typeof(IssuedGoodsSerialOrSSCCEntry));
                        this.Finish();
                    }

                }
                else if (numberOfHits > 1)
                {
                    return;
                }


                listener.updateData(adapterObj.returnData());
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private void chooseIdentOnly(string charge)
        {
            try
            {
                int numberOfHits = adapterObj.returnNumberOfItems();
                if (numberOfHits == 0)
                {
                    RunOnUiThread(() => { adapterObj.Filter(trails, true, string.Empty, false, ivTrail); });
                    return;
                }
                else if (numberOfHits == 1)
                {
                    Trail trail = adapterObj.returnData().ElementAt(0);
                    if (trail.Location == string.Empty)
                    {
                        Toast.MakeText(this, $"{Resources.GetString(Resource.String.s230)}", ToastLength.Long).Show();
                        return;
                    }
                    InUseObjects.Set("OpenOrder", trails.ElementAt(adapterObj.ReturnSelected().originalIndex));

                    if (SaveMoveHeadObjectMode(trail))
                    {
                        if (trails.Count - 1 == 1)
                        {
                            var lastItem = new NameValueObject("LastItem");
                            lastItem.SetBool("IsLastItem", true);
                            InUseObjects.Set("LastItem", lastItem);
                        }
                        Intent i = new Intent(Application.Context, typeof(IssuedGoodsSerialOrSSCCEntry));
                        i.PutExtra("ident", trail.Ident);
                        i.PutExtra("qty", trail.Qty);
                        i.PutExtra("selected", Trail.Serialize(trail));
                        i.PutExtra("scan", true);
                        StartActivity(i);
                        this.Finish();
                    }
                }
                else if (numberOfHits > 1)
                {
                    return;
                }
                listener.updateData(adapterObj.returnData());
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }





        private async Task FillDisplayedOrderInfo()
        {
            try
            {
                await Task.Run(async () =>
                {

                    try
                    {
                        List<Trail> unfiltered = new List<Trail>();
                        var filterLoc = tbLocationFilter.Text;
                        var filterIdent = tbIdentFilter.Text;

                        try
                        {
                            if (openOrder != null)
                            {
                                RunOnUiThread(() =>
                                {
                                    tbOrder.Text = openOrder.GetString("Key");
                                    tbReceiver.Text = openOrder.GetString("Receiver");
                                });

                                password = openOrder.GetString("Key");

                            }
                            else if (moveHead != null)
                            {
                                RunOnUiThread(() =>
                                {
                                    tbOrder.Text = moveHead.GetString("LinkKey");
                                    tbReceiver.Text = moveHead.GetString("Receiver");
                                });
                                password = moveHead.GetString("LinkKey");
                            }

                            string error;

                            var warehouse = moveHead.GetString("Wharehouse");
                            // qtyByLoc = Services.GetObjectList("ook", out error, password);
                            var parameters = new List<Services.Parameter>();
                            parameters.Add(new Services.Parameter { Name = "acKey", Type = "String", Value = password });

                            // This change is made because serial number and sscc are not shown and can result in many duplicate entries. 13.05.2024 Janko Jovičić
                            string sql = $"SELECT DISTINCT acIdent, acName, anQty, anNo, acKey, acSubject, aclocation, anPackQty FROM uWMSOrderItemByKeyOut WHERE acKey = @acKey;";

                            result = await AsyncServices.AsyncServices.GetObjectListBySqlAsync(sql, parameters, this);

                            if (!result.Success)
                            {
                                RunOnUiThread(() =>
                                {
                                    AlertDialog.Builder alert = new AlertDialog.Builder(this);
                                    alert.SetTitle($"{Resources.GetString(Resource.String.s265)}");
                                    alert.SetMessage($"{result.Error}");
                                    alert.SetPositiveButton("Ok", (senderAlert, args) =>
                                    {
                                        alert.Dispose();
                                    });
                                    Dialog dialog = alert.Create();
                                    dialog.Show();
                                });


                                return;
                            }



                            NameValueObjectVariableList = result.ConvertToNameValueObjectList("OpenOrder");
                            if (result != null && result.Success && result.Rows.Count > 0)
                            {
                                trails.Clear();
                                int counter = 0;
                                foreach (var row in result.Rows)
                                {
                                    var ident = row.StringValue("acIdent");
                                    var location = row.StringValue("aclocation");
                                    var name = row.StringValue("acName");

                                    if ((string.IsNullOrEmpty(filterLoc) || (location == filterLoc)) &&
                                       (string.IsNullOrEmpty(filterIdent) || (ident == filterIdent)))
                                    {

                                        var key = row.StringValue("acKey");
                                        var lvi = new Trail();
                                        lvi.Key = key;
                                        lvi.Ident = ident;
                                        lvi.Location = location;
                                        lvi.Qty = string.Format("{0:###,##0.00}", row.DoubleValue("anQty"));
                                        lvi.originalIndex = counter;
                                        lvi.No = (int)row.IntValue("anNo");
                                        lvi.Name = name;
                                        lvi.Packaging = row.DoubleValue("anPackQty") ?? -1;
                                        counter++;
                                        unfiltered.Add(lvi);

                                    }
                                }
                            }


                            RunOnUiThread(() =>
                            {
                                trails = unfiltered;
                                adapterObj.NotifyDataSetChanged();
                                RunOnUiThread(() => { adapterObj.Filter(trails, true, string.Empty, false, ivTrail); });
                                listener = new MyOnItemLongClickListener(this, adapterObj.returnData(), adapterObj);
                                ivTrail.OnItemLongClickListener = listener;
                              
                                /*

                                try                                
                                {

                                    SendDataToDevice();

                                } catch (Exception ex)
                                {
                                    SentrySdk.CaptureException(ex);
                                }

                                */

                               

                            });
                        }
                        catch (Exception error)
                        {
                            var e = error;
                            SentrySdk.CaptureException(e);
                        }
                    }
                    catch (Exception error)
                    {
                        SentrySdk.CaptureException(error);
                    }
                });
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

    
        private async Task FillDisplayedOrderInfoMultipleLocations()
        {
            try
            {
                await Task.Run(async () =>
                {
                    try
                    {
                        List<Trail> unfiltered = new List<Trail>();
                        var filterLoc = tbLocationFilter.Text;
                        var filterIdent = tbIdentFilter.Text;

                        try
                        {
                            if (openOrder != null)
                            {
                                RunOnUiThread(() =>
                                {
                                    tbOrder.Text = openOrder.GetString("Key");
                                    tbReceiver.Text = openOrder.GetString("Receiver");
                                });

                                password = openOrder.GetString("Key");

                            }
                            else if (moveHead != null)
                            {
                                RunOnUiThread(() =>
                                {
                                    tbOrder.Text = moveHead.GetString("LinkKey");
                                    tbReceiver.Text = moveHead.GetString("Receiver");
                                });
                                password = moveHead.GetString("LinkKey");
                            }

                            string error;

                            var warehouse = moveHead.GetString("Wharehouse");
                            // qtyByLoc = Services.GetObjectList("ook", out error, password);
                            var parameters = new List<Services.Parameter>();
                            parameters.Add(new Services.Parameter { Name = "acKey", Type = "String", Value = password });

                            // New extra way of showing the data. 21.05.2024 Janko Jovičić
                            string sql = $"SELECT acIdent, anLocation, acName, acActualLocation, acKey, anNo, anPackQty, anQty FROM uWMSOrderItemByKeyOutSUM WHERE acKey = @acKey;";

                            result = await AsyncServices.AsyncServices.GetObjectListBySqlAsync(sql, parameters, this);

                            if (!result.Success)
                            {
                                RunOnUiThread(() =>
                                {
                                    AlertDialog.Builder alert = new AlertDialog.Builder(this);
                                    alert.SetTitle($"{Resources.GetString(Resource.String.s265)}");
                                    alert.SetMessage($"{result.Error}");
                                    alert.SetPositiveButton("Ok", (senderAlert, args) =>
                                    {
                                        alert.Dispose();
                                    });
                                    Dialog dialog = alert.Create();
                                    dialog.Show();
                                });

                                return;
                            }


                            NameValueObjectVariableList = result.ConvertToNameValueObjectList("OpenOrder");
                            if (result.Success && result.Rows.Count > 0)
                            {
                                trails.Clear();
                                int counter = 0;
                                foreach (var row in result.Rows)
                                {
                                    var ident = row.StringValue("acIdent");
                                    var location = row.IntValue("anLocation");
                                    var name = row.StringValue("acName");
                                    var actualLocation = row.StringValue("acActualLocation");
                                    if ((string.IsNullOrEmpty(filterLoc) || (location.ToString() == filterLoc)) &&
                                       (string.IsNullOrEmpty(filterIdent) || (ident == filterIdent)))
                                    {

                                        var key = row.StringValue("acKey");
                                        var lvi = new Trail();
                                        lvi.Key = key;
                                        lvi.Ident = ident;


                                        if (location > 1)
                                        {
                                            lvi.Location = Resources.GetString(Resource.String.s346);
                                        }
                                        else if (location == 1)
                                        {
                                            /* Extra field for the use case where users want to see instantly on which location
                                             is the item present in the case of only one. 6.7.2024 JJ */
                                            lvi.Location = actualLocation;
                                        }
                                        else if (location <= 0)
                                        {
                                            lvi.Location = Resources.GetString(Resource.String.s345);
                                        }

                                        lvi.Qty = string.Format("{0:###,##0.00}", row.DoubleValue("anQty"));
                                        lvi.originalIndex = counter;
                                        lvi.No = (int)row.IntValue("anNo");
                                        lvi.Name = name;
                                        lvi.Packaging = row.DoubleValue("anPackQty") ?? -1;
                                        counter++;
                                        unfiltered.Add(lvi);

                                    }
                                }
                            }


                            RunOnUiThread(() =>
                            {
                                trails = unfiltered;
                                adapterObj.NotifyDataSetChanged();
                                RunOnUiThread(() => { adapterObj.Filter(trails, true, string.Empty, false, ivTrail); });
                                listener = new MyOnItemLongClickListener(this, adapterObj.returnData(), adapterObj);
                                ivTrail.OnItemLongClickListener = listener;

                                /*try
                                {

                                    SendDataToDevice();

                                }
                                catch (Exception ex)
                                {
                                    SentrySdk.CaptureException(ex);
                                }*/

                            });
                        }
                        catch (Exception error)
                        {
                            var e = error;
                            SentrySdk.CaptureException(e);
                        }
                    }
                    catch (Exception error)
                    {
                        SentrySdk.CaptureException(error);
                    }
                });
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }


        protected async override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                SetTheme(Resource.Style.AppTheme_NoActionBar);
                // Create your application here
                if (App.Settings.tablet)
                {
                    base.RequestedOrientation = ScreenOrientation.Landscape;
                    base.SetContentView(Resource.Layout.IssuedGoodsIdentEntryWithTrailTablet);
                    listData = FindViewById<ListView>(Resource.Id.listData);
                    dataAdapter = UniversalAdapterHelper.GetIssueGoodsIdentWithTrail(this, trails);
                    listData.Adapter = dataAdapter;

                }
                else
                {
                    base.RequestedOrientation = ScreenOrientation.Portrait;
                    base.SetContentView(Resource.Layout.IssuedGoodsIdentEntryWithTrail);
                }
                LoaderManifest.LoaderManifestLoopResources(this);
   
                AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
                var _customToolbar = new CustomToolbar(this, toolbar, Resource.Id.navIcon);
                _customToolbar.SetNavigationIcon(App.Settings.RootURL + "/Services/Logo");
                SetSupportActionBar(_customToolbar._toolbar);
                SupportActionBar.SetDisplayShowTitleEnabled(false);
                tbOrder = FindViewById<EditText>(Resource.Id.tbOrder);
                tbReceiver = FindViewById<EditText>(Resource.Id.tbReceiver);
                tbIdentFilter = FindViewById<EditText>(Resource.Id.tbIdentFilter);
                tbLocationFilter = FindViewById<EditText>(Resource.Id.tbLocationFilter);
                ivTrail = FindViewById<ListView>(Resource.Id.ivTrail);
                btConfirm = FindViewById<Button>(Resource.Id.btConfirm);
                btDisplayPositions = FindViewById<Button>(Resource.Id.btDisplayPositions);
                btBack = FindViewById<Button>(Resource.Id.btBack);
                btBack.Click += BtBack_Click;
                btLogout = FindViewById<Button>(Resource.Id.btLogout);
                barcode2D = new Barcode2D(this, this);
                color();
                tbLocationFilter.FocusChange += TbLocationFilter_FocusChange;
                trails = new List<Trail>();
                adapterObj = new TrailAdapter(this, trails);
                ivTrail.Adapter = adapterObj;
                ivTrail.ItemClick += IvTrail_ItemClick;
                btConfirm.Click += BtConfirm_Click;
                btDisplayPositions.Click += BtDisplayPositions_Click;
                btLogout.Click += BtLogout_Click;

                if (openOrder == null && moveHead == null)
                {
                    StartActivity(typeof(MainMenu));
                }

                if (trailFilters != null)
                {
                    tbIdentFilter.Text = trailFilters.GetString("Ident");
                    tbLocationFilter.Text = trailFilters.GetString("Location");
                }

                // New proccess for more locations for SkiSea 21.05.2024 Janko Jovičić

                if (await CommonData.GetSettingAsync("IssueSummaryView", this) == "1")
                {
                    await FillDisplayedOrderInfoMultipleLocations();
                }
                else
                {
                    await FillDisplayedOrderInfo();
                }



                var _broadcastReceiver = new NetworkStatusBroadcastReceiver();
                _broadcastReceiver.ConnectionStatusChanged += OnNetworkStatusChanged;
                Application.Context.RegisterReceiver(_broadcastReceiver,
                new IntentFilter(ConnectivityManager.ConnectivityAction), ReceiverFlags.NotExported);


                tbIdentFilter.AfterTextChanged += TbIdentFilter_AfterTextChanged;
                tbLocationFilter.AfterTextChanged += TbLocationFilter_AfterTextChanged;


                
                
                tbIdentFilter.RequestFocus();

                if (adapterObj.sList.Count > 0)
                {
                    HelperMethods.SelectPositionProgramaticaly(ivTrail, 0);
                    adapterObj.SetSelected(0);
                    chosen = adapterObj.ReturnSelected();
                }

                LoaderManifest.LoaderManifestLoopStop(this);
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }


        public class MyOnItemLongClickListener : Java.Lang.Object, AdapterView.IOnItemLongClickListener
        {
            Context context_;
            List<Trail> data_;
            TrailAdapter adapter_;


            public void updateData(List<Trail> data)
            {
                try
                {
                    data_ = data;
                }
                catch (Exception ex)
                {
                    GlobalExceptions.ReportGlobalException(ex);
                }
            }

            public MyOnItemLongClickListener(Context context, List<Trail> data, TrailAdapter adapter)
            {
                try
                {
                    context_ = context;
                    data_ = data;
                    adapter_ = adapter;
                }
                catch (Exception ex)
                {
                    GlobalExceptions.ReportGlobalException(ex);
                }
            }

            public bool OnItemLongClick(AdapterView parent, View view, int position, long id)
            {
                try
                {
                    adapter_.SetSelected(position);
                    Trail selected = data_.ElementAt(position);
                    AlertDialog.Builder builder = new AlertDialog.Builder(context_);
                    builder.SetTitle($"{context_.Resources.GetString(Resource.String.s256)}");
                    builder.SetMessage($"{context_.Resources.GetString(Resource.String.s257)}: {selected.Ident}\n{context_.Resources.GetString(Resource.String.s258)}: {selected.Location}\n{context_.Resources.GetString(Resource.String.s259)}: {selected.Key}\n{context_.Resources.GetString(Resource.String.s260)}: {selected.Name}");
                    builder.SetPositiveButton("OK", (s, args) =>
                    {
                    });
                    AlertDialog alertDialog = builder.Create();
                    alertDialog.Show();
                    return true; // Return true to consume the long click event
                }
                catch (Exception ex)
                {
                    GlobalExceptions.ReportGlobalException(ex);
                    return false;
                }
            }
        }




        private void TbLocationFilter_AfterTextChanged(object sender, AfterTextChangedEventArgs e)
        {
            try
            {
                try
                {
                    RunOnUiThread(() => { adapterObj.Filter(trails, false, tbLocationFilter.Text, false, ivTrail); });
                    listener.updateData(adapterObj.returnData());
                }
                catch
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private void TbIdentFilter_AfterTextChanged(object sender, AfterTextChangedEventArgs e)
        {
            try
            {
                try
                {
                    RunOnUiThread(() => { adapterObj.Filter(trails, true, tbIdentFilter.Text, false, ivTrail); });
                    listener.updateData(adapterObj.returnData());
                }
                catch
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }
      

     

        private void BtBack_Click(object sender, EventArgs e)
        {
            try
            {
                OnBackPressed();
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
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

        private void TbLocationFilter_FocusChange(object sender, View.FocusChangeEventArgs e)
        {
            try { }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private void color()
        {
            try
            {
                tbIdentFilter.SetBackgroundColor(Android.Graphics.Color.Aqua);
                tbLocationFilter.SetBackgroundColor(Android.Graphics.Color.Aqua);
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private void BtLogout_Click(object sender, EventArgs e)
        {
            try
            {
                StartActivity(typeof(MainMenu));
                Finish();
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private void BtDisplayPositions_Click(object sender, EventArgs e)
        {
            try
            {
                StartActivity(typeof(IssuedGoodsEnteredPositionsView));
                Finish();
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private void BtConfirm_Click(object sender, EventArgs e)
        {
            try
            {
                try
                {
                    if (adapterObj.ReturnSelected() != null)
                    {
                        if (string.IsNullOrEmpty(adapterObj.ReturnSelected().Location))
                        {
                            RunOnUiThread(() =>
                            {
                                Toast.MakeText(this, $"{Resources.GetString(Resource.String.s230)}", ToastLength.Long).Show();
                            });
                            return;
                        }
                        else
                        {
                            string error;
                            var toSave = Services.GetObject("oobl", adapterObj.ReturnSelected().Key + "|" + adapterObj.ReturnSelected().No, out error);
                            openOrder = toSave;
                            InUseObjects.Set("OpenOrder", toSave);
                            var openIdent = Services.GetObject("id", openOrder.GetString("Ident"), out error);
                            InUseObjects.Set("OpenIdent", openIdent);
                        }

                    }
                    else
                    {
                        RunOnUiThread(() =>
                        {
                            Toast.MakeText(this, "Izdelek ni izbran.", ToastLength.Long).Show();
                        });

                        return;
                    }


                    if (SaveMoveHead())
                    {
                        Base.Store.isUpdate = false;
                        if (trails.Count()  == 1)
                        {
                            var lastItem = new NameValueObject("LastItem");
                            lastItem.SetBool("IsLastItem", true);
                            InUseObjects.Set("LastItem", lastItem);
                            var obj = adapterObj.ReturnSelected();
                            var ident = obj.Ident;
                            var qty = obj.Qty;
                            Intent i = new Intent(Application.Context, typeof(IssuedGoodsSerialOrSSCCEntry));
                            i.PutExtra("ident", ident);
                            i.PutExtra("qty", qty);
                            string jsonString = JsonConvert.SerializeObject(obj);
                            i.PutExtra("selected", jsonString);
                            StartActivity(i);
                            Finish();
                        }
                        else
                        {
                            var obj = adapterObj.ReturnSelected();
                            var ident = obj.Ident;
                            var qty = obj.Qty;
                            Intent i = new Intent(Application.Context, typeof(IssuedGoodsSerialOrSSCCEntry));
                            i.PutExtra("ident", ident);
                            i.PutExtra("qty", qty);
                            string jsonString = JsonConvert.SerializeObject(obj);
                            i.PutExtra("selected", jsonString);
                            StartActivity(i);
                            Finish();
                        }
                    }
                }
                catch (Exception err)
                {
                    SentrySdk.CaptureException(err);
                    StartActivity(typeof(MainMenu));
                }
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }



        private void IvTrail_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            try
            {
                adapterObj.SetSelected(e.Position);
                chosen = adapterObj.ReturnSelected();

                // Save this to the global state variable // 16.04.2024
                // Base.Store.OpenOrder = new OpenOrder { Order = chosen.Key, Position = chosen.No, Client = chosen.}
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private bool SaveMoveHeadObjectMode(Trail trail)
        {
            try
            {
                if (trail == null) { return false; }
                var obj = trail;
                var ident = obj.Ident;
                var location = obj.Location;
                var qty = Convert.ToDouble(obj.Qty);
                var extraData = new NameValueObject("ExtraData");
                extraData.SetString("Location", location);
                extraData.SetDouble("Qty", qty);
                InUseObjects.Set("ExtraData", extraData);
                string error;
                try
                {
                    var openIdent = Services.GetObject("id", ident, out error);
                    if (openIdent == null)
                    {
                        string WebError = string.Format($"{Resources.GetString(Resource.String.s229)}" + error);
                        Toast.MakeText(this, WebError, ToastLength.Long).Show();
                        return false;
                    }
                    InUseObjects.Set("OpenIdent", openIdent);
                }
                catch (Exception err)
                {
                    SentrySdk.CaptureException(err);
                }
                if (!moveHead.GetBool("Saved"))
                {

                    try
                    {
                        var test = openOrder.GetString("No");
                        moveHead.SetInt("Clerk", Services.UserID());
                        moveHead.SetString("Type", "P");
                        moveHead.SetString("LinkKey", openOrder.GetString("Key"));
                        moveHead.SetString("LinkNo", openOrder.GetString("No"));
                        moveHead.SetString("Document1", openOrder.GetString("Document1"));
                        moveHead.SetDateTime("Document1Date", openOrder.GetDateTime("Document1Date"));
                        moveHead.SetString("Note", openOrder.GetString("Note"));
                        string testDocument1 = openOrder.GetString("Document1");
                        if (moveHead.GetBool("ByOrder"))
                        {
                            moveHead.SetString("Receiver", openOrder.GetString("Receiver"));
                        }
                        var savedMoveHead = Services.SetObject("mh", moveHead, out error);
                        if (savedMoveHead == null)
                        {
                            string WebError = string.Format($"{Resources.GetString(Resource.String.s213)}" + error);
                            Toast.MakeText(this, WebError, ToastLength.Long).Show();
                            return false;
                        }
                        else
                        {
                            moveHead.SetInt("HeadID", savedMoveHead.GetInt("HeadID"));
                            moveHead.SetBool("Saved", true);
                            return true;
                        }
                    }
                    catch (Exception err)
                    {
                        SentrySdk.CaptureException(err);
                        return false;
                    }
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
                return false;
            }
        }

        /* Save move head method. */
        private bool SaveMoveHead()
        {
            try
            {
                var obj = adapterObj.ReturnSelected();
                var ident = obj.Ident;
                var location = obj.Location;
                var qty = Convert.ToDouble(obj.Qty);
                var extraData = new NameValueObject("ExtraData");
                extraData.SetString("Location", location);
                extraData.SetDouble("Qty", qty);
                InUseObjects.Set("ExtraData", extraData);
                string error;

                if (!moveHead.GetBool("Saved"))
                {

                    try
                    {
                        var test = openOrder.GetString("No");
                        moveHead.SetInt("Clerk", Services.UserID());
                        moveHead.SetString("Type", "P");
                        moveHead.SetString("LinkKey", openOrder.GetString("Key"));
                        moveHead.SetString("LinkNo", openOrder.GetString("No"));
                        moveHead.SetString("Document1", openOrder.GetString("Document1"));
                        moveHead.SetDateTime("Document1Date", openOrder.GetDateTime("Document1Date"));
                        moveHead.SetString("Note", openOrder.GetString("Note"));
                        string testDocument1 = openOrder.GetString("Document1");
                        if (moveHead.GetBool("ByOrder"))
                        {
                            moveHead.SetString("Receiver", openOrder.GetString("Receiver"));
                        }
                        var savedMoveHead = Services.SetObject("mh", moveHead, out error);
                        if (savedMoveHead == null)
                        {
                            string WebError = string.Format($"{Resources.GetString(Resource.String.s213)}" + error);
                            Toast.MakeText(this, WebError, ToastLength.Long).Show();
                            return false;
                        }
                        else
                        {
                            moveHead.SetInt("HeadID", savedMoveHead.GetInt("HeadID"));
                            moveHead.SetBool("Saved", true);
                            return true;
                        }
                    }
                    catch (Exception err)
                    {
                        SentrySdk.CaptureException(err);
                        return false;
                    }
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
                return false;
            }
        }



    }
}