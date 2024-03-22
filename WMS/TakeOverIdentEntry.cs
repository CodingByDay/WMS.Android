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

using AndroidX.AppCompat.App;using AlertDialog = Android.App.AlertDialog;namespace WMS
{
    [Activity(Label = "TakeOverIdentEntry", ScreenOrientation = ScreenOrientation.Portrait)]
    public class TakeOverIdentEntry : AppCompatActivity, IBarcodeResult

    {
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
        SoundPool soundPool;
        int soundPoolId;
        private List<string> returnList = new List<string>();
        private List<string> identData = new List<string>();
        private Intent intentClass;
        private List<string> savedIdents;
        private CustomAutoCompleteAdapter<string> tbIdentAdapter;

        protected async override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetTheme(Resource.Style.AppTheme_NoActionBar);
            SetContentView(Resource.Layout.TakeOverIdentEntry);
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
            soundPool = new SoundPool(10, Stream.Music, 0);
            soundPoolId = soundPool.Load(this, Resource.Raw.beep, 1);
            Barcode2D barcode2D = new Barcode2D();
            barcode2D.open(this, this);
            tbIdent.FocusChange += TbIdent_FocusChange;
            if (moveHead == null) { throw new ApplicationException("moveHead not known at this point!?"); }
            displayedOrder = 0;
            FillDisplayedOrderInfo();
            btNext.Click += BtNext_Click;
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


            tbIdent.LongClick += ClearTheFields;
            tbIdentAdapter = new CustomAutoCompleteAdapter<string>(this, Android.Resource.Layout.SimpleDropDownItem1Line, new List<string>());
            tbIdent.Adapter = tbIdentAdapter;
            tbIdent.TextChanged += (sender, e) =>
            {
                string userInput = e.Text.ToString();
                UpdateSuggestions(userInput);
            };
            

            tbIdent.LongClick += ClearTheFields;
            var _broadcastReceiver = new NetworkStatusBroadcastReceiver();
            _broadcastReceiver.ConnectionStatusChanged += OnNetworkStatusChanged;
            Application.Context.RegisterReceiver(_broadcastReceiver,
            new IntentFilter(ConnectivityManager.ConnectivityAction));



            UpdateSuggestions(string.Empty);
            InputMethodManager imm = (InputMethodManager)GetSystemService(Context.InputMethodService);
            imm.ShowSoftInput(tbIdent, ShowFlags.Forced);
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
            HelpfulMethods.clearTheStack(this);
            Finish();
        }

        private void BtConfirm_Click(object sender, EventArgs e)
        {
            if (SaveMoveHead())
            {
                StartActivity(typeof(TakeOverSerialOrSSCCEntry));
                HelpfulMethods.clearTheStack(this);
                Finish();
            }
        }

        private void BtNext_Click(object sender, EventArgs e)
        {
            displayedOrder++;
            if (displayedOrder >= openOrders.Items.Count) { displayedOrder = 0; }
            FillDisplayedOrderInfo();
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
                    Crashes.TrackError(error);
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
                    Crashes.TrackError(error);
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
                            StartActivity(typeof(TakeOverSerialOrSSCCEntry));   
                        }
                        return;
                    }
                    else
                    {
                        tbNaziv.Text = openIdent.GetString("Name");               
                        openOrders = Services.GetObjectList("oo", out error, ident + "|" + moveHead.GetString("DocumentType") + "|" + moveHead.GetInt("HeadID"));
                        if (openOrders == null)
                        {
                            DialogHelper.ShowDialogError(this, this, $"{Resources.GetString(Resource.String.s216)}");
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
            } catch (Exception ex)
            {
                Crashes.TrackError(ex);
            }

        }
        private void FillDisplayedOrderInfo()
        {
            if ((openIdent != null) && (openOrders != null) && (openOrders.Items.Count > 0))
            {

                lbOrderInfo.Text = $"{Resources.GetString(Resource.String.s36)} (" + (displayedOrder + 1).ToString() + "/" + openOrders.Items.Count.ToString() + ")";
                var order = openOrders.Items[displayedOrder];
                InUseObjects.Set("OpenOrder", order);
                tbOrder.Text = order.GetString("Key");
                tbConsignee.Text = order.GetString("Consignee");
                tbQty.Text = order.GetDouble("OpenQty").ToString(CommonData.GetQtyPicture());

                var deadLine = order.GetDateTime("DeliveryDeadline");
                tbDeliveryDeadline.Text = deadLine == null ? "" : ((DateTime)deadLine).ToString("dd.MM.yyyy");

                btNext.Enabled = true;
                btConfirm.Enabled = true;
                tbOrder.Enabled = false;
                tbConsignee.Enabled = false;
                tbQty.Enabled = false;
                tbDeliveryDeadline.Enabled = false;
            }
            else
            {
                InUseObjects.Invalidate("OpenOrder");
                lbOrderInfo.Text = $"{Resources.GetString(Resource.String.s62)}";
                tbOrder.Text = "";
                tbConsignee.Text = "";
                tbQty.Text = "";
                tbDeliveryDeadline.Text = "";
                btNext.Enabled = false;
                btConfirm.Enabled = false;
            }
        }

 

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            if (requestCode == 2) {
                if (resultCode == Result.Ok)
                {
                    Toast.MakeText(this, data.Data.ToString(), ToastLength.Long).Show();
                    barcode = data.Data.ToString();
                    tbIdent.Text = barcode; // Change this later...
                } else
                {
                    Toast.MakeText(this, "Napačno branje", ToastLength.Long).Show();
                }
            }
        }

        public void GetBarcode(string barcode)
        {
            if (barcode != "Scan fail" && barcode != "")
            {
                if (HelperMethods.is2D(barcode))
                {
                    Parser2DCode parser2DCode = new Parser2DCode(barcode.Trim());
                    jumpAhead(parser2DCode);
                }
                else if (!CheckIdent(barcode) && barcode.Length > 17 && barcode.Contains("400"))
                {
                    var ident = barcode.Substring(0, barcode.Length - 16);
                    Sound();
                    tbIdent.Text = ident;
                    ProcessIdent();
                }
                else
                {
                    if (tbIdent.HasFocus)
                    {
                        Sound();
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

        private void jumpAhead(Parser2DCode parser2DCode)
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
                string query = $"SELECT * FROM uWMSOrderItemByKeyIn WHERE acKey = @acKey AND acIdent = @acIdent";
                var resultQuery = Services.GetObjectListBySql(query, parameters);
                if (resultQuery.Success && resultQuery.Rows.Count > 0)
                {
                    var row = resultQuery.Rows[0];
                    tbIdent.Text = ident;
                    ProcessIdent();
                    tbOrder.Text = row.StringValue("acKey");
                    tbConsignee.Text = row.StringValue("acSubject");
                    tbQty.Text = row.DoubleValue("anQty").ToString();
                    var deadLine = row.DateTimeValue("adDeliveryDeadline");
                    tbDeliveryDeadline.Text = deadLine == null ? "" : ((DateTime)deadLine).ToString("dd.MM.yyyy");
                    intentClass = new Intent(Application.Context, typeof(TakeOverSerialOrSSCCEntry));
                    intentClass.PutExtra("qty", qty);
                    intentClass.PutExtra("serial", parser2DCode.charge);
                    StartActivity(intentClass);
                    if (SaveMoveHead2D(row))
                    {
                        StartActivity(intentClass);
                        HelpfulMethods.clearTheStack(this);
                        Finish();
                    }
                }            
            }
        }

        private void Sound()
        {
            soundPool.Play(soundPoolId, 1, 1, 0, 0, 1);
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