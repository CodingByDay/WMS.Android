using Stream = Android.Media.Stream;
using System.Collections.Generic;
using System.Linq;
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
using Android.Widget;
using BarCode2D_Receiver;
using Microsoft.AppCenter.Crashes;
using Newtonsoft.Json;
using WMS.App;
using TrendNET.WMS.Core.Data;
using TrendNET.WMS.Device.App;
using TrendNET.WMS.Device.Services;

using AndroidX.AppCompat.App;
using AlertDialog = Android.App.AlertDialog;
using Microsoft.AppCenter.Analytics;
namespace WMS
{
    [Activity(Label = "IssuedGoodsIdentEntry", ScreenOrientation = ScreenOrientation.Portrait)]
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
        SoundPool soundPool;
        int soundPoolId;
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

        private void Sound()
        {
            soundPool.Play(soundPoolId, 1, 1, 0, 0, 1);
        }

        public void GetBarcode(string barcode)
        {
            // pass
           if(tbIdent.HasFocus)
            {
                Sound();
                tbIdent.Text = barcode;
                ProcessIdent();

            }
        }
        public void color()
        {
            tbIdent.SetBackgroundColor(Android.Graphics.Color.Aqua);
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
                            StartActivity(typeof(IssuedGoodsSerialOrSSCCEntry));
                            Finish();                                              
                        }
                        return;
                    }
                    else
                    {
                        tbNaziv.Text = openIdent.GetString("Name");

                        var parameters = new List<Services.Parameter>();
                        string debug = $"SELECT * from uWMSOrderItemByItemTypeWarehouseOut WHERE acIdent = {ident} AND acDocType = {moveHead.GetString("DocumentType")} AND acWarehouse = {moveHead.GetString("Wharehouse")};";

                        parameters.Add(new Services.Parameter { Name = "acIdent", Type = "String", Value = ident });
                        parameters.Add(new Services.Parameter { Name = "acDocType", Type = "String", Value = moveHead.GetString("DocumentType") });
                        parameters.Add(new Services.Parameter { Name = "acWarehouse", Type = "String", Value = moveHead.GetString("Wharehouse") });

                        var subjects = await AsyncServices.AsyncServices.GetObjectListBySqlAsync($"SELECT * from uWMSOrderItemByItemTypeWarehouseOut WHERE acIdent = @acIdent AND acDocType = @acDocType AND acWarehouse = @acWarehouse ORDER BY acKey, anNo;", parameters);

                        if (!subjects.Success)
                        {
                            RunOnUiThread(() =>
                            {
                                Analytics.TrackEvent(subjects.Error);
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
                                        Ident = row.StringValue("acIdent")
                                    });
                                    
                                }

                                displayedOrder = 0;
                            }
                        }
                    }
                }

                FillDisplayedOrderInfo();

                tbIdent.SetSelection(0, tbIdent.Text.Length);

            }
            catch (Exception err)
            {

                Crashes.TrackError(err);
                return;

            }
        }

        private void FillDisplayedOrderInfo()
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

                    Crashes.TrackError(err);
                    return false;

                }
            }
            else
            {
                return true;
            }
        }

        protected async override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetTheme(Resource.Style.AppTheme_NoActionBar);
            SetContentView(Resource.Layout.IssuedGoodsIdentEntry);
            AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
            var _customToolbar = new CustomToolbar(this, toolbar, Resource.Id.navIcon);
            _customToolbar.SetNavigationIcon(settings.RootURL + "/Services/Logo");
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
            soundPool = new SoundPool(10, Stream.Music, 0);
            soundPoolId = soundPool.Load(this, Resource.Raw.beep, 1);
            Barcode2D barcode2D = new Barcode2D();
            barcode2D.open(this, this);
            btNext.Click += BtNext_Click;
            tbIdent.KeyPress += TbIdent_KeyPress; 
            btConfirm.Click += BtConfirm_Click;
            button4.Click += Button4_Click;
            button5.Click += Button5_Click;
            tbIdent.RequestFocus();
            ISharedPreferences sharedPreferences = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
            ISharedPreferencesEditor editor = sharedPreferences.Edit();
            string savedIdentsJson = sharedPreferences.GetString("idents", "");
            if (!string.IsNullOrEmpty(savedIdentsJson))
            {
                savedIdents = JsonConvert.DeserializeObject<List<string>>(savedIdentsJson);
            }       
            tbIdentAdapter = new CustomAutoCompleteAdapter<string>(this, Android.Resource.Layout.SimpleDropDownItem1Line, new List<string>());
            tbIdent.Adapter = tbIdentAdapter;
            tbIdent.TextChanged += (sender, e) =>
            {
                string userInput = e.Text.ToString();
                UpdateSuggestions(userInput);
            };
            var _broadcastReceiver = new NetworkStatusBroadcastReceiver();
            _broadcastReceiver.ConnectionStatusChanged += OnNetworkStatusChanged;
            Application.Context.RegisterReceiver(_broadcastReceiver,
            new IntentFilter(ConnectivityManager.ConnectivityAction));
        }

        private void TbIdent_KeyPress(object? sender, View.KeyEventArgs e)
        {
            if (e.KeyCode == Keycode.Enter && e.Event.Action == KeyEventActions.Down)
            {
                ProcessIdent();
            }

            e.Handled = false;
        }

 

        public bool IsOnline()
        {
            var cm = (ConnectivityManager)GetSystemService(ConnectivityService);
            return cm.ActiveNetworkInfo == null ? false : cm.ActiveNetworkInfo.IsConnected;
        }

        private void UpdateSuggestions(string userInput)
        {
            if (userInput.Length < 3)
            {
                tbIdentAdapter.Clear();
                return;
            }
            else
            {
                // Provide custom suggestions based on user input
                List<string> suggestions = GetCustomSuggestions(userInput);
                // Clear the existing suggestions and add the new ones
                tbIdentAdapter.Clear();
                tbIdentAdapter.AddAll(suggestions);
                tbIdentAdapter.NotifyDataSetChanged();
            }
        }

        private List<string> GetCustomSuggestions(string userInput)
        {
            return savedIdents
                .Where(suggestion => suggestion.ToLower().Contains(userInput.ToLower())).Take(1000)
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
                    Crashes.TrackError(err);
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
 

       

        private void Button5_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(MainMenu));
             
        }

        private void Button4_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(IssuedGoodsEnteredPositionsView));
            this.Finish();
        }

        private void BtConfirm_Click(object sender, EventArgs e)
        {
            if (SaveMoveHead())
            {
               StartActivity(typeof(IssuedGoodsSerialOrSSCCEntry));
               this.Finish();
            }
        }

        private void BtNext_Click(object sender, EventArgs e)
        {
            try
            {
                // F2
                displayedOrder++;

                if (displayedOrder >= orders.Count) { 
                    displayedOrder = 0; 
                }

                FillDisplayedOrderInfo();
            } catch { return; }
        }


        // function keys

        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
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
    }
}