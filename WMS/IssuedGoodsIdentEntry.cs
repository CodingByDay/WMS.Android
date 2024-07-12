using Android.Content;
using Android.Content.PM;
using Android.Net;
using Android.Preferences;
using Android.Views;
using BarCode2D_Receiver;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using TrendNET.WMS.Core.Data;
using TrendNET.WMS.Device.App;
using TrendNET.WMS.Device.Services;
using WMS.App;
using WMS.ExceptionStore;
using AlertDialog = Android.App.AlertDialog;

namespace WMS
{
    [Activity(Label = "WMS")]
    public class IssuedGoodsIdentEntry : CustomBaseActivity, IBarcodeResult
    {
        private CustomAutoCompleteTextView tbIdent;
        private EditText tbNaziv;
        private EditText tbOrder;
        private EditText tbConsignee;
        private EditText tbDeliveryDeadline;
        private EditText tbQty;
        private Button btNext;
        private Button btConfirm;
        private Button button4;
        private Button button5;
        private BarCode2D_Receiver.Barcode2D barcode2D;
        private NameValueObject moveHead = (NameValueObject)InUseObjects.Get("MoveHead");
        private NameValueObject openIdent = null;
        private int displayedOrder = -1;
        private TextView lbOrderInfo;
        private NameValueObjectList openOrders = (NameValueObjectList)InUseObjects.Get("OpenOrders");
        private List<OpenOrder> orders = new List<OpenOrder>();
        private List<string> returnList = new List<string>();
        private List<string> identData = new List<string>();
        private CustomAutoCompleteAdapter<string> tbIdentAdapter;
        private List<string> savedIdents;
        private ListView? listData;
        private UniversalAdapter<OpenOrder> dataAdapter;
        private int selected;
        private int selectedItem;
        private List<string> suggestions = new List<string>();

