using Stream = Android.Media.Stream;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Media;
using Android.Net;
using Android.OS;
using Android.Preferences;
using Android.Runtime;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using BarCode2D_Receiver;
using Java.Util.Functions;
using Microsoft.AppCenter.Crashes;
using Newtonsoft.Json;
using WMS.App;
using TrendNET.WMS.Core.Data;
using TrendNET.WMS.Device.App;
using TrendNET.WMS.Device.Services;

using AndroidX.AppCompat.App;
using AlertDialog = Android.App.AlertDialog;
using Microsoft.AppCenter.Analytics;
using System.Data.Common;
namespace WMS
{
    [Activity(Label = "TakeOverIdentEntry", ScreenOrientation = ScreenOrientation.Portrait)]
    public class TakeOverIdentEntry : CustomBaseActivity, IBarcodeResult

    {
        private NameValueObject moveHead = (NameValueObject)InUseObjects.Get("MoveHead");
        private NameValueObject openIdent = null;
        private int displayedOrder = -1;
        Button btScan;
        public string barcode;
        private CustomAutoCompleteTextView tbIdent;
        private EditText tbNaziv;
        private EditText tbOrder;
        private EditText tbConsignee;
        private EditText tbDeliveryDeadline;
        private EditText tbQty;
        private TextView lbOrderInfo;
        private Button btNext;
        private Button btConfirm;
        private Button button4;
        private Button button5;
        SoundPool soundPool;
        int soundPoolId;
        private Barcode2D barcode2D;
        private List<string> returnList = new List<string>();
        private List<string> identData = new List<string>();
        private Intent intentClass;
        private List<string> savedIdents;
        private CustomAutoCompleteAdapter<string> tbIdentAdapter;
        private List<OpenOrder> orders = new List<OpenOrder>();
        private ListView listData;
        private UniversalAdapter<TakeOverIdentList> dataAdapter;
        private List<TakeOverIdentList> data = new List<TakeOverIdentList>();

        protected async override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetTheme(Resource.Style.AppTheme_NoActionBar);
            if (settings.tablet)
            {
                RequestedOrientation = ScreenOrientation.Landscape;
                SetContentView(Resource.Layout.TakeOverIdentEntryTablet);
                listData = FindViewById<ListView>(Resource.Id.listData);
                dataAdapter = UniversalAdapterHelper.GetTakeoverIdentEntry(this, data);
                listData.ItemClick += ListData_ItemClick;
                listData.ItemLongClick += ListData_ItemLongClick;
                listData.Adapter = dataAdapter;
            }
            else
            {
                RequestedOrientation = ScreenOrientation.Portrait;
                SetContentView(Resource.Layout.TakeOverIdentEntry);
            }
            AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
            var _customToolbar = new CustomToolbar(this, toolbar, Resource.Id.navIcon);
            _customToolbar.SetNavigationIcon(settings.RootURL + "/Services/Logo");
            SetSupportActionBar(_customToolbar._toolbar);
            SupportActionBar.SetDisplayShowTitleEnabled(false);
            tbIdent = FindViewById<CustomAutoCompleteTextView>(Resource.Id.tbIdent);
            tbNaziv = FindViewById<EditText>(Resource.Id.tbNaziv);
            tbOrder = FindViewById<EditText>(Resource.Id.tbOrder);
            tbConsignee = FindViewById<EditText>(Resource.Id.tbConsignee);
            tbDeliveryDeadline = FindViewById<EditText>(Resource.Id.tbDeliveryDeadline);
            tbQty = FindViewById<EditText>(Resource.Id.tbQty);
            lbOrderInfo = FindViewById<TextView>(Resource.Id.lbOrderInfo);
            btNext = FindViewById<Button>(Resource.Id.btNext);
            btConfirm = FindViewById<Button>(Resource.Id.btConfirm);
            button4 = FindViewById<Button>(Resource.Id.button4);
            button5 = FindViewById<Button>(Resource.Id.button5);
            color();
            barcode2D = new Barcode2D(this, this);
            if (moveHead == null) { throw new ApplicationException("moveHead not known at this point!?"); }
            displayedOrder = 0;
            FillDisplayedOrderInfo();
            btNext.Click += BtNext_Click;
            btConfirm.Click += BtConfirm_Click;
            button4.Click += Button4_Click;
            button5.Click += Button5_Click;
            ISharedPreferences sharedPreferences = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
            ISharedPreferencesEditor editor = sharedPreferences.Edit();
            string savedIdentsJson = sharedPreferences.GetString("idents", "");

