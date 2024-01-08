using Stream = Android.Media.Stream;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Media;
using Android.Net;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using BarCode2D_Receiver;
using Microsoft.AppCenter.Crashes;
using WMS.App;
using TrendNET.WMS.Core.Data;
using TrendNET.WMS.Device.App;
using TrendNET.WMS.Device.Services;

using AndroidX.AppCompat.App;using AlertDialog = Android.App.AlertDialog;namespace WMS
{


    [Activity(Label = "IssuedGoodsIdentEntryWithTrailTablet", ScreenOrientation = Android.Content.PM.ScreenOrientation.Landscape)]
    public class IssuedGoodsIdentEntryWithTrailTablet : AppCompatActivity, IBarcodeResult
    {

        private NameValueObject moveHead = (NameValueObject)InUseObjects.Get("MoveHead");
        private NameValueObject openOrder = (NameValueObject)InUseObjects.Get("OpenOrder");
        private NameValueObject trailFilters = (NameValueObject)InUseObjects.Get("TrailFilters");
        private EditText tbOrder;
        private EditText tbReceiver;
        private EditText tbIdentFilter;
        private EditText tbLocationFilter;
        private ListView ivTrail;
        private List<Trail> ChosenOnes = new List<Trail>();
        private Button btConfirm;
        private Button btDisplayPositions;
        private Button btLogout;
        SoundPool soundPool;
        int soundPoolId;
        private List<Trail> trails;
        public int selected;
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
                //return true;
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

        public void GetBarcode(string barcode)
        {
            // 
            if (tbIdentFilter.HasFocus)
            {
                Sound();
                tbIdentFilter.Text = barcode;
                ProcessIdent();
            }
            else if (tbLocationFilter.HasFocus)
            {
                Sound();
                tbLocationFilter.Text = barcode;
                ProcessIdent();
            }
        }



        /// <summary>
        ///  Process ident method.
        /// </summary>

        private void ProcessIdent()
        {
            FillDisplayedOrderInfo();
        }

        private void FillDisplayedOrderInfo()
        {
            var filterLoc = tbLocationFilter.Text;
            var filterIdent = tbIdentFilter.Text;



            try
            {
                tbOrder.Text = openOrder.GetString("Key");
                tbReceiver.Text = openOrder.GetString("Receiver");

                var warehouse = moveHead.GetString("Wharehouse");

                string error;
                var qtyByLoc = Services.GetObjectList("stoo", out error, warehouse + "|" + openOrder.GetString("Key") + "|" + moveHead.GetInt("HeadID"));
                if (qtyByLoc == null)
                {
                    throw new ApplicationException("Napaka pri pridobivanju podatkov za vodenje po skladišču: " + error);
                }

                trails.Clear();
                qtyByLoc.Items.ForEach(i =>
                {
                    var ident = i.GetString("Ident");
                    var location = i.GetString("Location");
                    var name = i.GetString("Name");

                    if ((string.IsNullOrEmpty(filterLoc) || (location == filterLoc)) &&
                        (string.IsNullOrEmpty(filterIdent) || (ident == filterIdent)))
                    {
                        var lvi = new Trail();
                        lvi.Ident = ident;
                        lvi.Location = location;
                        lvi.Qty = i.GetDouble("Qty").ToString("###,##0.00");
                        lvi.Name = name;
                        trails.Add(lvi);
                    }
                });
            }
            catch (Exception err)
            {

                Crashes.TrackError(err);


            }

            if ((!string.IsNullOrEmpty(filterLoc) || !string.IsNullOrEmpty(filterIdent)) && (trails.Count == 0))
            {
                tbLocationFilter.Text = "";
                tbIdentFilter.Text = "";
                FillDisplayedOrderInfo();
                return;
            }

            trailFilters = new NameValueObject("TrailFilters");
            trailFilters.SetString("Ident", filterIdent);
            trailFilters.SetString("Location", filterLoc);
            InUseObjects.Set("TrailFilters", trailFilters);

            btConfirm.Enabled = true;
        }


        /// <summary>
        /// Entrz  point for the application.
        /// </summary>
        /// <param name="savedInstanceState"></param>
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetTheme(Resource.Style.AppTheme_NoActionBar);
            SetContentView(Resource.Layout.IssuedGoodsIdentEntryWithTrailTablet);
            AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
            var _customToolbar = new CustomToolbar(this, toolbar, Resource.Id.navIcon);
            _customToolbar.SetNavigationIcon(settings.RootURL + "/Services/Logo");
            SetSupportActionBar(_customToolbar._toolbar);
            SupportActionBar.SetDisplayShowTitleEnabled(false);
            tbOrder = FindViewById<EditText>(Resource.Id.tbOrder);
            tbReceiver = FindViewById<EditText>(Resource.Id.tbReceiver);
            tbIdentFilter = FindViewById<EditText>(Resource.Id.tbIdentFilter);
            tbLocationFilter = FindViewById<EditText>(Resource.Id.tbLocationFilter);
            ivTrail = FindViewById<ListView>(Resource.Id.ivTrail);
            btConfirm = FindViewById<Button>(Resource.Id.btConfirm);
            btDisplayPositions = FindViewById<Button>(Resource.Id.btDisplayPositions);
            btLogout = FindViewById<Button>(Resource.Id.btLogout);
            soundPool = new SoundPool(10, Stream.Music, 0);
            soundPoolId = soundPool.Load(this, Resource.Raw.beep, 1);
            Barcode2D barcode2D = new Barcode2D();
            barcode2D.open(this, this);
            color();
            tbLocationFilter.FocusChange += TbLocationFilter_FocusChange;
            trails = new List<Trail>();
            // Custom adapter that I wrote.
            adapter adapter = new adapter(this, trails);
            ivTrail.Adapter = adapter;
            ivTrail.ItemClick += IvTrail_ItemClick;
            btConfirm.Click += BtConfirm_Click;
            btDisplayPositions.Click += BtDisplayPositions_Click;
            btLogout.Click += BtLogout_Click;
            if (moveHead == null) { throw new ApplicationException("moveHead not known at this point!?"); }
            if (openOrder == null) { throw new ApplicationException("openOrder not known at this point!?"); }
            if (trailFilters != null)
            {
                tbIdentFilter.Text = trailFilters.GetString("Ident");
                tbLocationFilter.Text = trailFilters.GetString("Location");
            }
            FillDisplayedOrderInfo();
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
                    Crashes.TrackError(err);
                }
            }
            else
            {
                LoaderManifest.LoaderManifestLoop(this);
            }
        }
        private void TbLocationFilter_FocusChange(object sender, View.FocusChangeEventArgs e)
        {
            ProcessIdent();
        }

        private void color()
        {
            tbIdentFilter.SetBackgroundColor(Android.Graphics.Color.Aqua);
            tbLocationFilter.SetBackgroundColor(Android.Graphics.Color.Aqua);

        }
        private void BtLogout_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(MainMenuTablet));
            HelpfulMethods.clearTheStack(this);
        }

        private void BtDisplayPositions_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(IssuedGoodsEnteredPositionsViewTablet));
            HelpfulMethods.clearTheStack(this);
        }

        private void BtConfirm_Click(object sender, EventArgs e)
        {
            if (SaveMoveHead())
            {
                if (trails.Count == 1)
                {
                    var lastItem = new NameValueObject("LastItem");
                    lastItem.SetBool("IsLastItem", true);
                    InUseObjects.Set("LastItem", lastItem);
                }

                StartActivity(typeof(IssuedGoodsSerialOrSSCCEntryTablet));
                HelpfulMethods.clearTheStack(this);

            }
        }

        private void IvTrail_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {


            selected = e.Position;
            string toast = string.Format("Izbrali ste: {0}", trails.ElementAt(selected).Location.ToString());
            Toast.MakeText(this, toast, ToastLength.Long).Show();



        }
        /* Save move head method. */
        private bool SaveMoveHead()
        {
            if (selected == -1)
            {
                string WebError = string.Format("Kritična napaka.");
                Toast.MakeText(this, WebError, ToastLength.Long).Show();
                return false;
            }

            var obj = trails.ElementAt(selected);
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
                    string WebError = string.Format("Napaka pri preverjanju identa." + error);
                    Toast.MakeText(this, WebError, ToastLength.Long).Show();
                    return false;
                }
                InUseObjects.Set("OpenIdent", openIdent);
            }
            catch (Exception err)
            {

                Crashes.TrackError(err);

            }

            if (!moveHead.GetBool("Saved"))
            {

                try
                {


                    moveHead.SetInt("Clerk", Services.UserID());
                    moveHead.SetString("Type", "P");
                    moveHead.SetString("LinkKey", openOrder.GetString("Key"));
                    moveHead.SetString("LinkNo", openOrder.GetString("No"));
                    moveHead.SetString("Document1", openOrder.GetString("Document1"));
                    moveHead.SetDateTime("Document1Date", openOrder.GetDateTime("Document1Date"));
                    moveHead.SetString("Note", openOrder.GetString("Note"));
                    if (moveHead.GetBool("ByOrder"))
                    {
                        moveHead.SetString("Receiver", openOrder.GetString("Receiver"));
                    }

                    var savedMoveHead = Services.SetObject("mh", moveHead, out error);
                    if (savedMoveHead == null)
                    {
                        string WebError = string.Format("Napaka pri dostopu do web aplikacije." + error);
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

                    Crashes.TrackError(err);
                    return false;

                }
            }
            else
            {
                return true;
            }
        }


        private void Sound()
        {
            soundPool.Play(soundPoolId, 1, 1, 0, 0, 1);
        }



    }
}