        public async void GetBarcode(string barcode)
        {
            try
            {
                // pass
                if (tbIdent.HasFocus)
                {
                    tbIdent.Text = barcode;
                    await ProcessIdent(false);
                }
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }
        public void color()
        {
            try
            {
                tbIdent.SetBackgroundColor(Android.Graphics.Color.Aqua);
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private async Task ProcessIdent(bool cleanUp)
        {
            try
            {
                try
                {
                    LoaderManifest.LoaderManifestLoopResources(this);
                    var ident = tbIdent.Text.Trim();


                    if (cleanUp)
                    {
                        ident = string.Empty;
                    }


                    if (string.IsNullOrEmpty(ident)) {

                        orders.Clear();
                        FillDisplayedOrderInfo();
                        if (App.Settings.tablet)
                        {
                            dataAdapter.NotifyDataSetChanged();
                        }
                    }
                    try
                    {
                        string error;
                        openIdent = Services.GetObject("id", ident, out error);
                        if (openIdent == null)
                        {
                            string SuccessMessage = string.Format($"{Resources.GetString(Resource.String.s229)}" + error);
                            Toast.MakeText(this, SuccessMessage, ToastLength.Long).Show();
                            tbIdent.Text = "";
                            tbIdent.RequestFocus();
                            tbNaziv.Text = "";
                        }
                        else
                        {
                            ident = openIdent.GetString("Code");
                            if (ident != tbIdent.Text)
                            {
                                // Needed because of the bimex process. 11. jul. 2024 Janko Jovičić
                                tbIdent.Text = ident;
                            }
                            InUseObjects.Set("OpenIdent", openIdent);
                            var isPackaging = openIdent.GetBool("IsPackaging");
                            if (!moveHead.GetBool("ByOrder") || isPackaging)
                            {
                                if (SaveMoveHead())
                                {
                                    StartActivity(typeof(IssuedGoodsSerialOrSSCCEntry));
                                    Finish();
                                }
                                return;
                            }
                            else
                            {
                                tbNaziv.Text = openIdent.GetString("Name");

                                var parameters = new List<Services.Parameter>();

                                // string debug = $"SELECT * from uWMSOrderItemByItemTypeWarehouseOut WHERE acIdent = {ident} AND acDocType = {moveHead.GetString("DocumentType")} AND acWarehouse = {moveHead.GetString("Wharehouse")};";

                                string sql = $"SELECT acSubject, acKey, anNo, anQty, DeliveryDeadline, acIdent, anPackQty from uWMSOrderItemByItemTypeWarehouseOut WHERE acIdent = @acIdent AND acDocType = @acDocType AND acWarehouse = @acWarehouse";

                                if (moveHead != null)
                                {
                                    string? subject = moveHead.GetString("Receiver");
                                    if (!string.IsNullOrEmpty(subject))
                                    {
                                        sql += " AND acSubject = @acSubject  ORDER BY acKey, anNo;";
                                        parameters.Add(new Services.Parameter { Name = "acSubject", Type = "String", Value = subject });
                                    }
                                    else
                                    {
                                        sql += " ORDER BY acKey, anNo;";
                                    }
                                }
                                else
                                {
                                    StartActivity(typeof(MainMenu));
                                    Finish();
                                }

                                parameters.Add(new Services.Parameter { Name = "acIdent", Type = "String", Value = ident });
                                parameters.Add(new Services.Parameter { Name = "acDocType", Type = "String", Value = moveHead.GetString("DocumentType") });
                                parameters.Add(new Services.Parameter { Name = "acWarehouse", Type = "String", Value = moveHead.GetString("Wharehouse") });



                                var subjects = await AsyncServices.AsyncServices.GetObjectListBySqlAsync(sql, parameters);
                                orders.Clear();
                                if (!subjects.Success)
                                {
                                    RunOnUiThread(() =>
                                    {

                                        AlertDialog.Builder alert = new AlertDialog.Builder(this);
                                        alert.SetTitle($"{Resources.GetString(Resource.String.s265)}");
                                        alert.SetMessage($"{subjects.Error}");
                                        alert.SetPositiveButton("Ok", (senderAlert, args) =>
                                        {
                                            alert.Dispose();
                                        });
                                        Dialog dialog = alert.Create();
                                        dialog.Show();

                                        SentrySdk.CaptureMessage(subjects.Error);
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

                                            orders.Add(new OpenOrder
                                            {
                                                Client = row.StringValue("acSubject"),
                                                Order = row.StringValue("acKey"),
                                                Position = (int?)row.IntValue("anNo"),
                                                Quantity = row.DoubleValue("anQty"),
                                                Date = row.DateTimeValue("DeliveryDeadline"),
                                                Ident = row.StringValue("acIdent"),
                                                Packaging = row.DoubleValue("anPackQty")
                                            });

                                        }

                                        displayedOrder = 0;

                                        if (App.Settings.tablet)
                                        {
                                            dataAdapter.NotifyDataSetChanged();
                                            UniversalAdapterHelper.SelectPositionProgramaticaly(listData, 0);
                                        }
                                    }
                                }
                            }
                        }

                        FillDisplayedOrderInfo();

                        tbIdent.SetSelection(0, tbIdent.Text.Length);

                    }
                    catch (Exception err)
                    {

                        SentrySdk.CaptureException(err);
                        return;

                    }
                }
                catch (Exception ex)
                {
                    SentrySdk.CaptureException(ex);
                }
                finally
                {
                    LoaderManifest.LoaderManifestLoopStop(this);
                }
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private void FillDisplayedOrderInfo()
        {
            try
            {
                if ((openIdent != null) && (orders != null) && (orders.Count > 0))
                {
                    lbOrderInfo.Text = $"{Resources.GetString(Resource.String.s14)} (" + (displayedOrder + 1).ToString() + "/" + orders.Count.ToString() + ")";
                    var order = orders.ElementAt(displayedOrder);
                    Base.Store.OpenOrder = order;
                    tbOrder.Text = order.Order + " / " + order.Position;
                    tbConsignee.Text = order.Client;
                    tbQty.Text = order.Quantity.ToString();
                    var deadLine = order.Date;
                    tbDeliveryDeadline.Text = deadLine == null ? "" : ((DateTime)deadLine).ToString("dd.MM.yyyy");
                    btNext.Enabled = true;
                    btConfirm.Enabled = true;
                }
                else
                {
                    lbOrderInfo.Text = $"{Resources.GetString(Resource.String.s289)}";
                    tbOrder.Text = "";
                    tbConsignee.Text = "";
                    tbQty.Text = "";
                    tbDeliveryDeadline.Text = "";
                    btNext.Enabled = false;
                    btConfirm.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private bool SaveMoveHead()
        {
            try
            {
                var order = Base.Store.OpenOrder;
                string key = string.Empty;
                string client = string.Empty;
                int no = 0;

                if (!moveHead.GetBool("Saved"))
                {

                    try
                    {

                        moveHead.SetInt("Clerk", Services.UserID());
                        moveHead.SetString("Type", "P");

                        if (order != null)
                        {
                            key = order.Order;
                            client = order.Client;
                            no = order.Position ?? 0;
                        }

                        moveHead.SetString("LinkKey", key);
                        moveHead.SetString("LinkNo", no.ToString());

                        if (moveHead.GetBool("ByOrder"))
                        {
                            moveHead.SetString("Receiver", client);
                        }

                        string error;
                        var savedMoveHead = Services.SetObject("mh", moveHead, out error);

                        if (savedMoveHead == null)
                        {
                            string WebError = string.Format($"{Resources.GetString(Resource.String.s213)}" + error);
                            Toast.MakeText(this, WebError, ToastLength.Long).Show(); return false;
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

        protected async override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                SetTheme(Resource.Style.AppTheme_NoActionBar);
                if (App.Settings.tablet)
                {
                    base.RequestedOrientation = ScreenOrientation.Landscape;
                    base.SetContentView(Resource.Layout.IssuedGoodsIdentEntryTablet);
                    listData = FindViewById<ListView>(Resource.Id.listData);
                    dataAdapter = UniversalAdapterHelper.GetIssuedGoodsIdentEntry(this, orders);
                    listData.ItemClick += ListData_ItemClick;
                    listData.ItemLongClick += ListData_ItemLongClick;
                    listData.Adapter = dataAdapter;
                }
                else
                {
                    base.RequestedOrientation = ScreenOrientation.Portrait;
                    base.SetContentView(Resource.Layout.IssuedGoodsIdentEntry);
                }
                AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
                var _customToolbar = new CustomToolbar(this, toolbar, Resource.Id.navIcon);
                _customToolbar.SetNavigationIcon(App.Settings.RootURL + "/Services/Logo");
                SetSupportActionBar(_customToolbar._toolbar);
                SupportActionBar.SetDisplayShowTitleEnabled(false);
                tbOrder = FindViewById<EditText>(Resource.Id.tbOrder);
                tbIdent = FindViewById<CustomAutoCompleteTextView>(Resource.Id.tbIdent);
                tbNaziv = FindViewById<EditText>(Resource.Id.tbNaziv);
                tbConsignee = FindViewById<EditText>(Resource.Id.tbConsignee);
                tbDeliveryDeadline = FindViewById<EditText>(Resource.Id.tbDeliveryDeadline);
                tbQty = FindViewById<EditText>(Resource.Id.tbQty);
                btNext = FindViewById<Button>(Resource.Id.btNext);
                btConfirm = FindViewById<Button>(Resource.Id.btConfirm);
                button4 = FindViewById<Button>(Resource.Id.button4);
                button5 = FindViewById<Button>(Resource.Id.button5);
                lbOrderInfo = FindViewById<TextView>(Resource.Id.lbOrderInfo);
                tbQty = FindViewById<EditText>(Resource.Id.tbQty);
                color();
                btNext.Enabled = false;
                btConfirm.Enabled = false;
                barcode2D = new Barcode2D(this, this);
                btNext.Click += BtNext_Click;
                btConfirm.Click += BtConfirm_Click;
                button4.Click += Button4_Click;
                button5.Click += Button5_Click;
                tbIdent.RequestFocus();
         
    
                var _broadcastReceiver = new NetworkStatusBroadcastReceiver();
                _broadcastReceiver.ConnectionStatusChanged += OnNetworkStatusChanged;
                Application.Context.RegisterReceiver(_broadcastReceiver,
                new IntentFilter(ConnectivityManager.ConnectivityAction), ReceiverFlags.NotExported);

                tbIdent.RequestFocus();
                tbIdent.TextChanged += TbIdent_TextChanged;

                // These are read only. 6.6.2024 JJ
                tbOrder.Enabled = false;
                tbConsignee.Enabled = false;
                tbDeliveryDeadline.Enabled = false;
                tbQty.Enabled = false;
                tbNaziv.Enabled = false;

                LoadIdentDataAsync();

            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private async void LoadIdentDataAsync()
        {
            try
            {
                await Task.Run(() => LoadData());

                // After loading the data, update the UI on the main thread
                RunOnUiThread(() =>
                {
                    if (savedIdents != null)
                    {
                        tbIdentAdapter = new CustomAutoCompleteAdapter<string>(this, Android.Resource.Layout.SimpleDropDownItem1Line, savedIdents);
                        tbIdent.Adapter = tbIdentAdapter;
                        tbIdentAdapter.SingleItemEvent += TbIdentAdapter_SingleItemEvent;
                    }
                });
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private void LoadData()
        {
            try
            {
                ISharedPreferences sharedPreferences = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
                string savedIdentsJson = sharedPreferences.GetString("idents", "");

                if (!string.IsNullOrEmpty(savedIdentsJson))
                {
                    LoaderManifest.LoaderManifestLoopResources(this);
                    savedIdents = JsonConvert.DeserializeObject<List<string>>(savedIdentsJson);
                    LoaderManifest.LoaderManifestLoopStop(this);
                }
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }


        private async void TbIdentAdapter_SingleItemEvent(string barcode)
        {
            try
            {
                var item = tbIdentAdapter.GetItem(0);
                tbIdent.SetText(item.ToString(), false);
                await ProcessIdent(false);
                tbIdent.SelectAll();
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }
        private async void TbIdent_TextChanged(object? sender, Android.Text.TextChangedEventArgs e)
        {
            try
            {
                if (e.Text.ToString() == string.Empty)
                {
                    await ProcessIdent(true);
                }
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }
        private void ListData_ItemLongClick(object? sender, AdapterView.ItemLongClickEventArgs e)
        {
            try
            {
                selected = e.Position;
                Select(selected);
                selectedItem = selected;

                btConfirm.PerformClick();
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private void ListData_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            try
            {
                selected = e.Position;
                Select(selected);
                selectedItem = selected;
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }
        private void Select(int postionOfTheItemInTheList)
        {
            try
            {
                displayedOrder = postionOfTheItemInTheList;
                FillDisplayedOrderInfo();
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

        private void UpdateSuggestions(string userInput)
        {
            try
            {
                if (userInput.Length < 3)
                {
                    tbIdentAdapter.Clear();
                    return;
                }
                else
                {
                    suggestions.Clear();
                    // Provide custom suggestions based on user input
                    suggestions = GetCustomSuggestions(userInput);
                    // Clear the existing suggestions and add the new ones
   
                    tbIdentAdapter.Clear();
                    tbIdentAdapter.AddAll(suggestions);
                    tbIdentAdapter.NotifyDataSetChanged();
                }
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
                return new List<string> ();
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

        private void Button5_Click(object sender, EventArgs e)
        {
            try
            {
                StartActivity(typeof(MainMenu));
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private void Button4_Click(object sender, EventArgs e)
        {
            try
            {
                StartActivity(typeof(IssuedGoodsEnteredPositionsView));
                this.Finish();
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
                if (SaveMoveHead())
                {
                    Base.Store.isUpdate = false;
                    StartActivity(typeof(IssuedGoodsSerialOrSSCCEntry));
                    this.Finish();
                }
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private void BtNext_Click(object sender, EventArgs e)
        {
            try
            {
                try
                {
                    // F2
                    displayedOrder++;

                    if (displayedOrder >= orders.Count)
                    {
                        displayedOrder = 0;
                    }

                    FillDisplayedOrderInfo();

                    if (App.Settings.tablet)
                    {
                        UniversalAdapterHelper.SelectPositionProgramaticaly(listData, displayedOrder);
                    }

                }
                catch { return; }
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }


        // function keys

        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            try
            {
                switch (keyCode)
                {
                    // in smartphone
                    case Keycode.F2:
                        if (btNext.Enabled == true)
                        {
                            BtNext_Click(this, null);
                        }
                        break;
                    // return true;


                    case Keycode.F3:
                        if (btConfirm.Enabled == true)
                        {
                            BtConfirm_Click(this, null);
                        }
                        break;


                    case Keycode.F4:
                        if (button4.Enabled == true)
                        {
                            Button4_Click(this, null);
                        }
                        break;

                    case Keycode.F8:
                        if (button5.Enabled == true)
                        {
                            Button5_Click(this, null);
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
    }
}