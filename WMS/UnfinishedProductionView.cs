using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Net;
using Android.Views;
using TrendNET.WMS.Core.Data;
using TrendNET.WMS.Device.App;
using TrendNET.WMS.Device.Services;
using WMS.App;
using WMS.ExceptionStore;
using static Android.App.ActionBar;
using WebApp = TrendNET.WMS.Device.Services.WebApp;

namespace WMS
{
    [Activity(Label = "UnfinishedProductionView", ScreenOrientation = ScreenOrientation.Portrait)]
    public class UnfinishedProductionView : CustomBaseActivity, ISwipeListener

    {
        private GestureDetector gestureDetector;
        private TextView lbInfo;
        private EditText tbWorkOrder;
        private EditText tbClient;
        private EditText tbIdent;
        private EditText tbItemCount;
        private EditText tbCreatedBy;
        private EditText tbCreatedAt;
        private Button btNext;
        private Button btFinish;
        private Button btDelete;
        private Button btNew;
        private Button btLogout;
        private int displayedPosition = 0;
        private NameValueObjectList positions = (NameValueObjectList)InUseObjects.Get("TakeOverHeads");
        private Dialog popupDialog;
        private Button btnYes;
        private Button btnNo;
        private ListView listData;
        private UniversalAdapter<UnfinishedProductionList> dataAdapter;
        private List<UnfinishedProductionList> data = new List<UnfinishedProductionList>();
        private int selected;
        private int selectedItem;

        protected async override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                SetTheme(Resource.Style.AppTheme_NoActionBar);
                if (App.Settings.tablet)
                {
                    base.RequestedOrientation = ScreenOrientation.Landscape;
                    base.SetContentView(Resource.Layout.UnfinishedProductionViewTablet);
                    listData = FindViewById<ListView>(Resource.Id.listData);
                    listData.ItemClick += DataList_ItemClick;
                    listData.ItemLongClick += DataList_ItemLongClick;
                    dataAdapter = UniversalAdapterHelper.GetUnfinishedProduction(this, data);
                    listData.Adapter = dataAdapter;
                }
                else
                {
                    base.RequestedOrientation = ScreenOrientation.Portrait;
                    base.SetContentView(Resource.Layout.UnfinishedProductionView);
                }

                LoaderManifest.LoaderManifestLoopResources(this);

                AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
                var _customToolbar = new CustomToolbar(this, toolbar, Resource.Id.navIcon);
                _customToolbar.SetNavigationIcon(App.Settings.RootURL + "/Services/Logo");
                SetSupportActionBar(_customToolbar._toolbar);
                SupportActionBar.SetDisplayShowTitleEnabled(false);
                lbInfo = FindViewById<TextView>(Resource.Id.lbInfo);
                tbWorkOrder = FindViewById<EditText>(Resource.Id.tbWorkOrder);
                tbClient = FindViewById<EditText>(Resource.Id.tbClient);
                tbIdent = FindViewById<EditText>(Resource.Id.tbIdent);
                tbItemCount = FindViewById<EditText>(Resource.Id.tbItemCount);
                tbCreatedBy = FindViewById<EditText>(Resource.Id.tbCreatedBy);
                tbCreatedAt = FindViewById<EditText>(Resource.Id.tbCreatedAt);

                btNext = FindViewById<Button>(Resource.Id.btNext);
                btFinish = FindViewById<Button>(Resource.Id.btFinish);
                btDelete = FindViewById<Button>(Resource.Id.btDelete);
                btNew = FindViewById<Button>(Resource.Id.btNew);
                btLogout = FindViewById<Button>(Resource.Id.btLogout);
                btNext.Click += BtNext_Click;
                btFinish.Click += BtFinish_Click;
                btDelete.Click += BtDelete_Click;
                btLogout.Click += BtLogout_Click;
                btNew.Click += BtNew_Click;
                InUseObjects.Clear();

                await LoadPositions();

