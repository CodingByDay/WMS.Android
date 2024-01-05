using Stream = Android.Media.Stream;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Net;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Microsoft.AppCenter.Crashes;
using WMS.App;
using TrendNET.WMS.Core.Data;
using TrendNET.WMS.Device.App;
using TrendNET.WMS.Device.Services;
using static Android.App.ActionBar;
using WebApp = TrendNET.WMS.Device.Services.WebApp;

namespace WMS
{
    [Activity(Label = "UnfinishedIssuedGoodsViewTablet", ScreenOrientation = Android.Content.PM.ScreenOrientation.Landscape)]
    public class UnfinishedIssuedGoodsViewTablet : Activity, ISwipeListener
    {

        private TextView lbInfo;
        private EditText tbBusEvent;
        private EditText tbOrder;
        private EditText tbClient;
        private EditText tbItemCount;
        private EditText tbCreatedBy;
        private EditText tbCreatedAt;
        private Button btFinish;
        private Button btDelete;
        private Button btNew;
        private Button btNext;
        private Button btLogout;
        private Dialog popupDialog;
        private int displayedPosition = 0;
        private NameValueObjectList positions = (NameValueObjectList)InUseObjects.Get("IssuedGoodHeads");
        private Button btnYes;
        private Button btnNo;
        private ListView issuedData;
        private UnfinishedIssuedAdapter adapter;
        private List<UnfinishedIssuedList> data = new List<UnfinishedIssuedList>();
        private int selected;
        private int selectedItem = -1;
        private GestureDetector gestureDetector;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.UnfinishedIssuedGoodsViewTablet);
            AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
            var _customToolbar = new CustomToolbar(this, toolbar, Resource.Id.navIcon);
            _customToolbar.SetNavigationIcon(settings.RootURL + "/Services/Logo");

            tbBusEvent = FindViewById<EditText>(Resource.Id.tbBusEvent);
            tbOrder = FindViewById<EditText>(Resource.Id.tbOrder);
            tbClient = FindViewById<EditText>(Resource.Id.tbClient);
            tbItemCount = FindViewById<EditText>(Resource.Id.tbItemCount);
            tbCreatedBy = FindViewById<EditText>(Resource.Id.tbCreatedBy);
            tbCreatedAt = FindViewById<EditText>(Resource.Id.tbCreatedAt);
            btFinish = FindViewById<Button>(Resource.Id.btFinish);
            btDelete = FindViewById<Button>(Resource.Id.btDelete);
            btNew = FindViewById<Button>(Resource.Id.btNew);
            btLogout = FindViewById<Button>(Resource.Id.btLogout);
            lbInfo = FindViewById<TextView>(Resource.Id.lbInfo);
            issuedData = FindViewById<ListView>(Resource.Id.issuedData);
            adapter = new UnfinishedIssuedAdapter(this, data);
            issuedData.Adapter = adapter;
            issuedData.ItemClick += IssuedData_ItemClick;
            btFinish.Click += BtFinish_Click; //
            btDelete.Click += BtDelete_Click; // 
            btNew.Click += BtNew_Click;       //
            btLogout.Click += BtLogout_Click; //
            issuedData.ItemLongClick += IssuedData_ItemLongClick;
            btNext = FindViewById<Button>(Resource.Id.btNext);
            btNext.Click += BtNext_Click;           
            InUseObjects.Clear();
            LoadPositions();
            FillItemsList();
            issuedData.PerformItemClick(issuedData, 0, 0);
            var _broadcastReceiver = new NetworkStatusBroadcastReceiver();
            _broadcastReceiver.ConnectionStatusChanged += OnNetworkStatusChanged;
            Application.Context.RegisterReceiver(_broadcastReceiver,
            new IntentFilter(ConnectivityManager.ConnectivityAction));




            GestureListener gestureListener = new GestureListener(this);
            gestureDetector = new GestureDetector(this, new GestureListener(this));

