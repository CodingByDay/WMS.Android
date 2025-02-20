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
using AlertDialog = Android.App.AlertDialog;
using WebApp = TrendNET.WMS.Device.Services.WebApp;

namespace WMS
{
    [Activity(Label = "WMS")]
    public class ProductionEnteredPositionsView : CustomBaseActivity
    {
        private TextView lbInfo;

        private EditText tbSSCC;
        private EditText tbSerialNumber;
        private EditText tbQty;
        private EditText tbLocation;
        private EditText tbCreatedBy;
        private EditText tbCreatedAt;

        private Button btNext;
        private Button btUpdate;
        private Button button4;
        private Button btFinish;
        private Button btDelete;

        private Button button5;
        private int displayedPosition = 0;
        private NameValueObject moveHead = (NameValueObject)InUseObjects.Get("MoveHead");
        private NameValueObjectList positions = null;
        private Dialog popupDialog;
        private Button btnYes;
        private Button btnNo;
        private ProgressDialogClass progress;
        private Dialog popupDialogConfirm;
        private Button btnYesConfirm;
        private Button btnNoConfirm;
        private ListView listData;
        private UniversalAdapter<ProductionEnteredPositionList> dataAdapter;
        private int selected;
        private int selectedItem;
        private string tempUnit;
        private List<ProductionEnteredPositionList> data = new List<ProductionEnteredPositionList>();

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                SetTheme(Resource.Style.AppTheme_NoActionBar);

                if (App.Settings.tablet)
                {
                    base.RequestedOrientation = ScreenOrientation.Landscape;
                    base.SetContentView(Resource.Layout.ProductionEnteredPositionsViewTablet);
                    listData = FindViewById<ListView>(Resource.Id.listData);
                    dataAdapter = UniversalAdapterHelper.GetProductionEnteredPositionsView(this, data);
                    listData.ItemClick += ListData_ItemClick;
                    listData.ItemLongClick += ListData_ItemLongClick;
                    listData.Adapter = dataAdapter;

                }
                else
                {
                    base.RequestedOrientation = ScreenOrientation.Portrait;
                    base.SetContentView(Resource.Layout.ProductionEnteredPositionsView);
                }

                lbInfo = FindViewById<TextView>(Resource.Id.lbInfo);
                AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
                var _customToolbar = new CustomToolbar(this, toolbar, Resource.Id.navIcon);
                _customToolbar.SetNavigationIcon(App.Settings.RootURL + "/Services/Logo");
                SetSupportActionBar(_customToolbar._toolbar);
                SupportActionBar.SetDisplayShowTitleEnabled(false);
                tbSSCC = FindViewById<EditText>(Resource.Id.tbSSCC);
                tbSerialNumber = FindViewById<EditText>(Resource.Id.tbSerialNumber);
                tbQty = FindViewById<EditText>(Resource.Id.tbQty);
                tbLocation = FindViewById<EditText>(Resource.Id.tbLocation);
                tbCreatedBy = FindViewById<EditText>(Resource.Id.tbCreatedBy);
                tbCreatedAt = FindViewById<EditText>(Resource.Id.tbCreatedAt);

                btNext = FindViewById<Button>(Resource.Id.btNext);
                btUpdate = FindViewById<Button>(Resource.Id.btUpdate);
                button4 = FindViewById<Button>(Resource.Id.button4);
                btFinish = FindViewById<Button>(Resource.Id.btFinish);
                btDelete = FindViewById<Button>(Resource.Id.btDelete);
                button5 = FindViewById<Button>(Resource.Id.button5);
                btNext.Click += BtNext_Click;
                btUpdate.Click += BtUpdate_Click;
                button4.Click += Button4_Click;
                btFinish.Click += BtFinish_Click;
                btDelete.Click += BtDelete_Click;
                button5.Click += Button5_Click;

                InUseObjects.ClearExcept(new string[] { "MoveHead" });
                if (moveHead == null)
                {
                    StartActivity(typeof(MainMenu));
                }
                await LoadPositions();




                var isPalletCodeHidden = await CommonData.GetSettingAsync("Pi.HideLegCode", this);

