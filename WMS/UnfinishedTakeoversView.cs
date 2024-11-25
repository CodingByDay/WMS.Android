﻿using Android.Content;
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
    [Activity(Label = "WMS")]
    public class UnfinishedTakeoversView : CustomBaseActivity, ISwipeListener
    {
        private EditText tbBusEvent;
        private EditText tbOrder;
        private EditText tbSupplier;
        private EditText tbItemCount;
        private EditText tbCreatedBy;
        private EditText tbCreatedAt;
        private Button btNext;
        private Button btFinish;
        private Button btDelete;
        private Button btLogout;
        private TextView lbInfo;
        private Dialog popupDialog;
        private int displayedPosition = 0;
        private Button btnYes;
        private Button btnConfirm;
        private Button btnNo;
        private Button btNew;
        private ListView listData;
        private UniversalAdapter<UnfinishedTakeoverList> dataAdapter;
        private NameValueObjectList positions = (NameValueObjectList)InUseObjects.Get("TakeOverHeads");
        private List<UnfinishedTakeoverList> dataSource = new List<UnfinishedTakeoverList>();
        private GestureDetector gestureDetector;
        private string finalString;
        private int selected = 0;
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
                    base.SetContentView(Resource.Layout.UnfinishedTakeoversViewTablet);
                    listData = FindViewById<ListView>(Resource.Id.listData);
                    dataAdapter = UniversalAdapterHelper.GetUnfinishedTakeover(this, dataSource);
                    listData.Adapter = dataAdapter;
                    listData.ItemClick += DataList_ItemClick;
                    listData.ItemLongClick += DataList_ItemLongClick;
                }
                else
                {
                    base.RequestedOrientation = ScreenOrientation.Portrait;
                    base.SetContentView(Resource.Layout.UnfinishedTakeoversView);
                }

                LoaderManifest.LoaderManifestLoopResources(this);

                AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
                var _customToolbar = new CustomToolbar(this, toolbar, Resource.Id.navIcon);
                _customToolbar.SetNavigationIcon(App.Settings.RootURL + "/Services/Logo");
                SetSupportActionBar(_customToolbar._toolbar);
                SupportActionBar.SetDisplayShowTitleEnabled(false);
                tbBusEvent = FindViewById<EditText>(Resource.Id.tbBusEvent);
                tbOrder = FindViewById<EditText>(Resource.Id.tbOrder);
                tbSupplier = FindViewById<EditText>(Resource.Id.tbSupplier);
                tbItemCount = FindViewById<EditText>(Resource.Id.tbItemCount);
                tbCreatedBy = FindViewById<EditText>(Resource.Id.tbCreatedBy);
                tbCreatedAt = FindViewById<EditText>(Resource.Id.tbCreatedAt);
                btNext = FindViewById<Button>(Resource.Id.btNext);
                btFinish = FindViewById<Button>(Resource.Id.btFinish);
                btDelete = FindViewById<Button>(Resource.Id.btDelete);
                btNew = FindViewById<Button>(Resource.Id.btnew);
                btLogout = FindViewById<Button>(Resource.Id.logout);
                lbInfo = FindViewById<TextView>(Resource.Id.lbInfo);
                btFinish.Click += BtFinish_Click;
                btNext.Click += BtNext_Click;
                btDelete.Click += BtDelete_Click;
                btNew.Click += BtNew_Click;
                btLogout.Click += BtLogout_Click;

                InUseObjects.Clear();

                await LoadPositions();

                if (App.Settings.tablet)
                {

                    FillItemsList();
                    UniversalAdapterHelper.SelectPositionProgramaticaly(listData, 0);

                }

                // Try to get the bitmap
                GestureListener gestureListener = new GestureListener(this);
                gestureDetector = new GestureDetector(this, new GestureListener(this));

                LinearLayout yourLinearLayout = FindViewById<LinearLayout>(Resource.Id.fling);
                // Initialize the GestureDetector
                yourLinearLayout.SetOnTouchListener(gestureListener);

                var _broadcastReceiver = new NetworkStatusBroadcastReceiver();
                _broadcastReceiver.ConnectionStatusChanged += OnNetworkStatusChanged;
                Application.Context.RegisterReceiver(_broadcastReceiver,
                new IntentFilter(ConnectivityManager.ConnectivityAction), ReceiverFlags.NotExported);

                LoaderManifest.LoaderManifestLoopStop(this);
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private void DataList_ItemClick(object? sender, AdapterView.ItemClickEventArgs e)
        {
            try
            { 
                selected = e.Position;
                Select(selected);
                selectedItem = selected;
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
                if (positions != null)
                {
                    selected = postionOfTheItemInTheList;
                    displayedPosition = postionOfTheItemInTheList;
                    if (displayedPosition >= positions.Items.Count) { displayedPosition = 0; }
                    FillDisplayedItem();
                }
            }   
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
         }

        private void DataList_ItemLongClick(object? sender, AdapterView.ItemLongClickEventArgs e)
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



        private void DeleteFromTouch(int index)
        {
            try
            {
                RunOnUiThread(() =>
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
                });
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
                popupDialog.Dismiss();
                popupDialog.Hide();
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
                            dataSource.Clear();
                            FillItemsList();

                            RunOnUiThread(() =>
                            {
                                popupDialog.Dismiss();
                                popupDialog.Hide();
                            });

                        }
                        else
                        {
                            positions = null;
                            string errorWebAppIssued = string.Format($"{Resources.GetString(Resource.String.s212)}" + result);


                            // UI changes.
                            RunOnUiThread(() =>
                            {
                                Toast.MakeText(this, errorWebAppIssued, ToastLength.Long).Show();
                                popupDialog.Dismiss();
                                popupDialog.Hide();
                            });

                            await LoadPositions();
                            return;
                        }
                    }
                    else
                    {
                        // UI changes.
                        RunOnUiThread(() =>
                        {
                            string errorWebAppIssued = string.Format($"{Resources.GetString(Resource.String.s213)}" + result);
                            Toast.MakeText(this, errorWebAppIssued, ToastLength.Long).Show();
                            popupDialog.Dismiss();
                            popupDialog.Hide();
                        });


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

                        if (item.GetString("DocumentTypeName") == "")
                        {
                            var headID = item.GetString("HeadID");
                            finalString = $"Brez-št. {headID} ";
                        }
                        else
                            finalString = item.GetString("LinkKey");

                            if(!item.GetBool("ByOrder"))
                            {
                                 finalString = Resources.GetString(Resource.String.s355);
                            }

                            dataSource.Add(new UnfinishedTakeoverList
                            {
                                Document = finalString,
                                Issuer = item.GetString("Receiver"),
                                Date = date,
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
                            Toast.MakeText(this, errorWebApp, ToastLength.Long).Show();
                        });

                    }
                }
                // UI changes.
                RunOnUiThread(() =>
                {
                    dataAdapter.NotifyDataSetChanged();
                    UniversalAdapterHelper.SelectPositionProgramaticaly(listData, 0);
                });
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
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

                    // return true;


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


                    case Keycode.F8:
                        if (btLogout.Enabled == true)
                        {
                            BtLogout_Click(this, null);
                        }
                        break;
                }
                return base.OnKeyDown(keyCode, e);
            } catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
                return false;
            }
        }

        private void BtLogout_Click(object sender, EventArgs e)
        {
            try
            {
                StartActivity(typeof(MainMenu));
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
                Base.Store.byOrder = true;
                NameValueObject moveHead = new NameValueObject("MoveHead");
                moveHead.SetBool("Saved", false);
                InUseObjects.Set("MoveHead", moveHead);
                StartActivity(typeof(TakeOverBusinessEventSetup));
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
                    popupDialog.Window.SetLayout(LayoutParams.MatchParent, LayoutParams.WrapContent);
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


                    var (success, result) = await WebApp.GetAsync("mode=delMoveHead&head=" + id.ToString() + "&deleter=" + Services.UserID().ToString(), this);

                    if (success)
                    {
                        if (result == "OK!")
                        {
                            positions = null;
                            await LoadPositions();

                            if (App.Settings.tablet)
                            {
                                dataSource.Clear();
                                FillItemsList();
                            }
                            popupDialog.Dismiss();
                            popupDialog.Hide();
                        }
                        else
                        {
                            string errorWebApp = string.Format($"{Resources.GetString(Resource.String.s212)}" + result);
                            Toast.MakeText(this, errorWebApp, ToastLength.Long).Show();
                            positions = null;
                            await LoadPositions();
                            popupDialog.Dismiss();
                            popupDialog.Hide();
                            return;
                        }
                    }
                    else
                    {

                        string errorWebApp = string.Format($"{Resources.GetString(Resource.String.s216)}" + result);
                        Toast.MakeText(this, errorWebApp, ToastLength.Long).Show();
                        popupDialog.Dismiss();
                        popupDialog.Hide();
                        return;
                    }
                }
                catch { }
                finally
                {
                    popupDialog.Dismiss();
                    popupDialog.Hide();
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
                StartActivity(typeof(TakeOverEnteredPositionsView));
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
                    listData.RequestFocusFromTouch();
                    selected++;
                    listData.Clickable = false;
                    if (selected <= (positions.Items.Count - 1))
                    {
                        listData.CheckedItemPositions.Clear();
                        listData.ClearChoices();
                        UniversalAdapterHelper.SelectPositionProgramaticaly(listData, selected);
                    }
                    else
                    {

                        listData.CheckedItemPositions.Clear();
                        listData.ClearChoices();
                        selected = 0;
                        UniversalAdapterHelper.SelectPositionProgramaticaly(listData, selected);
                    }

                    listData.Clickable = true;
                }
                displayedPosition++;
                if (displayedPosition >= positions.Items.Count) { displayedPosition = 0; }
                FillDisplayedItem();
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }
        protected override void OnResume()
        {
            try
            {
                base.OnResume();
                // Activity has become visible (it is now "resumed")
            }
            catch (Exception ex)
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
                    positions = await AsyncServices.AsyncServices.GetObjectListAsync("mh", "I", this);

                    if (positions == null)
                    {
                        return;
                    }

                    InUseObjects.Set("TakeOverHeads", positions);

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
                try
                {
                    if ((positions != null) && (positions.Items.Count > 0))
                    {
                        RunOnUiThread(() =>
                        {
                            lbInfo.Text = $"{Resources.GetString(Resource.String.s12)} (" + (displayedPosition + 1).ToString() + "/" + positions.Items.Count + ")";
                            var item = positions.Items[displayedPosition];
                            tbBusEvent.Text = item.GetString("DocumentTypeName");
                            tbOrder.Text = item.GetString("LinkKey");
                            if (!item.GetBool("ByOrder"))
                            {
                                tbOrder.Text = Resources.GetString(Resource.String.s355);
                                Base.Store.byOrder = false;
                            } else
                            {
                                Base.Store.byOrder = true;
                            }
                            tbSupplier.Text = item.GetString("Receiver");
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
                            btNext.Enabled = false;
                            btDelete.Enabled = false;
                            btFinish.Enabled = false;
                        });

                    }
                }
                catch (Exception ex)
                {
                    SentrySdk.CaptureException(ex);
                }
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
    }
}