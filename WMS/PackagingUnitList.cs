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
using static Android.App.ActionBar;
using WebApp = TrendNET.WMS.Device.Services.WebApp;

using AndroidX.AppCompat.App;using AlertDialog = Android.App.AlertDialog;namespace WMS
{
    [Activity(Label = "PackagingUnitList")]
    public class PackagingUnitList : AppCompatActivity

    {
        private TextView lbInfo;
        private EditText tbIdent;
        private EditText tbSSCC;
        private EditText tbSerialNo;
        private EditText tbQty;
        private EditText tbLocation;
        private EditText tbCreatedBy;
        private Button btNext;
        private Button btUpdate;
        private Button btDelete;
        private Button btCreate;
        private Button btLogout;
        private int displayedPosition = 0;
        private NameValueObjectList positions = null;
        private NameValueObject head = (NameValueObject)InUseObjects.Get("PackagingHead");
        private Dialog popupDialog;
        private Button btnYes;
        private Button btnNo;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.PackagingUnitList);
            AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
            var _customToolbar = new CustomToolbar(this, toolbar, Resource.Id.navIcon);
            _customToolbar.SetNavigationIcon(settings.RootURL + "/Services/Logo");
            lbInfo = FindViewById<TextView>(Resource.Id.lbInfo);
            tbIdent = FindViewById<EditText>(Resource.Id.tbIdent);
            tbSSCC = FindViewById<EditText>(Resource.Id.tbSSCC);
            tbSerialNo = FindViewById<EditText>(Resource.Id.tbSerialNo);
            tbQty = FindViewById<EditText>(Resource.Id.tbQty);
            tbLocation = FindViewById<EditText>(Resource.Id.tbLocation);
            tbCreatedBy = FindViewById<EditText>(Resource.Id.tbCreatedBy);
            btNext = FindViewById<Button>(Resource.Id.btNext);
            btUpdate = FindViewById<Button>(Resource.Id.btUpdate);
            btDelete = FindViewById<Button>(Resource.Id.btDelete);
            btCreate = FindViewById<Button>(Resource.Id.btCreate);
            btLogout = FindViewById<Button>(Resource.Id.btExit);
            btNext.Click += BtNext_Click;
            btUpdate.Click += BtUpdate_Click;
            btDelete.Click += BtDelete_Click;
            btCreate.Click += BtCreate_Click;
            btLogout.Click += BtLogout_Click;

            if (head == null) { throw new ApplicationException("head not known at this point!?"); }

            LoadPositions();

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

        private void BtLogout_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(MainMenu));
            HelpfulMethods.clearTheStack(this);
        }

        private void BtCreate_Click(object sender, EventArgs e)
        {

            InUseObjects.Set("PackagingItem", null);
            StartActivity(typeof(PackagingUnit));
            HelpfulMethods.clearTheStack(this);
        }
        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            switch (keyCode)
            {
                // Setting F2 to method ProccesStock()
                case Keycode.F2:
                    if (btNext.Enabled == true)
                    {
                        BtNext_Click(this, null);
                    }
                    break;

                case Keycode.F3:
                    if (btUpdate.Enabled == true)
                    {
                        BtUpdate_Click(this, null);
                    }
                    break;

                case Keycode.F4://
                    if (btDelete.Enabled == true)
                    {
                        BtDelete_Click(this, null);
                    }
                    break;

                case Keycode.F5:
                    if (btCreate.Enabled == true)
                    {
                        BtCreate_Click(this, null);
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

        private void BtDelete_Click(object sender, EventArgs e)
        {
            popupDialog = new Dialog(this);
            popupDialog.SetContentView(Resource.Layout.YesNoPopUp);
            popupDialog.Window.SetSoftInputMode(SoftInput.AdjustResize);
            popupDialog.Show();
            popupDialog.Window.SetLayout(LayoutParams.MatchParent, LayoutParams.WrapContent);
            popupDialog.Window.SetBackgroundDrawableResource(Android.Resource.Color.HoloRedLight);
            // Access Popup layout fields like below
            btnYes = popupDialog.FindViewById<Button>(Resource.Id.btnYes);
            btnNo = popupDialog.FindViewById<Button>(Resource.Id.btnNo);
            btnYes.Click += BtnYes_Click;
            btnNo.Click += BtnNo_Click;
        }

        private void BtnNo_Click(object sender, EventArgs e)
        {
            popupDialog.Dismiss();
            popupDialog.Hide();
        }

        private void BtnYes_Click(object sender, EventArgs e)
        {

            {
                var item = positions.Items[displayedPosition];
                var id = item.GetInt("ItemID");


                try
                {

                    string result;
                    if (WebApp.Get("mode=delPackItem&item=" + id.ToString(), out result))
                    {
                        if (result == "OK!")
                        {
                            positions = null;
                            LoadPositions();
                        }
                        else
                        {
                            string WebError = string.Format("Napaka pri brisanju pozicije: " + result);
                            Toast.MakeText(this, WebError, ToastLength.Long).Show();

                            return;
                        }
                    }
                    else
                    {
                        string WebError = string.Format("Napaka pri dostopu do web aplikacije: " + result);
                        Toast.MakeText(this, WebError, ToastLength.Long).Show();

                        return;
                    }
                }
                catch (Exception err)
                {

                    Crashes.TrackError(err);
                    return;

                }


            }
        }

        private void BtUpdate_Click(object sender, EventArgs e)
        {
            var item = positions.Items[displayedPosition];
            InUseObjects.Set("PackagingItem", item);
            StartActivity(typeof(PackagingUnit));
            HelpfulMethods.clearTheStack(this);
        }

        private void BtNext_Click(object sender, EventArgs e)
        {
            displayedPosition++;
            if (displayedPosition >= positions.Items.Count) { displayedPosition = 0; }
            FillDisplayedItem();
        }

        private void LoadPositions()
        {

            try
            {

                if (positions == null)
                {
                    var error = "";

                    if (positions == null)
                    {
                        positions = Services.GetObjectList("pi", out error, head.GetInt("HeadID").ToString());
                        InUseObjects.Set("PackagingItemPositions", positions);
                    }
                    if (positions == null)
                    {
                        string WebError = string.Format("Napaka pri dostopu do web aplikacije: " + error);
                        Toast.MakeText(this, WebError, ToastLength.Long).Show();
                        return;
                    }
                }

                displayedPosition = 0;
                FillDisplayedItem();
            }
            catch (Exception err)
            {

                Crashes.TrackError(err);
                return;

            }
        }
        private void FillDisplayedItem()
        {
            if ((positions != null) && (displayedPosition < positions.Items.Count))
            {
                var item = positions.Items[displayedPosition];
                lbInfo.Text = "Vnesene pozicije na pakiranju (" + (displayedPosition + 1).ToString() + "/" + positions.Items.Count + ")";

                tbIdent.Text = item.GetString("IdentName");
                tbSSCC.Text = item.GetString("SSCC");
                tbSerialNo.Text = item.GetString("SerialNo");
                tbQty.Text = item.GetDouble("Qty").ToString(CommonData.GetQtyPicture());
                tbLocation.Text = item.GetString("Location");

                var created = item.GetDateTime("DateIns");
                tbCreatedBy.Text = created == null ? "" : ((DateTime)created).ToString("dd.MM.") + " " + item.GetString("ClerkName");

                btUpdate.Enabled = true;
                btDelete.Enabled = true;
                btNext.Enabled = true;



                tbIdent.Enabled = false;
                tbSSCC.Enabled = false;
                tbSerialNo.Enabled = false;
                tbQty.Enabled = false;
                tbLocation.Enabled = false;
                tbCreatedBy.Enabled = false;



                tbIdent.SetTextColor(Android.Graphics.Color.Black);
                tbSSCC.SetTextColor(Android.Graphics.Color.Black);
                tbSerialNo.SetTextColor(Android.Graphics.Color.Black);
                tbQty.SetTextColor(Android.Graphics.Color.Black);
                tbLocation.SetTextColor(Android.Graphics.Color.Black);
                tbCreatedBy.SetTextColor(Android.Graphics.Color.Black);
            }
            else
            {
                lbInfo.Text = "Vnesene pozicije na pakiranju (ni)";

                tbIdent.Text = "";
                tbSSCC.Text = "";
                tbSerialNo.Text = "";
                tbQty.Text = "";
                tbLocation.Text = "";
                tbCreatedBy.Text = "";

                btUpdate.Enabled = false;
                btDelete.Enabled = false;
                btNext.Enabled = false;



                tbIdent.Enabled = false;
                tbSSCC.Enabled = false;
                tbSerialNo.Enabled = false;
                tbQty.Enabled = false;
                tbLocation.Enabled = false;
                tbCreatedBy.Enabled = false;

                tbIdent.SetTextColor(Android.Graphics.Color.Black);
                tbSSCC.SetTextColor(Android.Graphics.Color.Black);
                tbSerialNo.SetTextColor(Android.Graphics.Color.Black);
                tbQty.SetTextColor(Android.Graphics.Color.Black);
                tbLocation.SetTextColor(Android.Graphics.Color.Black);
                tbCreatedBy.SetTextColor(Android.Graphics.Color.Black);
            }
        }
    }
}
