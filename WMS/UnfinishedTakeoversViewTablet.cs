using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using TrendNET.WMS.Core.Data;
using TrendNET.WMS.Device.App;
using TrendNET.WMS.Device.Services;
using WebApp = TrendNET.WMS.Device.Services.WebApp;

using static Android.App.ActionBar;
using Scanner.App;
using System.ComponentModel;
using Android.Net;
using Microsoft.AppCenter.Crashes;
using Android.Support.Percent;
using AndroidX.PercentLayout.Widget;
using PercentRelativeLayout = AndroidX.PercentLayout.Widget.PercentRelativeLayout;

namespace Scanner
{
    [Activity(Label = "UnfinishedTakeoversViewTablet", ScreenOrientation = Android.Content.PM.ScreenOrientation.Landscape)]
    public class UnfinishedTakeoversViewTablet : Activity, INotifyPropertyChanged, ISwipeListener
    {
        private EditText tbBusEvent;
        private EditText tbOrder;
        private EditText tbSupplier;
        private EditText tbItemCount;
        private EditText tbCreatedBy;
        private EditText tbCreatedAt;
        private Button btFinish;
        private Button btDelete;
        private Button btLogout;
        private TextView lbInfo;
        private Dialog popupDialog;
        private int displayedPosition = -1;
        private Button btnYes;
        private Button btnNo;
        private Button btNew;
        private ListView dataList;
        private UnfinishedTakeoverAdapter adapter;
        private Button btNext;
        private List<UnfinishedTakeoverList> dataSource = new List<UnfinishedTakeoverList>();
        public int selectedItem = -1;
        private NameValueObjectList positions = (NameValueObjectList)InUseObjects.Get("TakeOverHeads");
        private int selected = -1;
        private string finalString;
        private GestureDetector gestureDetector;

        public event PropertyChangedEventHandler PropertyChanged;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Create your application here
            SetContentView(Resource.Layout.UnfinishedTakeoversViewTablet);
            gestureDetector = new GestureDetector(this, new GestureListener(this));
            AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
            var _customToolbar = new CustomToolbar(this, toolbar, Resource.Id.navIcon);
            _customToolbar.SetNavigationIcon(settings.RootURL + "/Services/Logo");
            tbBusEvent = FindViewById<EditText>(Resource.Id.tbBusEvent);
            tbOrder = FindViewById<EditText>(Resource.Id.tbOrder);
            tbSupplier = FindViewById<EditText>(Resource.Id.tbSupplier);
            tbItemCount = FindViewById<EditText>(Resource.Id.tbItemCount);
            tbCreatedBy = FindViewById<EditText>(Resource.Id.tbCreatedBy);
            tbCreatedAt = FindViewById<EditText>(Resource.Id.tbCreatedAt);
            dataList = FindViewById<ListView>(Resource.Id.dataList);
            adapter = new UnfinishedTakeoverAdapter(this, dataSource);
            dataList.Adapter = adapter;
            btFinish = FindViewById<Button>(Resource.Id.btFinish);
            btDelete = FindViewById<Button>(Resource.Id.btDelete);
            btNext = FindViewById<Button>(Resource.Id.btNext);
            btNew = FindViewById<Button>(Resource.Id.btnew);
            btLogout = FindViewById<Button>(Resource.Id.logout);
            lbInfo = FindViewById<TextView>(Resource.Id.lbInfo);
            btNext.Click += BtNext_Click;
            dataList.ChoiceMode = ChoiceMode.Single;
            btFinish.Click += BtFinish_Click;
            btDelete.Click += BtDelete_Click;
            btNew.Click += BtNew_Click;
            btLogout.Click += BtLogout_Click;
            selectedItem = -1;
            dataList.ItemClick += DataList_ItemClick;
            // here
            dataList.ItemSelected += DataList_ItemSelected;
            InUseObjects.Clear();
            dataList.ItemLongClick += DataList_ItemLongClick;
            LoadPositions();
            FillItemsList();
            dataList.PerformItemClick(dataList, 0, 0);
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

