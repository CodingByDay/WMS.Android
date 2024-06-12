using Android.Content;
using Android.Content.PM;
using Android.Net;
using TrendNET.WMS.Core.Data;
using TrendNET.WMS.Device.App;
using TrendNET.WMS.Device.Services;
using WMS.App;
using WMS.Printing;


namespace WMS
{
    [Activity(Label = "TakeOver2Orders", ScreenOrientation = ScreenOrientation.Portrait)]
    public class TakeOver2Orders : CustomBaseActivity
    {
        private EditText tbIdent;
        private EditText tbNaziv;
        private EditText tbKolicinaPrevzema;
        private EditText tbNarocilo;
        private EditText tbKupec;
        private EditText tbDatumDostave;
        private EditText tbKolicinaOdprta;
        private EditText tbKolicinaPrevzetaDoSedaj;
        private EditText tbKolicinaPrevzetaNova;
        private Button button1;
        private Button button2;
        private Button button3;
        private Button button4;
        private Button button5;
        private TextView lblOrder;
        private NameValueObject moveItem = (NameValueObject)InUseObjects.Get("MoveItem");
        private NameValueObjectList moveItemDivision;
        private int displayOrder = 0;
        private Button logout;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetTheme(Resource.Style.AppTheme_NoActionBar);
            if (App.Settings.tablet)
            {
                base.RequestedOrientation = ScreenOrientation.Landscape;
                base.SetContentView(Resource.Layout.TakeOver2OrdersTablet);
            }
            else
            {
                base.RequestedOrientation = ScreenOrientation.Portrait;
                base.SetContentView(Resource.Layout.TakeOver2Orders);
            }
            AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
            var _customToolbar = new CustomToolbar(this, toolbar, Resource.Id.navIcon);
            _customToolbar.SetNavigationIcon(App.Settings.RootURL + "/Services/Logo");
            SetSupportActionBar(_customToolbar._toolbar);
            SupportActionBar.SetDisplayShowTitleEnabled(false);
            tbIdent = FindViewById<EditText>(Resource.Id.tbIdent);
            tbNaziv = FindViewById<EditText>(Resource.Id.tbNaziv);
            tbKolicinaPrevzema = FindViewById<EditText>(Resource.Id.tbKolicinaPrevzema);
            tbNarocilo = FindViewById<EditText>(Resource.Id.tbNarocilo);
            tbKupec = FindViewById<EditText>(Resource.Id.tbKupec);
            lblOrder = FindViewById<TextView>(Resource.Id.lblOrder);
            tbDatumDostave = FindViewById<EditText>(Resource.Id.tbDatumDostave);
            tbKolicinaOdprta = FindViewById<EditText>(Resource.Id.tbKolicinaOdprta);
            tbKolicinaPrevzetaDoSedaj = FindViewById<EditText>(Resource.Id.tbKolicinaPrevzetaDoSedaj);
            tbKolicinaPrevzetaNova = FindViewById<EditText>(Resource.Id.tbKolicinaPrevzetaNova);
            logout = FindViewById<Button>(Resource.Id.button6);
            button1 = FindViewById<Button>(Resource.Id.button1);
            button2 = FindViewById<Button>(Resource.Id.button2);
            button3 = FindViewById<Button>(Resource.Id.button3);
            button4 = FindViewById<Button>(Resource.Id.button4);
            button5 = FindViewById<Button>(Resource.Id.button5);
            button1.Click += Button1_Click;
            button2.Click += Button2_Click;
            button3.Click += Button3_Click;
            button4.Click += Button4_Click;
            button5.Click += Button5_Click;
            logout.Click += Logout_Click;

            if (moveItem == null)
            {
                Toast.MakeText(this, "moveItem not known at this point!?", ToastLength.Long).Show();

            }

            var ident = CommonData.LoadIdent(moveItem.GetString("Ident"));
            if (ident == null)
            {
                Toast.MakeText(this, "Invalid ident at this point: " + moveItem.GetString("Ident"), ToastLength.Long).Show();

                throw new ApplicationException("Invalid ident at this point: ");
            }
            tbIdent.Text = ident.GetString("Code");
            tbNaziv.Text = ident.GetString("Name");