            if (!string.IsNullOrEmpty(savedIdentsJson))
            {
                savedIdents = JsonConvert.DeserializeObject<List<string>>(savedIdentsJson);
            }

            tbIdentAdapter = new CustomAutoCompleteAdapter<string>(this, Android.Resource.Layout.SimpleDropDownItem1Line, new List<string>());
            tbIdent.Adapter = tbIdentAdapter;
            var _broadcastReceiver = new NetworkStatusBroadcastReceiver();
            _broadcastReceiver.ConnectionStatusChanged += OnNetworkStatusChanged;
            Application.Context.RegisterReceiver(_broadcastReceiver,
            new IntentFilter(ConnectivityManager.ConnectivityAction));
            // UpdateSuggestions(string.Empty);
            InputMethodManager imm = (InputMethodManager)GetSystemService(Context.InputMethodService);
            imm.ShowSoftInput(tbIdent, ShowFlags.Forced);
            tbIdent.KeyPress += TbIdent_KeyPress;
            tbIdent.AfterTextChanged += TbIdent_AfterTextChanged;
            tbIdent.RequestFocus();
        }

        private void ListData_ItemLongClick(object? sender, AdapterView.ItemLongClickEventArgs e)
        {
            selected = e.Position;
            Select(selected);
            selectedItem = selected;

            btConfirm.PerformClick();
        }

