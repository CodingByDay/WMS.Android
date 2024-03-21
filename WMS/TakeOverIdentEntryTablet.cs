using Stream = Android.Media.Stream;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
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

using AndroidX.AppCompat.App;using AlertDialog = Android.App.AlertDialog;namespace WMS
{
    [Activity(Label = "TakeOverIdentEntryTablet", ScreenOrientation = Android.Content.PM.ScreenOrientation.Landscape)]
    public class TakeOverIdentEntryTablet : AppCompatActivity, IBarcodeResult, IDialogInterfaceOnClickListener

    {
        private int displayedPosition;
        private bool preventingDups = false;
        private NameValueObject moveHead = (NameValueObject)InUseObjects.Get("MoveHead"); 
        private NameValueObject openIdent = null;
        private NameValueObjectList openOrders = null;
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
        private List<string> identData = new List<string>();
        private ListView listData;
        private List<TakeOverIdentList> data = new List<TakeOverIdentList>();
        SoundPool soundPool;
        int soundPoolId;
        public NameValueObject order;
        public string openQty;
        private int selectedItem= -1;
        public int selected = -1;

        private List<string> returnList;
        private List<string> savedIdents;
        private CustomAutoCompleteAdapter<string> tbIdentAdapter;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetTheme(Resource.Style.AppTheme_NoActionBar);
            SetContentView(Resource.Layout.TakeOverIdentEntryTablet);
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
            btConfirm = FindViewById<Button>(Resource.Id.btConfirm);
            button4 = FindViewById<Button>(Resource.Id.button4);
            button5 = FindViewById<Button>(Resource.Id.button5);
            listData = FindViewById<ListView>(Resource.Id.listData);
            TakeOverIdentAdapter adapter = new TakeOverIdentAdapter(this, data);
            listData.Adapter = adapter;
            color();
            soundPool = new SoundPool(10, Stream.Music, 0);
            soundPoolId = soundPool.Load(this, Resource.Raw.beep, 1);
            Barcode2D barcode2D = new Barcode2D();
            barcode2D.open(this, this);
            tbIdent.FocusChange += TbIdent_FocusChange;
            if (moveHead == null) { throw new ApplicationException("moveHead not known at this point!?"); }
            displayedOrder = 0;
            FillDisplayedOrderInfo();
            btNext = FindViewById<Button>(Resource.Id.btNext);
            btNext.Click += BtNext_Click;
            btConfirm.Click += BtConfirm_Click;
            button4.Click += Button4_Click;
            button5.Click += Button5_Click;
            listData.ItemClick += ListData_ItemClick;
            ISharedPreferences sharedPreferences = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
            ISharedPreferencesEditor editor = sharedPreferences.Edit();
            string savedIdentsJson = sharedPreferences.GetString("idents", "");
            if (!string.IsNullOrEmpty(savedIdentsJson))
            {
                // Deserialize the JSON string back to a List<string>
                savedIdents = JsonConvert.DeserializeObject<List<string>>(savedIdentsJson);
                // Now you have your list of idents in the savedIdents variable
            }
            tbIdent.LongClick += ClearTheFields;
            tbIdentAdapter = new CustomAutoCompleteAdapter<string>(this, Android.Resource.Layout.SimpleDropDownItem1Line, new List<string>());
            tbIdent.Adapter = tbIdentAdapter;
            tbIdent.TextChanged += (sender, e) =>
            {
                string userInput = e.Text.ToString();
                UpdateSuggestions(userInput);
            };

            tbIdent.RequestFocus();
            tbIdent.LongClick += ClearTheFields;
            var _broadcastReceiver = new NetworkStatusBroadcastReceiver();
            _broadcastReceiver.ConnectionStatusChanged += OnNetworkStatusChanged;
            Application.Context.RegisterReceiver(_broadcastReceiver,
            new IntentFilter(ConnectivityManager.ConnectivityAction));
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



        public async void OnSearchTextChanged(string p0)
        {
            var test = p0;
           

        }
        private void ClearTheFields(object sender, View.LongClickEventArgs e)
        {
            tbIdent.Text = "";
            tbNaziv.Text = "";
           
        }

 

       

      
        private void BtNext_Click1(object sender, EventArgs e)
        {

            displayedPosition++;
            if (displayedOrder >= openOrders.Items.Count) { displayedOrder = 0; }

            FillDisplayedOrderInfoSelect();


            // Change the highlight position.
            listData.RequestFocusFromTouch();
            listData.SetItemChecked(displayedPosition, true);
            listData.SetSelection(displayedPosition);
        }

        private void ListData_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            selected = e.Position;
            Select(selected);
            selectedItem = selected;
        }

     


        private void Select(int postionOfTheItemInTheList)
        {

            displayedOrder = postionOfTheItemInTheList;
        
            FillDisplayedOrderInfoSelect();
        }

        private void TbIdent_FocusChange(object sender, View.FocusChangeEventArgs e)
        {
             // Preventing duplicate list filling with idents.
            ProcessIdent();
            OnSearchTextChanged(tbIdent.Text);

        }
        private string LoadStockFromStock(string warehouse, string ident)
        {
            try
            {
                string error;
                var stock = Services.GetObjectList("str", out error, warehouse + "|" + ident);
                if (stock == null)
                {
                    string WebError = string.Format($"{Resources.GetString(Resource.String.s216)}" + error);
                    Toast.MakeText(this, WebError, ToastLength.Long).Show(); tbIdent.Text = "";
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
       

        private void Button5_Click(object sender, EventArgs e)
        {
            this.Finish();
        }

        private void Button4_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(TakeOverEnteredPositionsViewTablet));
            HelpfulMethods.clearTheStack(this);
        }

        private void BtConfirm_Click(object sender, EventArgs e)
        {
            if (SaveMoveHead())
            {
                StartActivity(typeof(TakeOverSerialOrSSCCEntryTablet));
                HelpfulMethods.clearTheStack(this);
            }
        }

        private void BtNext_Click(object sender, EventArgs e)
        {
            selected++;
            if (openOrders != null) {
                if (selected <= (openOrders.Items.Count - 1))
                {
                    listData.RequestFocusFromTouch();
                    listData.SetSelection(selected);
                    listData.SetItemChecked(selected, true);
                }
                else
                {
                    selected = 0;
                    listData.RequestFocusFromTouch();
                    listData.SetSelection(selected);
                    listData.SetItemChecked(selected, true);
                }
                displayedOrder++;
                if (displayedOrder >= openOrders.Items.Count) { displayedOrder = 0; }
                FillDisplayedOrderInfo(); 
            }
        }


        private bool SaveMoveHead()
        {
            if (!moveHead.GetBool("Saved"))
            {

                try
                {


                    NameValueObject order;
                    if ((openOrders == null) || (openOrders.Items.Count == 0))
                    {
                        order = new NameValueObject("OpenOrder");
                        InUseObjects.Set("OpenOrder", order);
                    }
                    else
                    {
                        order = openOrders.Items[displayedOrder];
                        InUseObjects.Set("OpenOrder", order);
                    }

                    moveHead.SetInt("Clerk", Services.UserID());
                    moveHead.SetString("Type", "I");
                    moveHead.SetString("LinkKey", order.GetString("Key"));
                    moveHead.SetString("LinkNo", order.GetString("No"));
                    moveHead.SetString("Document1", order.GetString("Document1"));
                    moveHead.SetDateTime("Document1Date", order.GetDateTime("Document1Date"));
                    moveHead.SetString("Note", order.GetString("Note"));
                    if (moveHead.GetBool("ByOrder"))
                    {
                        moveHead.SetString("Receiver", order.GetString("Receiver"));
                    }

                    string error;
                    var savedMoveHead = Services.SetObject("mh", moveHead, out error);
                    if (savedMoveHead == null)
                    {
                        Toast.MakeText(this, "Napaka pri dostopu do web aplikacije" + error, ToastLength.Long).Show();
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

                    Crashes.TrackError(err);
                    return false;

                }
            }
            else
            {
                return true;
            }
        }
        private void ProcessIdent()
        {
            var ident = tbIdent.Text.Trim();
            if (string.IsNullOrEmpty(ident)) { return; }


            try
            {


                string error;
                openIdent = Services.GetObject("id", ident, out error);
                if (openIdent == null)
                {
                    Toast.MakeText(this, "Napaka pri preverjanju identa" + error, ToastLength.Long).Show();

                    tbIdent.Text = "";
                    tbNaziv.Text = "";
                    tbQty.Text = "";
                    openOrders = null;
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
                            StartActivity(typeof(TakeOverSerialOrSSCCEntryTablet));
                            HelpfulMethods.clearTheStack(this);

                        }
                        return;
                    }
                    else
                    {
                        tbNaziv.Text = openIdent.GetString("Name");


                        openOrders = Services.GetObjectList("oo", out error, ident + "|" + moveHead.GetString("DocumentType") + "|" + moveHead.GetInt("HeadID"));
                        if (openOrders == null)
                        {
                            // Napaka pri pridobivanju odprtih naročil: " + error
                            Toast.MakeText(this, "Napaka pri pridobivanju odprtih naročil: " + error, ToastLength.Long).Show();


                            tbIdent.Text = "";

                            tbNaziv.Text = "";
                        }
                        else
                        {
                            InUseObjects.Set("openOrders", openOrders);
                            displayedOrder = 0;
                        }
                    }
                }
                FillDisplayedOrderInfo();
                fillList(tbIdent.Text);
            }
            catch (Exception err)
            {

                Crashes.TrackError(err);
                return;

            }
        }


        private void fillList(string ident)
        {
            if (preventingDups == false)
            {
                string error;
                var stock = Services.GetObjectList("str", out error, moveHead.GetString("Wharehouse") + "||" + tbIdent.Text);
                //  var openOrder = Services.GetObjectList("oo", out error, tbIdent.Text + "|" + moveHead.GetString("DocumentType") + "|" + moveHead.GetInt("HeadID"));
                if (openOrders != null)
                {
                    openOrders.Items.ForEach(x =>
                    {
                        data.Add(new TakeOverIdentList
                        {
                            Ident = tbIdent.Text,
                            Name = x.GetString("Name").Trim().Substring(0, 10),
                            Open = x.GetDouble("OpenQty").ToString(CommonData.GetQtyPicture()),
                            Ordered = x.GetDouble("FullQty").ToString(CommonData.GetQtyPicture()),
                            Received = (x.GetDouble("FullQty") - x.GetDouble("OpenQty")).ToString(CommonData.GetQtyPicture())
                        }); ;
                    });
                    preventingDups = true;
                }

                else
                {
                    Toast.MakeText(this, "Ni padatkov." + error, ToastLength.Long).Show();
                }
            } 
        }
        private void FillDisplayedOrderInfo()
        {
            if ((openIdent != null) && (openOrders != null) && (openOrders.Items.Count > 0))
            {
                lbOrderInfo.Text = "Naročilo (" + (displayedOrder + 1).ToString() + "/" + openOrders.Items.Count.ToString() + ")";
                order = openOrders.Items[displayedOrder];
                InUseObjects.Set("OpenOrder", order);
                tbOrder.Text = order.GetString("Key");
                tbConsignee.Text = order.GetString("Consignee");
                tbQty.Text = order.GetDouble("OpenQty").ToString(CommonData.GetQtyPicture());
               
                var deadLine = order.GetDateTime("DeliveryDeadline");
                tbDeliveryDeadline.Text = deadLine == null ? "" : ((DateTime)deadLine).ToString("dd.MM.yyyy");
                string error;
                var stock = Services.GetObjectList("str", out error, moveHead.GetString("Wharehouse") + "||" + tbIdent.Text);
                btConfirm.Enabled = true;

                tbOrder.Enabled = false;
                tbConsignee.Enabled = false;
                tbQty.Enabled = false;
                tbDeliveryDeadline.Enabled = false;
            }
            else
            {
                InUseObjects.Invalidate("OpenOrder");
                lbOrderInfo.Text = "Naročilo (ni postavk)";
                tbOrder.Text = "";
                tbConsignee.Text = "";
                tbQty.Text = "";
                tbDeliveryDeadline.Text = "";
                btConfirm.Enabled = false;
            }
        }

        private void FillDisplayedOrderInfoSelect()
        {
            if ((openIdent != null) && (openOrders != null) && (openOrders.Items.Count > 0))
            {
                lbOrderInfo.Text = "Naročilo (" + (displayedOrder + 1).ToString() + "/" + openOrders.Items.Count.ToString() + ")";
                order = openOrders.Items[displayedOrder];
                InUseObjects.Set("OpenOrder", order);
                tbOrder.Text = order.GetString("Key");
                tbConsignee.Text = order.GetString("Consignee");
                tbQty.Text = order.GetDouble("OpenQty").ToString(CommonData.GetQtyPicture());
                var deadLine = order.GetDateTime("DeliveryDeadline");
                tbDeliveryDeadline.Text = deadLine == null ? "" : ((DateTime)deadLine).ToString("dd.MM.yyyy");
                string error;
                var stock = Services.GetObjectList("str", out error, moveHead.GetString("Wharehouse") + "||" + tbIdent.Text);
                btConfirm.Enabled = true;
                tbOrder.Enabled = false;
                tbConsignee.Enabled = false;
                tbQty.Enabled = false;
                tbDeliveryDeadline.Enabled = false;
            }
            else
            {
                InUseObjects.Invalidate("OpenOrder");
                lbOrderInfo.Text = "Naročilo (ni postavk)";
                tbOrder.Text = "";
                tbConsignee.Text = "";
                tbQty.Text = "";
                tbDeliveryDeadline.Text = "";
                btConfirm.Enabled = false;
            }
        }


        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            if (requestCode == 2)
            {
                if (resultCode == Result.Ok)
                {
                    Toast.MakeText(this, data.Data.ToString(), ToastLength.Long).Show();
                    barcode = data.Data.ToString();
                    tbIdent.Text = barcode; // change this later...
                }
                else
                {
                    Toast.MakeText(this, "Napačno branje", ToastLength.Long).Show();
                }
            }
        }

        public void GetBarcode(string barcode)
        {
            if (tbIdent.HasFocus)
            {
                Sound();
                tbIdent.Text = barcode;
                ProcessIdent();
            }
        }

        private void Sound()
        {
            soundPool.Play(soundPoolId, 1, 1, 0, 0, 1);
        }


        // color
        private void color()
        {
            tbIdent.SetBackgroundColor(Android.Graphics.Color.Aqua);
        }

        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
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

        public void OnClick(IDialogInterface dialog, int which)
        {
            throw new NotImplementedException();
        }
    }
}