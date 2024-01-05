using System;
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
using Scanner.App;
using TrendNET.WMS.Core.Data;
using TrendNET.WMS.Device.App;
using TrendNET.WMS.Device.Services;
using static Android.App.ActionBar;
using WebApp = TrendNET.WMS.Device.Services.WebApp;

namespace Scanner
{
    [Activity(Label = "PackagingUnitListTablet", ScreenOrientation = Android.Content.PM.ScreenOrientation.Landscape)]
    public class PackagingUnitListTablet : Activity

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
        private ListView listData;
        private List<PackagingList> data = new List<PackagingList>();
        private string tempUnit;
        private int selected;
        private int selectedItem=-1;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Create your application here
            SetContentView(Resource.Layout.PackagingUnitListTablet);
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
            listData = FindViewById<ListView>(Resource.Id.listData);
            packagingListAdapter adapter = new packagingListAdapter(this, data);
            listData.Adapter = adapter;
            btNext.Click += BtNext_Click;
            btUpdate.Click += BtUpdate_Click;
            btDelete.Click += BtDelete_Click;
            btCreate.Click += BtCreate_Click;
            btLogout.Click += BtLogout_Click;
            listData.ItemClick += ListData_ItemClick;
            if (head == null) { throw new ApplicationException("head not known at this point!?"); }

            LoadPositions();
            fillList();
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

        private void Select(int postionOfTheItemInTheList)
        {
            displayedPosition = postionOfTheItemInTheList;
            if (displayedPosition >= positions.Items.Count) { displayedPosition = 0; }
            FillDisplayedItem();
        }
        private void ListData_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            selected = e.Position;
            Select(selected);
            selectedItem = selected;
        }


        private void fillList()
        {

            for (int i = 0; i < positions.Items.Count; i++)
            {
                if (i < positions.Items.Count && positions.Items.Count > 0)
                {
                    var item = positions.Items.ElementAt(i);
                    var created = item.GetDateTime("DateInserted");
                    var numbering = i + 1;
                    bool setting;

                    if (CommonData.GetSetting("ShowNumberOfUnitsField") == "1")
                    {
                        setting = false;
                    }
                    else
                    {
                        setting = true;
                    }
                    if (setting)
                    {
                        tempUnit = item.GetDouble("Qty").ToString();

                    }
                    else
                    {
                        tempUnit = item.GetDouble("Factor").ToString();
                    }
                    string error;
                    var ident = item.GetString("Ident").Trim();
                    var openIdent = Services.GetObject("id", ident, out error);
                    //  var ident = CommonData.LoadIdent(item.GetString("Ident"));
                    var identName = openIdent.GetString("Name");
                    var date = created == null ? "" : ((DateTime)created).ToString("dd.MM.yyyy");
                    data.Add(new PackagingList
                    {

                        Ident = item.GetString("Ident").Trim(),
                        SerialNumber = item.GetString("SerialNo"),
                        SSCC = item.GetString("SSCC"),
                        Quantity = tempUnit,
                        Position = numbering.ToString(),
                        Name = identName,

                    });
                    ;
                }
                else
                {
                    string errorWebApp = string.Format("Kritična napaka...");
                    Toast.MakeText(this, errorWebApp, ToastLength.Long).Show();
                }

            }
        }


        private void BtLogout_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(MainMenuTablet));
            HelpfulMethods.clearTheStack(this);
        }

        private void BtCreate_Click(object sender, EventArgs e)
        {

            InUseObjects.Set("PackagingItem", null);
            StartActivity(typeof(PackagingUnitTablet));
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
            StartActivity(typeof(PackagingUnitTablet));
            HelpfulMethods.clearTheStack(this);
        }

        private void BtNext_Click(object sender, EventArgs e)
        {
            selectedItem++;

            if (selectedItem <= (positions.Items.Count - 1))
            {
                listData.RequestFocusFromTouch();
                listData.SetSelection(selectedItem);
                listData.SetItemChecked(selectedItem, true);
            }
            else
            {
                selectedItem = 0;
                listData.RequestFocusFromTouch();
                listData.SetSelection(selectedItem);
                listData.SetItemChecked(selectedItem, true);
            }
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