                if (isPalletCodeHidden != null)
                {
                    if (isPalletCodeHidden != "1")
                    {
                        btFinish.Visibility = ViewStates.Gone;
                    }
                }


                var _broadcastReceiver = new NetworkStatusBroadcastReceiver();
                _broadcastReceiver.ConnectionStatusChanged += OnNetworkStatusChanged;
                Application.Context.RegisterReceiver(_broadcastReceiver,
                new IntentFilter(ConnectivityManager.ConnectivityAction), ReceiverFlags.NotExported);

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
                            data.Clear();
                          
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
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }


        private void ListData_ItemLongClick(object sender, AdapterView.ItemLongClickEventArgs e)
        {
            try
            {
                selected = e.Position;
                Select(selected);
                selectedItem = selected;
                UniversalAdapterHelper.SelectPositionProgramaticaly(listData, selected);
                btUpdate.PerformClick();
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private void ListData_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
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

        private async void Select(int selected)
        {
            try
            {
                displayedPosition = selected;
                if (displayedPosition >= positions.Items.Count) { displayedPosition = 0; }
                await FillDisplayedItem();
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private async Task FillList()
        {
            try
            {
                for (int i = 0; i < positions.Items.Count; i++)
                {
                    if (i < positions.Items.Count && positions.Items.Count > 0)
                    {

                        var item = positions.Items.ElementAt(i);
                        var created = item.GetDateTime("DateInserted");
                        var numbering = i + 1;
                        bool setting;

                        if (await CommonData.GetSettingAsync("ShowNumberOfUnitsField", this) == "1")
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

                        data.Add(new ProductionEnteredPositionList
                        {
                            Ident = item.GetString("Ident"),
                            SerialNumber = item.GetString("SerialNo"),
                            SSCC = item.GetString("SSCC"),
                            Quantity = tempUnit,
                            Location = item.GetString("LocationName")

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
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        public bool IsOnline()
        {
            var cm = (ConnectivityManager)GetSystemService(ConnectivityService);
            return cm.ActiveNetworkInfo == null ? false : cm.ActiveNetworkInfo.IsConnected;
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
                    // Setting F2 to method ProccesStock()
                    case Keycode.F1:
                        if (btNext.Enabled == true)
                        {
                            BtNext_Click(this, null);
                        }
                        break;

                    case Keycode.F2:
                        if (btUpdate.Enabled == true)
                        {
                            BtUpdate_Click(this, null);
                        }
                        break;

                    case Keycode.F3://
                        if (button4.Enabled == true)
                        {
                            Button4_Click(this, null);
                        }
                        break;

                    case Keycode.F4:
                        if (btFinish.Enabled == true)
                        {
                            BtFinish_Click(this, null);
                        }
                        break;

                    case Keycode.F5:
                        if (btDelete.Enabled == true)
                        {
                            BtDelete_Click(this, null);
                        }
                        break;

                    case Keycode.F6:
                        if (button5.Enabled == true)
                        {
                            Button5_Click(this, null);
                        }
                        break;
                        //return true;
                }
                return base.OnKeyDown(keyCode, e);
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
                return false;
            }
        }

        private void Button5_Click(object sender, EventArgs e)
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

                var item = positions.Items[displayedPosition];
                var id = item.GetInt("ItemID");


                try
                {
                    LoaderManifest.LoaderManifestLoopResources(this);
                    var (success, result) = await WebApp.GetAsync("mode=delMoveItem&item=" + id.ToString() + "&deleter=" + Services.UserID().ToString(), this);

                    if (success)
                    {
                        if (result == "OK!")
                        {
                            positions = null;
                            await LoadPositions();
                            popupDialog.Dismiss();
                            popupDialog.Hide();
                        }
                        else
                        {

                            AlertDialog.Builder alert = new AlertDialog.Builder(this);
                            alert.SetTitle($"{Resources.GetString(Resource.String.s265)}");
                            alert.SetMessage($"{Resources.GetString(Resource.String.s212)}" + result);

                            alert.SetPositiveButton("Ok", (senderAlert, args) =>
                            {
                                alert.Dispose();

                            });



                            Dialog dialog = alert.Create();
                            dialog.Show();
                            positions = null;
                            await LoadPositions();
                            popupDialog.Dismiss();
                            popupDialog.Hide();
                            return;
                        }
                    }
                    else
                    {
                        string errorWebApp = string.Format($"{Resources.GetString(Resource.String.s213)}" + result);
                        Toast.MakeText(this, errorWebApp, ToastLength.Long).Show();
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


        private async Task FinishMethod()
        {
            try
            {
                await Task.Run(async () =>
                {
                    var headID = moveHead.GetInt("HeadID");

                    try
                    {

                        var (success, result) = await WebApp.GetAsync("mode=finish&stock=add&print=" + Services.DeviceUser() + "&id=" + headID.ToString(), this);
                        if (success)
                        {
                            if (result.StartsWith("OK!"))
                            {

                                RunOnUiThread(() =>
                                {
                                    var id = result.Split('+')[1];

                                    AlertDialog.Builder alert = new AlertDialog.Builder(this);
                                    alert.SetTitle($"{Resources.GetString(Resource.String.s263)}");
                                    alert.SetMessage($"{Resources.GetString(Resource.String.s264)}" + id);

                                    alert.SetPositiveButton("Ok", (senderAlert, args) =>
                                    {
                                        alert.Dispose();
                                        StartActivity(typeof(MainMenu));
                                        Finish();
                                    });



                                    Dialog dialog = alert.Create();
                                    dialog.Show();
                                });


                            }
                            else
                            {

                                RunOnUiThread(() =>
                                {

                                    AlertDialog.Builder alert = new AlertDialog.Builder(this);
                                    alert.SetTitle($"{Resources.GetString(Resource.String.s265)}");
                                    alert.SetMessage($"{Resources.GetString(Resource.String.s266)}" + result);

                                    alert.SetPositiveButton("Ok", (senderAlert, args) =>
                                    {
                                        alert.Dispose();
                                        StartActivity(typeof(MainMenu));
                                        Finish();

                                    });



                                    Dialog dialog = alert.Create();
                                    dialog.Show();
                                });
                            }
                        }
                        else
                        {
                            RunOnUiThread(() =>
                            {
                                string errorWebApp = string.Format($"{Resources.GetString(Resource.String.s218)}" + result);
                                Toast.MakeText(this, errorWebApp, ToastLength.Long).Show();
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        SentrySdk.CaptureException(ex);
                    }
                });
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
                popupDialogConfirm = new Dialog(this);
                popupDialogConfirm.SetContentView(Resource.Layout.Confirmation);
                popupDialogConfirm.Window.SetSoftInputMode(SoftInput.AdjustResize);
                popupDialogConfirm.Show();

                popupDialogConfirm.Window.SetLayout(LayoutParams.MatchParent, LayoutParams.WrapContent);
                popupDialogConfirm.Window.SetBackgroundDrawable(new ColorDrawable(Color.ParseColor("#081a45")));


                // Access Popup layout fields like below
                btnYesConfirm = popupDialogConfirm.FindViewById<Button>(Resource.Id.btnYes);
                btnNoConfirm = popupDialogConfirm.FindViewById<Button>(Resource.Id.btnNo);
                btnYesConfirm.Click += BtnYesConfirm_Click;
                btnNoConfirm.Click += BtnNoConfirm_Click;

            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private void BtnNoConfirm_Click(object sender, EventArgs e)
        {
            try
            {
                popupDialogConfirm.Dismiss();
                popupDialogConfirm.Hide();
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private async void BtnYesConfirm_Click(object sender, EventArgs e)
        {
            try
            {
                RunOnUiThread(() =>
                {
                    popupDialogConfirm.Dismiss();
                    popupDialogConfirm.Hide();

                    LoaderManifest.LoaderManifestLoopResources(this);
                });
                await FinishMethod();
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            } finally
            {
                RunOnUiThread(() =>
                {
                    LoaderManifest.LoaderManifestLoopStop(this);
                });
            }
        }

        private void Button4_Click(object sender, EventArgs e)
        {
            try
            {
                StartActivity(typeof(ProductionSerialOrSSCCEntry));
                Finish();
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private void BtUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                var item = positions.Items[displayedPosition];
                InUseObjects.Set("MoveItem", item);
                Base.Store.isUpdate = true;
                StartActivity(typeof(ProductionSerialOrSSCCEntry));
                Finish();
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private async void BtNext_Click(object sender, EventArgs e)
        {
            try
            {
                if (App.Settings.tablet)
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
                await FillDisplayedItem();
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

                    if (positions == null)
                    {
                        var error = "";

                        if (positions == null)
                        {
                            positions = Services.GetObjectList("mi", out error, moveHead.GetInt("HeadID").ToString());
                            InUseObjects.Set("TakeOverEnteredPositions", positions);
                        }
                        if (positions == null)
                        {
                            string errorWebApp = string.Format($"{Resources.GetString(Resource.String.s213)}" + error);
                            Toast.MakeText(this, errorWebApp, ToastLength.Long).Show();

                            return;
                        }
                    }

                    displayedPosition = 0;
                    await FillDisplayedItem();

                    if (App.Settings.tablet)
                    {
                        await FillList();
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

        private async Task FillDisplayedItem()
        {
            try
            {
                if ((positions != null) && (displayedPosition < positions.Items.Count))
                {
                    var item = positions.Items[displayedPosition];
                    lbInfo.Text = $"{Resources.GetString(Resource.String.s92)} (" + (displayedPosition + 1).ToString() + "/" + positions.Items.Count + ")";

                    tbSSCC.Text = item.GetString("SSCC");
                    tbSerialNumber.Text = item.GetString("SerialNo");
                    if (await CommonData.GetSettingAsync("ShowNumberOfUnitsField", this) == "1")
                    {
                        tbQty.Text = item.GetDouble("Factor").ToString() + " x " + item.GetDouble("Packing").ToString();
                    }
                    else
                    {
                        tbQty.Text = item.GetDouble("Qty").ToString();
                    }
                    tbLocation.Text = item.GetString("LocationName");
                    tbCreatedBy.Text = item.GetString("ClerkName");

                    var created = item.GetDateTime("DateInserted");
                    tbCreatedAt.Text = created == null ? "" : ((DateTime)created).ToString("dd.MM.yyyy");

                    tbSSCC.Enabled = false;
                    tbSerialNumber.Enabled = false;
                    tbQty.Enabled = false;
                    tbLocation.Enabled = false;
                    tbCreatedBy.Enabled = false;
                    tbCreatedAt.Enabled = false;

                    tbSSCC.SetTextColor(Android.Graphics.Color.Black);
                    tbSerialNumber.SetTextColor(Android.Graphics.Color.Black);
                    tbQty.SetTextColor(Android.Graphics.Color.Black);
                    tbLocation.SetTextColor(Android.Graphics.Color.Black);
                    tbCreatedBy.SetTextColor(Android.Graphics.Color.Black);
                    tbCreatedAt.SetTextColor(Android.Graphics.Color.Black);


                    btUpdate.Enabled = true;
                    btDelete.Enabled = true;
                }
                else
                {
                    lbInfo.Text = $"{Resources.GetString(Resource.String.s267)}";

                    tbSSCC.Text = "";
                    tbSerialNumber.Text = "";
                    tbQty.Text = "";
                    tbLocation.Text = "";
                    tbCreatedBy.Text = "";
                    tbCreatedAt.Text = "";


                    tbSSCC.Enabled = false;
                    tbSerialNumber.Enabled = false;
                    tbQty.Enabled = false;
                    tbLocation.Enabled = false;
                    tbCreatedBy.Enabled = false;
                    tbCreatedAt.Enabled = false;

                    tbSSCC.SetTextColor(Android.Graphics.Color.Black);
                    tbSerialNumber.SetTextColor(Android.Graphics.Color.Black);
                    tbQty.SetTextColor(Android.Graphics.Color.Black);
                    tbLocation.SetTextColor(Android.Graphics.Color.Black);
                    tbCreatedBy.SetTextColor(Android.Graphics.Color.Black);
                    tbCreatedAt.SetTextColor(Android.Graphics.Color.Black);


                    btNext.Enabled = false;
                    btUpdate.Enabled = false;
                    btDelete.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

    }
}