                if (App.Settings.tablet)
                {
                    FillItemsList();
                    UniversalAdapterHelper.SelectPositionProgramaticaly(listData, 0);

                }
                var _broadcastReceiver = new NetworkStatusBroadcastReceiver();
                _broadcastReceiver.ConnectionStatusChanged += OnNetworkStatusChanged;
                Application.Context.RegisterReceiver(_broadcastReceiver,
                new IntentFilter(ConnectivityManager.ConnectivityAction), ReceiverFlags.NotExported);

                GestureListener gestureListener = new GestureListener(this);
                gestureDetector = new GestureDetector(this, new GestureListener(this));

                LinearLayout yourLinearLayout = FindViewById<LinearLayout>(Resource.Id.fling);
                // Initialize the GestureDetector
                yourLinearLayout.SetOnTouchListener(gestureListener);


                LoaderManifest.LoaderManifestLoopStop(this);
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }

        }
        public void OnSwipeLeft()
        {
            try
            {
                displayedPosition--;
                if (displayedPosition < 0) { displayedPosition = positions.Items.Count - 1; }
                FillDisplayedItem();
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        public void OnSwipeRight()
        {
            try
            {
                displayedPosition++;
                if (displayedPosition >= positions.Items.Count) { displayedPosition = 0; }
                FillDisplayedItem();
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }


        private void DataList_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            try
            {
                selected = e.Position;
                Select(selected);
                selectedItem = selected;
                UniversalAdapterHelper.SelectPositionProgramaticaly(listData, selected);
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }
        private void DataList_ItemLongClick(object sender, AdapterView.ItemLongClickEventArgs e)
        {
            try
            {
                var index = e.Position;
                DeleteFromTouch(index);
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }
        private void FillItemsList()
        {
            try
            {
                for (int i = 0; i < positions.Items.Count; i++)
                {
                    if (i < positions.Items.Count && positions.Items.Count > 0)
                    {
                        var item = positions.Items.ElementAt(i);
                        var created = item.GetDateTime("DateInserted");

                        // UI changes.
                        RunOnUiThread(() =>
                        {
                            tbCreatedAt.Text = created == null ? "" : ((DateTime)created).ToString("dd.MM.yyyy");
                        });

                        var date = created == null ? "" : ((DateTime)created).ToString("dd.MM.yyyy");
                        data.Add(new UnfinishedProductionList
                        {
                            WorkOrder = tbWorkOrder.Text = item.GetString("LinkKey"),
                            Orderer = item.GetString("Receiver"),
                            Ident = item.GetString("FirstIdent"),
                            NumberOfPositions = item.GetInt("ItemCount").ToString(),
                            // tbItemCount.Text = item.GetInt("ItemCount").ToString();
                        });
                    }
                    else
                    {
                        // UI changes.
                        RunOnUiThread(() =>
                        {
                            string errorWebApp = string.Format($"{Resources.GetString(Resource.String.s247)}");
                            DialogHelper.ShowDialogError(this, this, errorWebApp);
                        });

                    }

                }
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }



        private void No(int index)
        {
            try
            {
                // UI changes.
                RunOnUiThread(() =>
                {
                    popupDialog.Dismiss();
                    popupDialog.Hide();
                });

            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }

        }


        private async Task Yes(int index)
        {
            try
            {
                var item = positions.Items[index];
                var id = item.GetInt("HeadID");


                try
                {


                    var (success, result) = await WebApp.GetAsync("mode=delMoveHead&head=" + id.ToString() + "&deleter=" + Services.UserID().ToString(), this);

                    if (success)
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
                            DialogHelper.ShowDialogError(this, this, errorWebAppIssued);
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
                        DialogHelper.ShowDialogError(this, this, errorWebAppIssued);
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
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }
        private void DeleteFromTouch(int index)
        {
            try
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
                btnYes.Click += async (e, ev) => { await Yes(index); };
                btnNo.Click += (e, ev) => { No(index); };
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private void Select(int postionOfTheItemInTheList)
        {
            try
            {
                displayedPosition = postionOfTheItemInTheList;
                if (displayedPosition >= positions.Items.Count) { displayedPosition = 0; }
                FillDisplayedItem();
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }
        public bool IsOnline()
        {
            try
            {
                var cm = (ConnectivityManager)GetSystemService(ConnectivityService);
                return cm.ActiveNetworkInfo == null ? false : cm.ActiveNetworkInfo.IsConnected;
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
                return false;
            }
        }

        private void OnNetworkStatusChanged(object sender, EventArgs e)
        {
            try
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
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }
        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            try
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
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
                return false;
            }
        }


        public override void OnBackPressed()
        {
            try
            {
                base.OnBackPressed();
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }



        private void BtNew_Click(object sender, EventArgs e)
        {
            try
            {
                StartActivity(typeof(ProductionWorkOrderSetup));
                Finish();
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private void BtLogout_Click(object sender, EventArgs e)
        {
            try
            {
                StartActivity(typeof(MainMenu));
                Finish();
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private void BtDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (positions.Items.Count > 0)
                {
                    popupDialog = new Dialog(this);
                    popupDialog.SetContentView(Resource.Layout.YesNoPopUp);
                    popupDialog.Window.SetSoftInputMode(SoftInput.AdjustResize);
                    popupDialog.Show();
                    popupDialog.Window.SetBackgroundDrawable(new ColorDrawable(Color.ParseColor("#081a45")));
                    // Access Popup layout fields like below
                    btnYes = popupDialog.FindViewById<Button>(Resource.Id.btnYes);
                    btnNo = popupDialog.FindViewById<Button>(Resource.Id.btnNo);
                    btnYes.Click += BtnYes_Click;
                    btnNo.Click += BtnNo_Click;
                }
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private void BtnNo_Click(object sender, EventArgs e)
        {
            try
            {
                popupDialog.Dismiss();
                popupDialog.Hide();
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private async void BtnYes_Click(object sender, EventArgs e)
        {
            try
            {


                try
                {
                    var item = positions.Items[displayedPosition];
                    var id = item.GetInt("HeadID");

                    LoaderManifest.LoaderManifestLoopResources(this);
                    var (success, result) = await WebApp.GetAsync("mode=delMoveHead&head=" + id.ToString() + "&deleter=" + Services.UserID().ToString() + Services.UserID().ToString(), this);

                    if (success)
                    {
                        if (result == "OK!")
                        {
                            positions = null;
                            await LoadPositions();
                            if (App.Settings.tablet)
                            {
                                data.Clear();
                                FillItemsList();
                            }
                            popupDialog.Dismiss();
                            popupDialog.Hide();
                        }
                        else
                        {
                            string errorWebAppProduction = string.Format($"{Resources.GetString(Resource.String.s212)}" + result);
                            DialogHelper.ShowDialogError(this, this, errorWebAppProduction);
                            positions = null;
                            await LoadPositions();

                            popupDialog.Dismiss();
                            popupDialog.Hide();
                            return;
                        }
                    }
                    else
                    {
                        string errorWebAppProduction = string.Format($"{Resources.GetString(Resource.String.s216)}" + result);
                        DialogHelper.ShowDialogError(this, this, errorWebAppProduction);

                        return;
                    }
                }
                catch (Exception err)
                {
                    SentrySdk.CaptureException(err);
                    return;

                }
                finally
                {
                    LoaderManifest.LoaderManifestLoopStop(this);
                }
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private void BtFinish_Click(object sender, EventArgs e)
        {
            try
            {
                var moveHead = positions.Items[displayedPosition];
                moveHead.SetBool("Saved", true);
                InUseObjects.Set("MoveHead", moveHead);
                StartActivity(typeof(ProductionEnteredPositionsView));
                Finish();
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private void BtNext_Click(object sender, EventArgs e)
        {
            try
            {
                if (App.Settings.tablet)
                {
                    selectedItem++;

                    if (selectedItem <= (positions.Items.Count - 1))
                    {
                        UniversalAdapterHelper.SelectPositionProgramaticaly(listData, selectedItem);
                    }
                    else
                    {
                        selectedItem = 0;
                        UniversalAdapterHelper.SelectPositionProgramaticaly(listData, selectedItem);
                    }

                }
                displayedPosition++;
                if (displayedPosition >= positions.Items.Count) { displayedPosition = 0; }
                FillDisplayedItem();
            }     catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private async Task LoadPositions()
        {
            try
            {
                try
                {

                    positions = await AsyncServices.AsyncServices.GetObjectListAsync("mh", "W", this);

                    if (positions == null)
                    {
                        return;
                    }

                    InUseObjects.Set("ProductionHeads", positions);

                    displayedPosition = 0;
                    FillDisplayedItem();
                }
                catch (Exception err)
                {

                    SentrySdk.CaptureException(err);
                    return;

                }
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }
        private void FillDisplayedItem()
        {
            try
            {
                if ((positions != null) && (positions.Items.Count > 0))
                {
                    RunOnUiThread(() =>
                    {
                        lbInfo.Text = $"{Resources.GetString(Resource.String.s12)} (" + (displayedPosition + 1).ToString() + "/" + positions.Items.Count + ")";
                        var item = positions.Items[displayedPosition];

                        tbWorkOrder.Text = item.GetString("LinkKey");
                        tbClient.Text = item.GetString("Receiver");
                        tbIdent.Text = item.GetString("FirstIdent");
                        tbItemCount.Text = item.GetInt("ItemCount").ToString();
                        tbCreatedBy.Text = item.GetString("ClerkName");

                        var created = item.GetDateTime("DateInserted");
                        tbCreatedAt.Text = created == null ? "" : ((DateTime)created).ToString("dd.MM.yyyy");

                        tbWorkOrder.Enabled = false;
                        tbClient.Enabled = false;
                        tbIdent.Enabled = false;
                        tbItemCount.Enabled = false;
                        tbCreatedBy.Enabled = false;
                        tbCreatedAt.Enabled = false;


                        tbWorkOrder.SetTextColor(Android.Graphics.Color.Black);
                        tbClient.SetTextColor(Android.Graphics.Color.Black);
                        tbIdent.SetTextColor(Android.Graphics.Color.Black);
                        tbItemCount.SetTextColor(Android.Graphics.Color.Black);
                        tbCreatedBy.SetTextColor(Android.Graphics.Color.Black);
                        tbCreatedAt.SetTextColor(Android.Graphics.Color.Black);

                        btNext.Enabled = true;
                        btDelete.Enabled = true;
                        btFinish.Enabled = true;
                    });


                }
                else
                {

                    RunOnUiThread(() =>
                    {
                        lbInfo.Text = $"{Resources.GetString(Resource.String.s331)}";

                        tbWorkOrder.Text = "";
                        tbClient.Text = "";
                        tbIdent.Text = "";
                        tbItemCount.Text = "";
                        tbCreatedBy.Text = "";
                        tbCreatedAt.Text = "";

                        tbWorkOrder.Enabled = false;
                        tbClient.Enabled = false;
                        tbIdent.Enabled = false;
                        tbItemCount.Enabled = false;
                        tbCreatedBy.Enabled = false;
                        tbCreatedAt.Enabled = false;

                        tbWorkOrder.SetTextColor(Android.Graphics.Color.Black);
                        tbClient.SetTextColor(Android.Graphics.Color.Black);
                        tbIdent.SetTextColor(Android.Graphics.Color.Black);
                        tbItemCount.SetTextColor(Android.Graphics.Color.Black);
                        tbCreatedBy.SetTextColor(Android.Graphics.Color.Black);
                        tbCreatedAt.SetTextColor(Android.Graphics.Color.Black);


                        btNext.Enabled = false;
                        btDelete.Enabled = false;
                        btFinish.Enabled = false;
                    });

                }
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

    }
}