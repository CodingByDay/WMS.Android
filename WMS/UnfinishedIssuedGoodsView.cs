using Stream = Android.Media.Stream;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Net;
using Android.OS;
using Android.Runtime;
using Android.Speech.Tts;
using Android.Views;
using Android.Widget;
using Java.Util.Concurrent;

using WMS.App;
using TrendNET.WMS.Core.Data;
using TrendNET.WMS.Device.App;
using TrendNET.WMS.Device.Services;
using static Android.App.ActionBar;
using WebApp = TrendNET.WMS.Device.Services.WebApp;

using AndroidX.AppCompat.App;
using AlertDialog = Android.App.AlertDialog;
using Android.Graphics.Drawables;
using Android.Graphics;
using AndroidX.Lifecycle;
using System.Data.Common;
namespace WMS
{
    [Activity(Label = "UnfinishedIssuedGoodsView", ScreenOrientation = ScreenOrientation.Portrait)]
    public class UnfinishedIssuedGoodsView : CustomBaseActivity, ISwipeListener
    {
    
        private TextView lbInfo;
        private EditText tbBusEvent;
        private EditText tbOrder;
        private EditText tbClient;
        private EditText tbItemCount;
        private EditText tbCreatedBy;
        private EditText tbCreatedAt;
        private Button btNext;
        private Button btFinish;
        private Button btDelete;
        private Button btNew;
        private Button btLogout;
        private Dialog popupDialog;
        private int displayedPosition = 0;
        private NameValueObjectList positions = (NameValueObjectList)InUseObjects.Get("IssuedGoodHeads");
        private Button btnYes;
        private Button btnNo;
        private NameValueObject choice;
        private GestureDetector gestureDetector;
        private ListView issuedData;
        private List<UnfinishedIssuedList> data = new List<UnfinishedIssuedList>();
        private int selected;
        private int selectedItem;
        private ListView? listData;
        private UniversalAdapter<UnfinishedIssuedList> dataAdapter;

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
                        Orderer = item.GetString("Receiver"),
                        Date = date,
                        NumberOfPositions = item.GetInt("ItemCount").ToString(),
                        // tbItemCount.Text = item.GetInt("ItemCount").ToString();
                    });
                }
                else
                {
                    string errorWebApp = string.Format($"{Resources.GetString(Resource.String.s247)}");
                    Toast.MakeText(this, errorWebApp, ToastLength.Long).Show();
                }

            }

            dataAdapter.NotifyDataSetChanged();
            UniversalAdapterHelper.SelectPositionProgramaticaly(listData, 0);
        }

        private void No(int index)
        {
            popupDialog.Dismiss();
            popupDialog.Hide();
        }

        private async void Yes(int index)
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
                        await LoadPositions();
                        data.Clear();
                        FillItemsList();
                        popupDialog.Dismiss();
                        popupDialog.Hide();
                    }
                    else
                    {
                        string errorWebAppIssued = string.Format($"{Resources.GetString(Resource.String.s212)}" + result);
                        Toast.MakeText(this, errorWebAppIssued, ToastLength.Long).Show();
                        positions = null;
                        await LoadPositions();

                        popupDialog.Dismiss();
                        popupDialog.Hide();
                        return;
                    }
                }
                else
                {
                    string errorWebAppIssued = string.Format($"{Resources.GetString(Resource.String.s213)}" + result);
                    Toast.MakeText(this, errorWebAppIssued, ToastLength.Long).Show();
                    popupDialog.Dismiss();
                    popupDialog.Hide();

                    return;
                }
            }
            catch (Exception err)
            {

                SentrySdk.CaptureException(err);
                return;

            }

            string errorWebApp = string.Format($"{Resources.GetString(Resource.String.s214)}");
            Toast.MakeText(this, errorWebApp, ToastLength.Long).Show();
        }

        protected async override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetTheme(Resource.Style.AppTheme_NoActionBar);
            if (settings.tablet)
            {
                RequestedOrientation = ScreenOrientation.Landscape;
                SetContentView(Resource.Layout.UnfinishedIssuedGoodsViewTablet);
                listData = FindViewById<ListView>(Resource.Id.listData);
                dataAdapter = UniversalAdapterHelper.GetUnfinishedIssued(this, data);
                listData.ItemClick += ListData_ItemClick;
                listData.ItemLongClick += ListData_ItemLongClick;
                listData.Adapter = dataAdapter;
            }
            else
            {
                RequestedOrientation = ScreenOrientation.Portrait;
                SetContentView(Resource.Layout.UnfinishedIssuedGoodsView);
            }
            LoaderManifest.LoaderManifestLoopResources(this);
            AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
            var _customToolbar = new CustomToolbar(this, toolbar, Resource.Id.navIcon);
            _customToolbar.SetNavigationIcon(settings.RootURL + "/Services/Logo");
            SetSupportActionBar(_customToolbar._toolbar);
            SupportActionBar.SetDisplayShowTitleEnabled(false);
            tbBusEvent = FindViewById<EditText>(Resource.Id.tbBusEvent);
            tbOrder = FindViewById<EditText>(Resource.Id.tbOrder);
            tbClient = FindViewById<EditText>(Resource.Id.tbClient);
            tbItemCount = FindViewById<EditText>(Resource.Id.tbItemCount);
            tbCreatedBy = FindViewById<EditText>(Resource.Id.tbCreatedBy);
            tbCreatedAt = FindViewById<EditText>(Resource.Id.tbCreatedAt);
            btNext = FindViewById<Button>(Resource.Id.btNext);
            btFinish = FindViewById<Button>(Resource.Id.btFinish);
            btDelete = FindViewById<Button>(Resource.Id.btDelete);
            btNew = FindViewById<Button>(Resource.Id.btNew);
            btLogout = FindViewById<Button>(Resource.Id.btLogout);
            lbInfo = FindViewById<TextView>(Resource.Id.lbInfo);
            btNext.Click += BtNext_Click;
            btFinish.Click += BtFinish_Click;
            btDelete.Click += BtDelete_Click;
            btNew.Click += BtNew_Click;
            btLogout.Click += BtLogout_Click;
            InUseObjects.Clear();
            await LoadPositions();
            if(settings.tablet)
            {
                FillItemsList();
            }
            var _broadcastReceiver = new NetworkStatusBroadcastReceiver();
            _broadcastReceiver.ConnectionStatusChanged += OnNetworkStatusChanged;
            Application.Context.RegisterReceiver(_broadcastReceiver,
            new IntentFilter(ConnectivityManager.ConnectivityAction));
            GestureListener gestureListener = new GestureListener(this);
            gestureDetector = new GestureDetector(this, new GestureListener(this));
            LinearLayout yourLinearLayout = FindViewById<LinearLayout>(Resource.Id.fling);
            // Initialize the GestureDetector
            yourLinearLayout.SetOnTouchListener(gestureListener);
            LoaderManifest.LoaderManifestLoopStop(this);
        }

        private void ListData_ItemLongClick(object? sender, AdapterView.ItemLongClickEventArgs e)
        {
            var index = e.Position;
            DeleteFromTouch(index);
        }
        private void DeleteFromTouch(int index)
        {
            popupDialog = new Dialog(this);
            popupDialog.SetContentView(Resource.Layout.YesNoPopUp);
            popupDialog.Window.SetSoftInputMode(SoftInput.AdjustResize);
            popupDialog.Show();

            popupDialog.Window.SetLayout(LayoutParams.MatchParent, LayoutParams.WrapContent);
            popupDialog.Window.SetBackgroundDrawable(new ColorDrawable(Color.ParseColor("#081a45")));


            // Access Popup layout fields like below
            btnYes = popupDialog.FindViewById<Button>(Resource.Id.btnYes);
            btnNo = popupDialog.FindViewById<Button>(Resource.Id.btnNo);
            btnYes.Click += (e, ev) => { Yes(index); };
            btnNo.Click += (e, ev) => { No(index); };
        }
        private void ListData_ItemClick(object? sender, AdapterView.ItemClickEventArgs e)
        {
            selected = e.Position;
            Select(selected);
            selectedItem = selected;
        }
        private void Select(int postionOfTheItemInTheList)
        {
            if (positions != null)
            {
                selected = postionOfTheItemInTheList;
                displayedPosition = postionOfTheItemInTheList;
                if (displayedPosition >= positions.Items.Count) { displayedPosition = 0; }
                FillDisplayedItem();
            }
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

        private void SetUpClientPicking(string flow)
        {
            if (flow == string.Empty)
            {
                Base.Store.modeIssuing = 1;
            }
            else if (flow == "0")
            {
                Base.Store.modeIssuing = 1;
            }
            else if (flow == "1")
            {
                Base.Store.modeIssuing = 2;
            }
            else if (flow == "A")
            {
                Base.Store.modeIssuing = 3;
            }
        }


        private void SetUpClientPickingChange(string flow)
        {
            if (flow == "0")
            {
                Base.Store.modeIssuing = 1;
            }
            else if (flow == "1")
            {
                Base.Store.modeIssuing = 2;
            }
            else if (flow == "3")
            {
                Base.Store.modeIssuing = 3;
            }
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
        public override void OnBackPressed()
        {
            HelpfulMethods.releaseLock();
            base.OnBackPressed();
        }
        private void BtLogout_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(MainMenu));
            HelpfulMethods.clearTheStack(this);
        }

        private void BtNew_Click(object sender, EventArgs e)
        {

            NameValueObject moveHead = new NameValueObject("MoveHead");
            moveHead.SetBool("Saved", false);
            InUseObjects.Set("MoveHead", moveHead);

            // Changing the parameter name because of the old code which is still in use. 15.5.2024 Janko Jovičić 
            string pickingChoice = CommonData.GetSetting("IssueProcessSelect");
            SetUpClientPicking(pickingChoice);

            switch (pickingChoice)
            {
                case "0":
                    // Petpak process
                    StartActivity(typeof(IssuedGoodsBusinessEventSetup));
                    break;
                case "1":
                    // Bimex, soltec
                    StartActivity(typeof(IssuedGoodsBusinessEventSetup));
                    break;
                case "A":
                    // Ski sea
                    StartActivity(typeof(PickingMenu));
                    break;
            }

            HelpfulMethods.clearTheStack(this);
            Finish();
        }

        private void BtDelete_Click(object sender, EventArgs e)
        {
            if (positions.Items.Count > 0)
            {

                popupDialog = new Dialog(this);
                popupDialog.SetContentView(Resource.Layout.YesNoPopUp);
                popupDialog.Window.SetSoftInputMode(SoftInput.AdjustResize);
                popupDialog.Show();
                popupDialog.Window.SetLayout(LayoutParams.MatchParent, LayoutParams.WrapContent);
                popupDialog.Window.SetBackgroundDrawable(new ColorDrawable(Color.ParseColor("#081a45")));

                // Access Popup layout fields like below
                btnYes = popupDialog.FindViewById<Button>(Resource.Id.btnYes);
                btnNo = popupDialog.FindViewById<Button>(Resource.Id.btnNo);
                btnYes.Click += BtnYes_Click;
                btnNo.Click += BtnNo_Click;
            }
        }




        private void BtnNo_Click(object sender, EventArgs e)
        {
            popupDialog.Dismiss();
            popupDialog.Hide();
        }

        private async void BtnYes_Click(object sender, EventArgs e)
        {
            LoaderManifest.LoaderManifestLoopResources(this);

           
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

                        await LoadPositions();
                        if (settings.tablet)
                        {
                            data.Clear();
                            FillItemsList();
                        }

                        popupDialog.Dismiss();
                        popupDialog.Hide();
                    }
                    else
                    {
                        string errorWebAppIssued = string.Format($"{Resources.GetString(Resource.String.s212)}" + result);
                        Toast.MakeText(this, errorWebAppIssued, ToastLength.Long).Show();
                        positions = null;
                        await LoadPositions();
                        popupDialog.Dismiss();
                        popupDialog.Hide();
                        return;
                    }
                }
                else
                {
                    string errorWebAppIssued = string.Format($"{Resources.GetString(Resource.String.s213)}" + result);
                    Toast.MakeText(this, errorWebAppIssued, ToastLength.Long).Show();
                    popupDialog.Dismiss();
                    popupDialog.Hide();

                    return;
                }
            }
            catch { }
            finally
            {
                LoaderManifest.LoaderManifestLoopStop(this);
            }

        }

        private void BtFinish_Click(object sender, EventArgs e)
        {
            var moveHead = positions.Items[displayedPosition];
            moveHead.SetBool("Saved", true);
            if (moveHead != null)
            {
                InUseObjects.Set("MoveHead", moveHead);
                StartActivity(typeof(IssuedGoodsEnteredPositionsView));
                HelpfulMethods.clearTheStack(this);
            } 
        }

        private void BtNext_Click(object sender, EventArgs e)
        {
            if(settings.tablet)
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
            }
            displayedPosition++;
            if (displayedPosition >= positions.Items.Count) { displayedPosition = 0; }
            FillDisplayedItem();
        }

        private async Task LoadPositions()
        {
          
            try
            {


                positions = await AsyncServices.AsyncServices.GetObjectListAsync("mhp", "P");
                    
                if (positions == null)
                {
                    return;
                }

                InUseObjects.Set("IssuedGoodHeads", positions);

                displayedPosition = 0;
                FillDisplayedItem();
            }
            catch(Exception err)
            {
                SentrySdk.CaptureException(err);
            }
        }

        private void FillDisplayedItem()
        {
            if ((positions != null) && (positions.Items.Count > 0))
            {
                lbInfo.Text = $"{Resources.GetString(Resource.String.s205)} (" + (displayedPosition + 1).ToString() + "/" + positions.Items.Count + ")";
                var item = positions.Items[displayedPosition];
                
                tbBusEvent.Text = item.GetString("DocumentTypeName");
                tbOrder.Text = item.GetString("LinkKey");
                tbClient.Text = item.GetString("Receiver");
                tbItemCount.Text = item.GetInt("ItemCount").ToString();
                tbCreatedBy.Text = item.GetString("ClerkName");
                SetUpClientPickingChange(item.GetString("CurrentFlow"));
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

                btNext.Enabled = true;
                btDelete.Enabled = true;
                btFinish.Enabled = true;           

            }
            else
            {
                lbInfo.Text = $"{Resources.GetString(Resource.String.s330)}";

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


                btNext.Enabled = false;
              //  btDelete.Enabled = false;
                btFinish.Enabled = false;
            }
        }
        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            switch (keyCode)
            {
                // in smartphone
                case Keycode.F1:
                    if (btNext.Enabled == true)
                    {
                        BtNext_Click(this, null);
                    }
                    break;
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