            LinearLayout yourLinearLayout = FindViewById<LinearLayout>(Resource.Id.app);
            // Initialize the GestureDetector
            yourLinearLayout.SetOnTouchListener(gestureListener);
        }
        public bool IsOnline()
        {
            var cm = (ConnectivityManager)GetSystemService(ConnectivityService);
            return cm.ActiveNetworkInfo == null ? false : cm.ActiveNetworkInfo.IsConnected;

        }
        public void OnSwipeLeft()
        {
            displayedPosition--;
            if (displayedPosition < 0) { displayedPosition = positions.Items.Count - 1; }
            FillDisplayedItem();
        }

        public void OnSwipeRight()
        {
            displayedPosition++;
            if (displayedPosition >= positions.Items.Count) { displayedPosition = 0; }
            FillDisplayedItem();
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



        private void IssuedData_ItemLongClick(object sender, AdapterView.ItemLongClickEventArgs e)
        {
            var index = e.Position;
            DeleteFromTouch(index);
        }

        public override void OnBackPressed()
        {

            HelpfulMethods.releaseLock();

            base.OnBackPressed();
        }

        private void IssuedData_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            selected = e.Position;
            Select(selected);
            selectedItem = selected;
            issuedData.RequestFocusFromTouch();
            issuedData.SetItemChecked(selected, true);
            issuedData.SetSelection(selected);
        }

        private void Select(int postionOfTheItemInTheList)
        {
            displayedPosition = postionOfTheItemInTheList;
            if (displayedPosition >= positions.Items.Count) { displayedPosition = 0; }
            FillDisplayedItem();
        }

        /// <summary>
        /// Writting a method to fill in all the positions in the right third of the screen...
        /// positions
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// 

        private void DeleteFromTouch(int index)
        {
            popupDialog = new Dialog(this);
            popupDialog.SetContentView(Resource.Layout.YesNoPopUp);
            popupDialog.Window.SetSoftInputMode(SoftInput.AdjustResize);
            popupDialog.Show();
            popupDialog.Window.SetLayout(LayoutParams.MatchParent, LayoutParams.WrapContent);
            popupDialog.Window.SetBackgroundDrawableResource(Android.Resource.Color.HoloOrangeLight);
            // Access Popup layout fields like below
            btnYes = popupDialog.FindViewById<Button>(Resource.Id.btnYes);
            btnNo = popupDialog.FindViewById<Button>(Resource.Id.btnNo);
            btnYes.Click += (e, ev) => { Yes(index); };
            btnNo.Click += (e, ev) => { No(index); };

        }

        private void No(int index)
        {
            popupDialog.Dismiss();
            popupDialog.Hide();
        }

        private void Yes(int index)
        {
            var item = positions.Items[index];
            var id = item.GetInt("HeadID");


            try
            {

                string result;
                if (WebApp.Get("mode=delMoveHead&head=" + id.ToString() + "&deleter=" + Services.UserID().ToString(), out result))
                {
                    if (result == "OK!")
                    {
                        positions = null;
                        LoadPositions();
                        data.Clear();
                        FillItemsList();
                        popupDialog.Dismiss();
                        popupDialog.Hide();
                    }
                    else
                    {
                        string errorWebAppIssued = string.Format("Napaka pri brisanju pozicije " + result);
                        Toast.MakeText(this, errorWebAppIssued, ToastLength.Long).Show();
                        positions = null;
                        LoadPositions();

                        popupDialog.Dismiss();
                        popupDialog.Hide();
                        return;
                    }
                }
                else
                {
                    string errorWebAppIssued = string.Format("Napaka pri dostopu web aplikacije: " + result);
                    Toast.MakeText(this, errorWebAppIssued, ToastLength.Long).Show();
                    popupDialog.Dismiss();
                    popupDialog.Hide();

                    return;
                }
            }
            catch (Exception err)
            {

                Crashes.TrackError(err);
                return;

            }

            string errorWebApp = string.Format("Pozicija uspešno zbrisana.");
            Toast.MakeText(this, errorWebApp, ToastLength.Long).Show();
        }

        private void BtLogout_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(MainMenuTablet));
            HelpfulMethods.clearTheStack(this);

        }

       /* private void SetUpClientPicking(string flow)
        {
            NameValueObject choice = new NameValueObject("CurrentClientFlow");
            choice.SetString("CurrentFlow", flow);
            InUseObjects.Set("CurrentClientFlow", choice);
        }
       */

        private void BtNew_Click(object sender, EventArgs e)
        {
            NameValueObject moveHead = new NameValueObject("MoveHead");
            moveHead.SetBool("Saved", false);
            InUseObjects.Set("MoveHead", moveHead);
            StartActivity(typeof(IssuedGoodsBusinessEventSetupTablet));
            HelpfulMethods.clearTheStack(this);
            Finish();
        }

        private void BtDelete_Click(object sender, EventArgs e)
        {
            popupDialog = new Dialog(this);
            popupDialog.SetContentView(Resource.Layout.YesNoPopUp);
            popupDialog.Window.SetSoftInputMode(SoftInput.AdjustResize);
            popupDialog.Show();
            popupDialog.Window.SetLayout(LayoutParams.MatchParent, LayoutParams.WrapContent);
            popupDialog.Window.SetBackgroundDrawableResource(Android.Resource.Color.HoloOrangeLight);
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
            var item = positions.Items[displayedPosition];
            var id = item.GetInt("HeadID");


            try
            {

                string result;
                if (WebApp.Get("mode=delMoveHead&head=" + id.ToString() + "&deleter=" + Services.UserID().ToString(), out result))
                {
                    if (result == "OK!")
                    {
                        positions = null;
                        LoadPositions();
                        data.Clear();
                        FillItemsList();
                        popupDialog.Dismiss();
                        popupDialog.Hide();
                    }
                    else
                    {
                        string errorWebAppIssued = string.Format("Napaka pri brisanju pozicije " + result);
                        Toast.MakeText(this, errorWebAppIssued, ToastLength.Long).Show();
                        positions = null;
                        LoadPositions();

                        popupDialog.Dismiss();
                        popupDialog.Hide();
                        return;
                    }
                }
                else
                {
                    string errorWebAppIssued = string.Format("Napaka pri dostopu web aplikacije: " + result);
                    Toast.MakeText(this, errorWebAppIssued, ToastLength.Long).Show();
                    popupDialog.Dismiss();
                    popupDialog.Hide();

                    return;
                }
            }
            catch (Exception err)
            {

                Crashes.TrackError(err);
                return;

            }

            string errorWebApp = string.Format("Pozicija uspešno zbrisana.");
            Toast.MakeText(this, errorWebApp, ToastLength.Long).Show();
        }

        private void BtFinish_Click(object sender, EventArgs e)
        {
            var moveHead = positions.Items[displayedPosition];
            moveHead.SetBool("Saved", true);
            InUseObjects.Set("MoveHead", moveHead);

            StartActivity(typeof(IssuedGoodsEnteredPositionsViewTablet));
            HelpfulMethods.clearTheStack(this);
        }

        private void BtNext_Click(object sender, EventArgs e)
        {


            selectedItem++;

            if (selectedItem <= (positions.Items.Count - 1))
            {
                issuedData.RequestFocusFromTouch();
                issuedData.SetSelection(selectedItem);
                issuedData.SetItemChecked(selectedItem, true);
            }
            else
            {
                selectedItem = 0;
                issuedData.RequestFocusFromTouch();
                issuedData.SetSelection(selectedItem);
                issuedData.SetItemChecked(selectedItem, true);
            }


            displayedPosition++;
            if (displayedPosition >= positions.Items.Count) { displayedPosition = 0; }
            FillDisplayedItem();
        }
        private void FillItemsList()
        {

            for (int i = 0; i < positions.Items.Count; i++)
            {
                if (i < positions.Items.Count && positions.Items.Count > 0)
                {
                    var item = positions.Items.ElementAt(i);
                    var created = item.GetDateTime("DateInserted");
                    tbCreatedAt.Text = created == null ? "" : ((DateTime)created).ToString("dd.MM.yyyy");

                    var date = created == null ? "" : ((DateTime)created).ToString("dd.MM.yyyy");
                    data.Add(new UnfinishedIssuedList
                    {
                        Document = item.GetString("DocumentTypeName").Substring(0, 5),
                        Orderer = item.GetString("Issuer"),
                        Date = date,
                        NumberOfPositions = item.GetInt("ItemCount").ToString(),
                        // tbItemCount.Text = item.GetInt("ItemCount").ToString();
                    }); adapter.NotifyDataSetChanged();
                }
                else
                {
                    string errorWebApp = string.Format("Kritična napaka...");
                    Toast.MakeText(this, errorWebApp, ToastLength.Long).Show();
                }
              
            }
            //issuedData.Adapter = null;

            //UnfinishedIssuedAdapter adapter = new UnfinishedIssuedAdapter(this, data);

            //issuedData.Adapter = adapter;



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
                        positions = Services.GetObjectList("mh", out error, "P");
                        InUseObjects.Set("IssuedGoodHeads", positions);
                    }
                    if (positions == null)
                    {
                        string errorWebApp = string.Format("Napaka pri brisanju pozicije:: " + error);
                        Toast.MakeText(this, errorWebApp, ToastLength.Long).Show();
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
            if ((positions != null) && (positions.Items.Count > 0))
            {
                lbInfo.Text = "Odprte odpreme na čitalcu (" + (displayedPosition + 1).ToString() + "/" + positions.Items.Count + ")";
                var item = positions.Items[displayedPosition];

                tbBusEvent.Text = item.GetString("DocumentTypeName");
                tbOrder.Text = item.GetString("LinkKey");
                tbClient.Text = item.GetString("Receiver");
                tbItemCount.Text = item.GetInt("ItemCount").ToString();
                tbCreatedBy.Text = item.GetString("ClerkName");

                var created = item.GetDateTime("DateInserted");
                tbCreatedAt.Text = created == null ? "" : ((DateTime)created).ToString("dd.MM.yyyy");

                tbBusEvent.Enabled = false;
                tbOrder.Enabled = false;
                tbClient.Enabled = false;
                tbItemCount.Enabled = false;
                tbCreatedBy.Enabled = false;
                tbCreatedAt.Enabled = false;


                tbBusEvent.SetTextColor(Android.Graphics.Color.Black);
                tbOrder.SetTextColor(Android.Graphics.Color.Black);
                tbClient.SetTextColor(Android.Graphics.Color.Black);
                tbItemCount.SetTextColor(Android.Graphics.Color.Black);
                tbCreatedBy.SetTextColor(Android.Graphics.Color.Black);
                tbCreatedAt.SetTextColor(Android.Graphics.Color.Black);

                btDelete.Enabled = true;
                btFinish.Enabled = true;
            }
            else
            {
                lbInfo.Text = "Odprte odpreme na čitalcu (ni)";

                tbBusEvent.Text = "";
                tbOrder.Text = "";
                tbClient.Text = "";
                tbItemCount.Text = "";
                tbCreatedBy.Text = "";
                tbCreatedAt.Text = "";

                tbBusEvent.Enabled = false;
                tbOrder.Enabled = false;
                tbClient.Enabled = false;
                tbItemCount.Enabled = false;
                tbCreatedBy.Enabled = false;
                tbCreatedAt.Enabled = false;

                tbBusEvent.SetTextColor(Android.Graphics.Color.Black);
                tbOrder.SetTextColor(Android.Graphics.Color.Black);
                tbClient.SetTextColor(Android.Graphics.Color.Black);
                tbItemCount.SetTextColor(Android.Graphics.Color.Black);
                tbCreatedBy.SetTextColor(Android.Graphics.Color.Black);
                tbCreatedAt.SetTextColor(Android.Graphics.Color.Black);


                //  btDelete.Enabled = false;
                btFinish.Enabled = false;
            }
        }
        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            switch (keyCode)
            {
            
               
                //return true;


                case Keycode.F2:
                    if (btFinish.Enabled == true)
                    {
                        BtFinish_Click(this, null);
                    }
                    break;


                case Keycode.F3:
                    if (btDelete.Enabled == true)
                    {
                        BtDelete_Click(this, null);
                    }
                    break;

                case Keycode.F4:
                    if (btNew.Enabled == true)
                    {
                        BtNew_Click(this, null);
                    }
                    break;


                case Keycode.F5:
                    if (btLogout.Enabled == true)
                    {
                        BtLogout_Click(this, null);
                    }
                    break;



            }
            return base.OnKeyDown(keyCode, e);
        }
    }
}