        private void ListData_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            selected = e.Position;
            Select(selected);
            selectedItem = selected;
        }


        private void fillList(string ident)
        {
            if(orders!=null)
            {
                orders.ForEach(x =>
                {
                    data.Add(new TakeOverIdentList
                    {
                        Ident = x.Ident,
                        Subject = x.Client,
                        Order = x.Order,
                        Position = x.Position,
                        Quantity = x.Quantity

                    });
                });
                dataAdapter.NotifyDataSetChanged();
                UniversalAdapterHelper.SelectPositionProgramaticaly(listData, 0);
            }                
        }
        private void Select(int postionOfTheItemInTheList)
        {

            displayedOrder = postionOfTheItemInTheList;

            FillDisplayedOrderInfo();
        }
        private void TbIdent_KeyPress(object? sender, View.KeyEventArgs e)
        {
            if(e.KeyCode == Keycode.Enter && e.Event.Action == KeyEventActions.Down)
            {
                e.Handled = true;
                ProcessIdent();
            }
        }

        private void TbIdent_AfterTextChanged(object? sender, Android.Text.AfterTextChangedEventArgs e)
        {
            string newText = tbIdent.Text;
            UpdateSuggestions(newText);
        }



        public bool IsOnline()
        {
            var cm = (ConnectivityManager)GetSystemService(ConnectivityService);
            return cm.ActiveNetworkInfo == null ? false : cm.ActiveNetworkInfo.IsConnected;
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
        private void SpinnerIdent_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            var item = e.Position;
            var chosen = identData.ElementAt(item);
            if (chosen != "")
            {
                tbIdent.Text = chosen;
            }
            ProcessIdent();
        }

        private void ClearTheFields(object sender, View.LongClickEventArgs e)
        {
            tbIdent.Text = "";
            tbNaziv.Text = "";

        }


        private void TbIdent_FocusChange(object sender, View.FocusChangeEventArgs e)
        {
            ProcessIdent();            
        }

        private void Button5_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(MainMenu));
            HelpfulMethods.clearTheStack(this);
        }

        private void Button4_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(TakeOverEnteredPositionsView));
            Finish();
        }

        private void BtConfirm_Click(object sender, EventArgs e)
        {
            if (SaveMoveHead())
            {
                var intent = new Intent(this, typeof(TakeOverSerialOrSSCCEntry));
                StartActivity(intent);
                Finish();
            }
        }

        private void BtNext_Click(object sender, EventArgs e)
        {
            displayedOrder++;

            if (displayedOrder >= orders.Count)
            {
                displayedOrder = 0;
            }

            FillDisplayedOrderInfo();

            if (settings.tablet)
            {
                UniversalAdapterHelper.SelectPositionProgramaticaly(listData, displayedOrder);
            }

        }

    
        private bool SaveMoveHead()
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
                    moveHead.SetString("Type", "I");

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
                        DialogHelper.ShowDialogError(this, this, $"{Resources.GetString(Resource.String.s213)}");
                        return false;
                    }
                    else
                    {
                        moveHead.SetInt("HeadID", savedMoveHead.GetInt("HeadID"));
                        moveHead.SetBool("Saved", true);
                        return true;
                    }
                }
                catch(Exception error)
                {
                    SentrySdk.CaptureException(error);
                    return false;
                }
            }
            else
            {
                return true;
            }
        }


        private bool SaveMoveHead2D(Row data)
        {
            if (!moveHead.GetBool("Saved"))
            {
                try
                {
                    moveHead.SetInt("Clerk", Services.UserID());
                    moveHead.SetString("Type", "I");
                    moveHead.SetString("LinkKey", data.StringValue("acKey"));
                    moveHead.SetString("LinkNo", data.IntValue("anNo").ToString());
                    if (moveHead.GetBool("ByOrder"))
                    {
                        moveHead.SetString("Receiver", data.StringValue("acSubject"));
                    }
                    string error;
                    var savedMoveHead = Services.SetObject("mh", moveHead, out error);
                    if (savedMoveHead == null)
                    {
                        DialogHelper.ShowDialogError(this, this, $"{Resources.GetString(Resource.String.s213)}");
                        return false;
                    }
                    else
                    {
                        moveHead.SetInt("HeadID", savedMoveHead.GetInt("HeadID"));
                        moveHead.SetBool("Saved", true);
                        return true;
                    }
                }
                catch (Exception error)
                {
                    SentrySdk.CaptureException(error);
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        private void SaveIdent2DCode()
        {
            var ident = tbIdent.Text.Trim();
            string error;
            openIdent = Services.GetObject("id", ident, out error);
            if (openIdent == null)
            {
                InUseObjects.Set("OpenIdent", new NameValueObject());
            }
            else
            {
                InUseObjects.Set("OpenIdent", openIdent);
            }
        }

        private async void ProcessIdent()
        {
            var ident = tbIdent.Text.Trim();
            if (string.IsNullOrEmpty(ident)) { return; }
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
                    tbIdent.Text = ident;
                    InUseObjects.Set("OpenIdent", openIdent);
                    var isPackaging = openIdent.GetBool("IsPackaging");
                    if (!moveHead.GetBool("ByOrder") || isPackaging)
                    {
                        if (SaveMoveHead())
                        {
                            var intent = new Intent(this, typeof(TakeOverSerialOrSSCCEntry));
                            StartActivity(intent);
                            Finish();
                        }
                        return;
                    }
                    else
                    {
                        tbNaziv.Text = openIdent.GetString("Name");

                        var parameters = new List<Services.Parameter>();

                        parameters.Add(new Services.Parameter { Name = "acIdent", Type = "String", Value = ident });
                        parameters.Add(new Services.Parameter { Name = "acDocType", Type = "String", Value = moveHead.GetString("DocumentType") });
                        parameters.Add(new Services.Parameter { Name = "acWarehouse", Type = "String", Value = moveHead.GetString("Wharehouse") });

                        var subjects = await AsyncServices.AsyncServices.GetObjectListBySqlAsync($"SELECT * FROM uWMSOrderItemByWarehouseTypeIn WHERE acIdent = @acIdent AND acDocType = @acDocType AND acWarehouse = @acWarehouse;", parameters);

                        if (!subjects.Success)
                        {
                            RunOnUiThread(() =>
                            {
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
                                        Position = (int?) row.IntValue("anNo"),
                                        Quantity = row.DoubleValue("anQty"),
                                        Date = row.DateTimeValue("adDeliveryDeadline"),
                                        Ident = row.StringValue("acIdent"),
                                        Packaging = row.DoubleValue("anPackQty")
                                    });

                                }
                                displayedOrder = 0;
                            }
                        }
                    }
                }

                FillDisplayedOrderInfo();

                if (settings.tablet)
                {
                    fillList(ident);
                }

                tbIdent.SetSelection(0, tbIdent.Text.Length);
            }
            catch (Exception err)
            {
                SentrySdk.CaptureException(err);
                return;
            }
        }
        private void FillDisplayedOrderInfo()
        {
            if ((openIdent != null) && (orders != null) && (orders.Count > 0))
            {

                lbOrderInfo.Text = $"{Resources.GetString(Resource.String.s36)} (" + (displayedOrder + 1).ToString() + "/" + orders.Count.ToString() + ")";
                var order = orders.ElementAt(displayedOrder);
                Base.Store.OpenOrder = order;
                tbOrder.Text = order.Order + " / " + order.Position;
                tbConsignee.Text = order.Client;
                tbQty.Text = order.Quantity.ToString();
                var deadLine = order.Date;
                tbDeliveryDeadline.Text = deadLine == null ? "" : ((DateTime)deadLine).ToString("dd.MM.yyyy");
                btNext.Enabled = true;
                btConfirm.Enabled = true;
                btNext.Enabled = true;
                btConfirm.Enabled = true;
                tbOrder.Enabled = false;
                tbConsignee.Enabled = false;
                tbQty.Enabled = false;
                tbDeliveryDeadline.Enabled = false;
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
 

        private bool preventDuplicate = false;
        private int selected;
        private int selectedItem;

        public void GetBarcode(string barcode)
        {
            if (barcode != "Scan fail" && barcode != "")
            {
                if (HelperMethods.is2D(barcode) && tbIdent.HasFocus && preventDuplicate == false)
                {
                    Parser2DCode parser2DCode = new Parser2DCode(barcode.Trim());
                    jumpAhead(parser2DCode);
                    preventDuplicate = true;
                }
                else if (HelperMethods.is2D(barcode) && tbIdent.HasFocus && preventDuplicate == true)
                {
                    return;
                }
                else if (!CheckIdent(barcode) && barcode.Length > 17 && barcode.Contains("400") && tbIdent.HasFocus)
                {
                    var ident = barcode.Substring(0, barcode.Length - 16);
                    // 
                    tbIdent.Text = ident;
                    ProcessIdent();
                }
                else
                {
                    if (tbIdent.HasFocus)
                    {
                        // 
                        tbIdent.Text = barcode;
                        ProcessIdent();
                    }
                }        
            }
            else
            {
                tbIdent.Text = string.Empty;
            }         
        }
        private bool CheckIdent(string barcode)
        {
            if (string.IsNullOrEmpty(barcode)) { return false ; }
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

        private async void jumpAhead(Parser2DCode parser2DCode)
        {
            String order = parser2DCode.clientOrder.ToString();
            string key = order;
            int addZeros = 13 - (key.Length - 2);
            int lastDash = key.LastIndexOf("-");
            string newKey = key.Insert(lastDash, "".PadLeft(addZeros, '0'));
            newKey = newKey.Replace("-", "");
            String qty = parser2DCode.netoWeight.ToString();
            String ident = parser2DCode.ident.ToString();
            string errors;
            openIdent = Services.GetObject("id", ident, out errors);
            if (openIdent != null)
            {
                var convertedIdent = openIdent.GetString("Code");
                ident = convertedIdent;
            }
            else
            {
                return;
            }
            // Get all the order that are from that ident and have the right order number
            if (!String.IsNullOrEmpty(ident)) {
                var parameters = new List<Services.Parameter>();
                parameters.Add(new Services.Parameter { Name = "acKey", Type = "String", Value = newKey });
                parameters.Add(new Services.Parameter { Name = "acIdent", Type = "String", Value = ident });
                string query = $"SELECT * FROM uWMSOrderItemByWarehouseTypeIn WHERE acKey = @acKey AND acIdent = @acIdent";
                var resultQuery = await AsyncServices.AsyncServices.GetObjectListBySqlAsync(query, parameters);
                if (resultQuery.Success && resultQuery.Rows.Count > 0)
                {
                    var row = resultQuery.Rows[0];
                    tbIdent.Text = ident;
                    SaveIdent2DCode();
                    tbOrder.Text = row.StringValue("acKey");
                    tbConsignee.Text = row.StringValue("acSubject");
                    tbQty.Text = row.DoubleValue("anQty").ToString();
                    var deadLine = row.DateTimeValue("adDeliveryDeadline");
                    tbDeliveryDeadline.Text = deadLine == null ? "" : ((DateTime)deadLine).ToString("dd.MM.yyyy");

                    parser2DCode.__helper__convertedOrder = newKey;
                    parser2DCode.__helper__position = (int) (row.IntValue("anNo") ?? 0);
                    parser2DCode.ident = ident;
                    Base.Store.code2D = parser2DCode;

                    if (SaveMoveHead2D(row))
                    {
                        var intent = new Intent(this, typeof(TakeOverSerialOrSSCCEntry));
                        StartActivity(intent);
                        Finish();
                    }

                }            
            }
        }

        private void color()
        {
            tbIdent.SetBackgroundColor(Android.Graphics.Color.Aqua);
        }

        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            switch (keyCode)
            {
                case Keycode.F2:
                    if (btNext.Enabled == true)
                    {
                        BtNext_Click(this, null);
                    }
                    break;
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

    }
}