            tbKolicinaPrevzema.Text = moveItem.GetDouble("Qty").ToString("###,###,##0.00");

            LoadState();

            displayOrder = 0;
            UpdateForm();


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

        private void Logout_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(MainMenu));
            Finish();
        }

        private void Button5_Click(object sender, EventArgs e)
        {
            if (SaveState())
            {
                InUseObjects.Set("MoveItem", null);
                StartActivity(typeof(TakeOver2Main));
                Finish();

            }
        }

        private void Button4_Click(object sender, EventArgs e)
        {
            var kolPrevStr = tbKolicinaPrevzetaNova.Text.Trim();
            if (string.IsNullOrEmpty(kolPrevStr))
            {
                Toast.MakeText(this, $"{Resources.GetString(Resource.String.s270)}", ToastLength.Long).Show();
                return;
            }
            var kolPrev = Convert.ToDouble(kolPrevStr);
            if (kolPrev <= 0.0)
            {
                Toast.MakeText(this, $"{Resources.GetString(Resource.String.s220)}", ToastLength.Long).Show();

                return;
            }

            if (SaveState())
            {
                // wf
                try
                {
                    Toast.MakeText(this, $"{Resources.GetString(Resource.String.s326)}", ToastLength.Long).Show();
                    var nvo = new NameValueObject("ReceiverSticker");
                    PrintingCommon.SetNVOCommonData(ref nvo);
                    nvo.SetString("Ident", tbIdent.Text.Trim());
                    nvo.SetString("Order", tbNarocilo.Text.Trim());
                    nvo.SetDouble("Qty", kolPrev);
                    PrintingCommon.SendToServer(nvo);
                }
                catch (Exception err)
                {

                    SentrySdk.CaptureException(err);
                    return;

                }
            }

        }

        private void Button3_Click(object sender, EventArgs e)
        {
            if (SaveState())
            {

                try
                {
                    Toast.MakeText(this, $"{Resources.GetString(Resource.String.s326)}", ToastLength.Long).Show();
                    var nvo = new NameValueObject("ReceiverSticker");
                    PrintingCommon.SetNVOCommonData(ref nvo);
                    nvo.SetString("Ident", tbIdent.Text.Trim());
                    nvo.SetString("Order", tbNarocilo.Text.Trim());
                    nvo.SetDouble("Qty", 1.0);
                    PrintingCommon.SendToServer(nvo);
                    Toast.MakeText(this, $"{Resources.GetString(Resource.String.s323)}", ToastLength.Long).Show();
                }
                catch (Exception err)
                {

                    SentrySdk.CaptureException(err);
                    return;

                }
            }
        }


        private bool SaveState()
        {
            var kolPrevStr = tbKolicinaPrevzetaNova.Text.Trim();
            if (string.IsNullOrEmpty(kolPrevStr)) { return true; }

            var kolPrev = Convert.ToDouble(kolPrevStr);
            if (kolPrev == 0.0)
            {
                Toast.MakeText(this, $"{Resources.GetString(Resource.String.s327)}", ToastLength.Long).Show();

                return false;
            }

            var kolOdpStr = tbKolicinaOdprta.Text.Trim();
            var kolOdp = string.IsNullOrEmpty(kolOdpStr) ? 0.0 : Convert.ToDouble(kolOdpStr);
            var kolPrevDSStr = tbKolicinaPrevzetaDoSedaj.Text.Trim();
            var kolPrevDS = string.IsNullOrEmpty(kolPrevDSStr) ? 0.0 : Convert.ToDouble(kolPrevDSStr);
            if ((kolPrevDS + kolPrev < 0) || (kolPrevDS + kolPrev > kolOdp))
            {
                Toast.MakeText(this, $"{Resources.GetString(Resource.String.s328)}", ToastLength.Long).Show();

                return false;
            }


            try
            {


                var nvo = new NameValueObject();
                nvo.SetInt("MoveItemID", moveItem.GetInt("ItemID"));
                nvo.SetString("Order", tbNarocilo.Text.Trim());
                nvo.SetDouble("AssignQty", kolPrev);

                string error;
                if (Services.SetObject("mid", nvo, out error) == null)
                {
                    Toast.MakeText(this, $"{Resources.GetString(Resource.String.s247)}" + error, ToastLength.Long).Show();

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

        private void Button2_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(tbKolicinaPrevzetaNova.Text.Trim()))
            {
                if (SaveState())
                {
                    LoadState();

                    displayOrder--;
                    if (displayOrder < 0) { displayOrder = moveItemDivision.Items.Count - 1; }
                    UpdateForm();
                }
            }
            else
            {
                displayOrder--;
                if (displayOrder < 0) { displayOrder = moveItemDivision.Items.Count - 1; }
                UpdateForm();
            }
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(tbKolicinaPrevzetaNova.Text.Trim()))
            {
                if (SaveState())
                {
                    LoadState();

                    displayOrder++;
                    if (displayOrder >= moveItemDivision.Items.Count) { displayOrder = 0; }
                    UpdateForm();
                }
            }
            else
            {
                displayOrder++;
                if (displayOrder >= moveItemDivision.Items.Count) { displayOrder = 0; }
                UpdateForm();
            }
        }

        private void LoadState()
        {

            try
            {

                string error = "";
                moveItemDivision = Services.GetObjectList("mid", out error, moveItem.GetInt("ItemID").ToString());
                if (moveItemDivision == null)
                {
                    // UI changes.
                    RunOnUiThread(() =>
                    {
                        Toast.MakeText(this, $"{Resources.GetString(Resource.String.s247)}" + error, ToastLength.Long).Show();
                    });
                }
            }
            catch (Exception err)
            {
                SentrySdk.CaptureException(err);
                return;
            }
        }



        private void UpdateForm()
        {
            if ((0 <= displayOrder) && (displayOrder < moveItemDivision.Items.Count))
            {
                // UI changes.
                RunOnUiThread(() =>
                {
                    var mid = moveItemDivision.Items[displayOrder];
                    lblOrder.Text = $"{Resources.GetString(Resource.String.s14)} (" + (displayOrder + 1).ToString() + "/" + moveItemDivision.Items.Count.ToString() + ")";
                    tbNarocilo.Text = mid.GetString("Order");
                    tbKupec.Text = mid.GetString("Receiver");
                    var dd = mid.GetDateTime("DeliveryDeadline");
                    tbDatumDostave.Text = dd == null ? "" : ((DateTime)dd).ToString("dd.MM.yyyy");

                    var availableQty = moveItem.GetDouble("Qty");
                    var alreadyAssigned = moveItemDivision.Items.Sum(i => i.GetDouble("AssignedQty"));
                    var maxQty = Math.Min(mid.GetDouble("OpenQty"), availableQty - alreadyAssigned);
                    tbKolicinaOdprta.Text = maxQty == 0.0 ? "" : maxQty.ToString("###,###,##0.00");

                    var assQty = mid.GetDouble("AssignedQty");
                    tbKolicinaPrevzetaDoSedaj.Text = assQty == 0.0 ? "" : assQty.ToString("###,###,##0.00");
                    tbKolicinaPrevzetaNova.Text = "";
                    tbKolicinaPrevzetaNova.Enabled = true;

                    button1.Enabled = true;
                    button2.Enabled = true;
                    button3.Enabled = true;
                    button4.Enabled = true;
                });

            }
            else
            {
                // UI changes.
                RunOnUiThread(() =>
                {
                    tbNarocilo.Text = "";
                    tbKupec.Text = "";
                    tbDatumDostave.Text = "";
                    tbKolicinaOdprta.Text = "";
                    tbKolicinaPrevzetaDoSedaj.Text = "";
                    tbKolicinaPrevzetaNova.Text = "";
                    tbKolicinaPrevzetaNova.Enabled = false;
                    button1.Enabled = false;
                    button2.Enabled = false;
                    button3.Enabled = false;
                    button4.Enabled = false;
                });

            }
        }
    }
}