        private void DataList_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            var position = e.Position;
            dataList.RequestFocusFromTouch();
            dataList.SetItemChecked(position, true);
            dataList.SetSelection(position);
        }
        public override void OnBackPressed()
        {

            HelpfulMethods.releaseLock();

            base.OnBackPressed();
        }
        private void BtNext_Click1(object sender, EventArgs e)
        {
            displayedPosition++;
            if (displayedPosition >= positions.Items.Count) { displayedPosition = 0; }
            FillDisplayedItem();


            // Change the highlight position.
            dataList.RequestFocusFromTouch();
            dataList.SetItemChecked(displayedPosition, true);
            dataList.SetSelection(displayedPosition);
           
        }

        private void DataList_ItemLongClick(object sender, AdapterView.ItemLongClickEventArgs e)
        {
            var index = e.Position;
            DeleteFromTouch(index);
          
        }



        private void DataList_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            selected = e.Position;
            Select(selected);
            selectedItem = selected;
            dataList.RequestFocusFromTouch();
            dataList.SetItemChecked(selected, true);
            dataList.SetSelection(selected);
        }



        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            switch (keyCode)
            {
                // in smart-phone
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
        private void BtLogout_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(MainMenuTablet));
            HelpfulMethods.clearTheStack(this);
        }

        private void BtNew_Click(object sender, EventArgs e)
        {
            NameValueObject moveHead = new NameValueObject("MoveHead");
            moveHead.SetBool("Saved", false);
            InUseObjects.Set("MoveHead", moveHead);

            StartActivity(typeof(TakeOverBusinessEventSetupTablet));
            HelpfulMethods.clearTheStack(this);
        }

        private void BtDelete_Click(object sender, EventArgs e)
        {
            popupDialog = new Dialog(this);
            popupDialog.SetContentView(Resource.Layout.YesNoPopUp);
            popupDialog.Window.SetSoftInputMode(SoftInput.AdjustResize);
            popupDialog.Show();

            popupDialog.Window.SetLayout(LayoutParams.MatchParent, LayoutParams.WrapContent);
            popupDialog.Window.SetBackgroundDrawableResource(Android.Resource.Color.HoloGreenDark);

            // Access Pop-up layout fields like below
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
                        dataSource.Clear();
                        FillItemsList();
                        popupDialog.Dismiss();
                        popupDialog.Hide();
                    }
                    else
                    {
                        // MessageForm.Show("Napaka pri brisanju pozicije: " + result);
                        string errorWebApp = string.Format("Napaka pri brisanju pozicije " + result);
                        Toast.MakeText(this, errorWebApp, ToastLength.Long).Show();
                        positions = null;
                        LoadPositions();
                        popupDialog.Dismiss();
                        popupDialog.Hide();
                        return;
                    }
                }
                else
                {

                    string errorWebApp = string.Format("Napaka pri brisanju pozicije:: " + result);
                    Toast.MakeText(this, errorWebApp, ToastLength.Long).Show();
                    popupDialog.Dismiss();
                    popupDialog.Hide();
                    return;
                }
            }
            finally
            {
                popupDialog.Dismiss();
                popupDialog.Hide();
            }

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
                        dataSource.Clear();
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

        private void BtFinish_Click(object sender, EventArgs e)
        {
            var moveHead = positions.Items[displayedPosition];
            moveHead.SetBool("Saved", true);
            InUseObjects.Set("MoveHead", moveHead);

            StartActivity(typeof(TakeOverEnteredPositionsViewTablet));
            HelpfulMethods.clearTheStack(this);
        }
        private void Select(int postionOfTheItemInTheList)
        {
            selected = postionOfTheItemInTheList;
            displayedPosition = postionOfTheItemInTheList;
            if (displayedPosition >= positions.Items.Count) { displayedPosition = 0; }
            FillDisplayedItem();
        }

        private void BtNext_Click(object sender, EventArgs e)
        {
            dataList.RequestFocusFromTouch();
            selected++;
            dataList.Clickable = false;
            if (selected <= (positions.Items.Count - 1))
            {

                dataList.CheckedItemPositions.Clear();
                dataList.ClearChoices();
                dataList.RequestFocusFromTouch();
                dataList.SetSelection(selected);
                 
                dataList.SetItemChecked(selected, true);
            }
            else
            {

                dataList.CheckedItemPositions.Clear();
                dataList.ClearChoices();
                selected = 0;
                dataList.RequestFocusFromTouch();
                dataList.SetSelection(selected);
                dataList.SetItemChecked(selected, true);
               
            }

            dataList.Clickable = true;



            displayedPosition++;
            if (displayedPosition >= positions.Items.Count) { displayedPosition = 0; }
            FillDisplayedItem();
        }


        // Load position method...
        private void LoadPositions()
        {

            try
            {

                if (positions == null)
                {
                    var error = "";
                    if (positions == null)
                    {
                        positions = Services.GetObjectList("mh", out error, "I");
                        InUseObjects.Set("TakeOverHeads", positions);
                    }
                    if (positions == null)
                    {
                        // exit 0
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
                    if (item.GetString("DocumentTypeName") == "")
                    {
                        var headID = item.GetString("HeadID");
                        finalString = $"Brez-št. {headID} ";
                    }
                    else
                        finalString = item.GetString("LinkKey");
                    dataSource.Add(new UnfinishedTakeoverList
                    {

                        Document = finalString,
                        Issuer = item.GetString("Receiver"),
                        Date = date,
                        NumberOfPositions = item.GetInt("ItemCount").ToString(),

                        // tbItemCount.Text = item.GetInt("ItemCount").ToString();
                    });
                    adapter.NotifyDataSetChanged();
                }
                else
                {
                    string errorWebApp = string.Format("Kritična napaka...");
                    Toast.MakeText(this, errorWebApp, ToastLength.Long).Show();
                }

            }
  

        }


        private void FillDisplayedItem()
        {
            if ((positions != null) && (positions.Items.Count > 0))
            {
                lbInfo.Text = "Odprti prevzemi na čitalcu (" + (displayedPosition + 1).ToString() + "/" + positions.Items.Count + ")";
                var item = positions.Items[displayedPosition];

                tbBusEvent.Text = item.GetString("DocumentTypeName");
                tbOrder.Text = item.GetString("LinkKey");
                tbSupplier.Text = item.GetString("Receiver");

                var iss1 = item.GetString("Receiver");

  


                tbItemCount.Text = item.GetInt("ItemCount").ToString();
                tbCreatedBy.Text = item.GetString("ClerkName");

                var created = item.GetDateTime("DateInserted");
                tbCreatedAt.Text = created == null ? "" : ((DateTime)created).ToString("dd.MM.yyyy");

                tbBusEvent.Enabled = false;
                tbOrder.Enabled = false;
                tbSupplier.Enabled = false;
                tbItemCount.Enabled = false;
                tbCreatedBy.Enabled = false;
                tbCreatedAt.Enabled = false;


                tbBusEvent.SetTextColor(Android.Graphics.Color.Black);
                tbOrder.SetTextColor(Android.Graphics.Color.Black);
                tbSupplier.SetTextColor(Android.Graphics.Color.Black);
                tbItemCount.SetTextColor(Android.Graphics.Color.Black);
                tbCreatedBy.SetTextColor(Android.Graphics.Color.Black);
                tbCreatedAt.SetTextColor(Android.Graphics.Color.Black);


                btDelete.Enabled = true;
                btFinish.Enabled = true;


            }
            else
            {
                lbInfo.Text = "Odprti prevzemi na čitalcu (ni)";

                tbBusEvent.Text = "";
                tbOrder.Text = "";
                tbSupplier.Text = "";
                tbItemCount.Text = "";
                tbCreatedBy.Text = "";
                tbCreatedAt.Text = "";

                tbBusEvent.Enabled = false;
                tbOrder.Enabled = false;
                tbSupplier.Enabled = false;
                tbItemCount.Enabled = false;
                tbCreatedBy.Enabled = false;
                tbCreatedAt.Enabled = false;




                tbBusEvent.SetTextColor(Android.Graphics.Color.Black);
                tbOrder.SetTextColor(Android.Graphics.Color.Black);
                tbSupplier.SetTextColor(Android.Graphics.Color.Black);
                tbItemCount.SetTextColor(Android.Graphics.Color.Black);
                tbCreatedBy.SetTextColor(Android.Graphics.Color.Black);
                tbCreatedAt.SetTextColor(Android.Graphics.Color.Black);

                btDelete.Enabled = false;
                btFinish.Enabled = false;
            }
        }
    }
}