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
using static Android.App.ActionBar;
using AlertDialog = Android.App.AlertDialog;
using WebApp = TrendNET.WMS.Device.Services.WebApp;
namespace WMS
{
    [Activity(Label = "InterWarehouseEnteredPositionsView", ScreenOrientation = ScreenOrientation.Portrait)]
    public class InterWarehouseEnteredPositionsView : CustomBaseActivity
    {
        private EditText tbIdent;
        private EditText tbSSCC;
        private EditText tbSerialNumber;
        private EditText tbQty;
        private EditText tbLocation;
        private EditText tbCreatedBy;
        private Button btNext;
        private Button btUpdate;
        private Button button4;
        private Button btFinish;
        private Button btDelete;
        private Button button5;
        private TextView lbInfo;
        private int displayedPosition = 0;
        private NameValueObject moveHead = (NameValueObject)InUseObjects.Get("MoveHead");
        private NameValueObjectList positions = null;
        private Dialog popupDialog;
        private Button btnYes;
        private Button btnNo;
        private ListView listData;
        private UniversalAdapter<InterWarehouseEnteredPositionsViewList> dataAdapter;
        private ProgressDialogClass progress;
        private Dialog popupDialogConfirm;
        private Button btnYesConfirm;
        private Button btnNoConfirm;
        private int selected;
        private int selectedItem;
        private string tempUnit;
        private List<InterWarehouseEnteredPositionsViewList> data = new List<InterWarehouseEnteredPositionsViewList>();

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetTheme(Resource.Style.AppTheme_NoActionBar);
            if (App.Settings.tablet)
            {
                base.RequestedOrientation = ScreenOrientation.Landscape;
                base.SetContentView(Resource.Layout.InterWarehouseEnteredPositionsViewTablet);
                listData = FindViewById<ListView>(Resource.Id.listData);
                dataAdapter = UniversalAdapterHelper.GetInterWarehouseEnteredPositionsView(this, data);
                listData.ItemClick += ListData_ItemClick;
                listData.ItemLongClick += ListData_ItemLongClick;
                listData.Adapter = dataAdapter;
            }
            else
            {
                base.RequestedOrientation = ScreenOrientation.Portrait;
                base.SetContentView(Resource.Layout.InterWarehouseEnteredPositionsView);
            }

            AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
            var _customToolbar = new CustomToolbar(this, toolbar, Resource.Id.navIcon);
            _customToolbar.SetNavigationIcon(App.Settings.RootURL + "/Services/Logo");
            SetSupportActionBar(_customToolbar._toolbar);
            SupportActionBar.SetDisplayShowTitleEnabled(false);
            tbIdent = FindViewById<EditText>(Resource.Id.tbIdent);
            tbSSCC = FindViewById<EditText>(Resource.Id.tbSSCC);
            tbSerialNumber = FindViewById<EditText>(Resource.Id.tbSerialNumber);
            tbQty = FindViewById<EditText>(Resource.Id.tbQty);
            tbLocation = FindViewById<EditText>(Resource.Id.tbLocation);
            tbCreatedBy = FindViewById<EditText>(Resource.Id.tbCreatedBy);
            lbInfo = FindViewById<TextView>(Resource.Id.lbInfo);

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
            if (moveHead == null) { throw new ApplicationException("moveHead not known at this point!?"); }

            LoadPositions();

            if (App.Settings.tablet)
            {
                fillItems();
                UniversalAdapterHelper.SelectPositionProgramaticaly(listData, 0);
            }

        }


        private void ListData_ItemLongClick(object sender, AdapterView.ItemLongClickEventArgs e)
        {
            selected = e.Position;
            Select(selected);
            selectedItem = selected;
            btUpdate.PerformClick();
        }



        private void ListData_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            selected = e.Position;
            Select(selected);
            selectedItem = selected;
        }

        private void Select(int postionOfTheItemInTheList)
        {
            displayedPosition = postionOfTheItemInTheList;
            if (displayedPosition >= positions.Items.Count) { displayedPosition = 0; }
            FillDisplayedItem();
        }

        private void fillItems()
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
                    else // random comment number 23
                    {
                        setting = true;
                    }
                    if (setting)
                    { // Saved data.
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
                    data.Add(new InterWarehouseEnteredPositionsViewList
                    {
                        Ident = item.GetString("Ident"),
                        SerialNumber = item.GetString("SerialNo"),
                        SSCC = item.GetString("SSCC"),
                        Quantity = tempUnit,
                        Position = numbering.ToString(),
                        Name = identName.Trim(),


                    }); // Add adapter handler.

                }
                else
                {
                    string errorWebApp = string.Format("Kritična napaka...");
                    DialogHelper.ShowDialogError(this, this, errorWebApp);
                }

            }




        }

        private void Button5_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(MainMenu));
            HelpfulMethods.clearTheStack(this);
        }



        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            switch (keyCode)
            {

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

        private void BtDelete_Click(object sender, EventArgs e)
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
        private void BtnNo_Click(object sender, EventArgs e)
        {
            popupDialog.Dismiss();
            popupDialog.Hide();
        }

        private async void BtnYes_Click(object sender, EventArgs e)
        {
            var item = positions.Items[displayedPosition];
            var id = item.GetInt("ItemID");


            try
            {

                var (success, result) = await WebApp.GetAsync("mode=delMoveItem&item=" + id.ToString() + "&deleter=" + Services.UserID().ToString(), this);
                if (success)
                {
                    if (result == "OK!")
                    {
                        positions = null;

                        LoadPositions();

                        if (App.Settings.tablet)
                        {
                            data.Clear();
                            fillItems();
                        }
                        popupDialog.Dismiss();
                        popupDialog.Hide();
                    }
                    else
                    {
                        string WebErrors = string.Format($"{Resources.GetString(Resource.String.s212)}" + result);
                        DialogHelper.ShowDialogError(this, this, WebErrors);

                        positions = null;
                        LoadPositions();
                        popupDialog.Dismiss();
                        popupDialog.Hide();
                        return;
                    }
                }
                else
                {
                    string WebErrora = string.Format($"{Resources.GetString(Resource.String.s213)}" + result);
                    DialogHelper.ShowDialogError(this, this, WebErrora);
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

            string WebError = string.Format($"{Resources.GetString(Resource.String.s214)}");
            DialogHelper.ShowDialogError(this, this, WebError);
            popupDialog.Dismiss();
            popupDialog.Hide();
        }

        private async Task FinishMethod()
        {
            await Task.Run(async () =>
            {

                RunOnUiThread(() =>
                {
                    progress = new ProgressDialogClass();

                    progress.ShowDialogSync(this, $"{Resources.GetString(Resource.String.s262)}");
                });

                try
                {
                    var headID = moveHead.GetInt("HeadID");

                    var (success, result) = await WebApp.GetAsync("mode=finish&print=" + Services.DeviceUser() + "&id=" + headID.ToString(), this);
                    if (success)
                    {
                        if (result.StartsWith("OK!"))
                        {

                            RunOnUiThread(() =>
                            {
                                progress.StopDialogSync();

                                var id = result.Split('+')[1];

                                AlertDialog.Builder alert = new AlertDialog.Builder(this);
                                alert.SetTitle($"{Resources.GetString(Resource.String.s263)}");
                                alert.SetMessage($"{Resources.GetString(Resource.String.s264)}" + id);

                                alert.SetPositiveButton("Ok", (senderAlert, args) =>
                                {
                                    alert.Dispose();
                                    System.Threading.Thread.Sleep(500);
                                    StartActivity(typeof(MainMenu));
                                });
                                Dialog dialog = alert.Create();
                                dialog.Show();
                            });

                        }
                        else
                        {
                            RunOnUiThread(() =>
                            {
                                progress.StopDialogSync();

                                AlertDialog.Builder alert = new AlertDialog.Builder(this);
                                alert.SetTitle($"{Resources.GetString(Resource.String.s265)}");
                                alert.SetMessage($"{Resources.GetString(Resource.String.s266)}" + result);

                                alert.SetPositiveButton("Ok", (senderAlert, args) =>
                                {
                                    alert.Dispose();
                                    System.Threading.Thread.Sleep(500);
                                    StartActivity(typeof(MainMenu));
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
                            progress.StopDialogSync();
                            string WebError = string.Format($"{Resources.GetString(Resource.String.s218)}" + result);
                            DialogHelper.ShowDialogError(this, this, WebError);
                        });


                    }
                }
                catch (Exception err)
                {

                    SentrySdk.CaptureException(err);


                }
                finally
                {
                    RunOnUiThread(() =>
                    {
                        progress.StopDialogSync();

                    });
                }


            });



        }
        private async void BtFinish_Click(object sender, EventArgs e)
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

        private void BtnNoConfirm_Click(object sender, EventArgs e)
        {
            popupDialogConfirm.Dismiss();
            popupDialogConfirm.Hide();
        }

        private async void BtnYesConfirm_Click(object sender, EventArgs e)
        {
            await FinishMethod();

        }

        private void Button4_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(InterWarehouseSerialOrSSCCEntry));
            HelpfulMethods.clearTheStack(this);
        }

        private void BtUpdate_Click(object sender, EventArgs e)
        {
            var item = positions.Items[displayedPosition];
            InUseObjects.Set("MoveItem", item);
            Base.Store.isUpdate = true;
            StartActivity(typeof(InterWarehouseSerialOrSSCCEntry));
            HelpfulMethods.clearTheStack(this);
        }

        private void BtNext_Click(object sender, EventArgs e)
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
                        positions = Services.GetObjectList("mi", out error, moveHead.GetInt("HeadID").ToString());
                        InUseObjects.Set("TakeOverEnteredPositions", positions);
                    }
                    if (positions == null)
                    {
                        string WebError = string.Format($"{Resources.GetString(Resource.String.s213)}" + error);
                        DialogHelper.ShowDialogError(this, this, WebError);
                        return;
                    }
                }
                displayedPosition = 0;
                FillDisplayedItem();
            }
            catch (Exception err)
            {
                SentrySdk.CaptureException(err);
                return;
            }
        }
        private void FillDisplayedItem()
        {
            if ((positions != null) && (displayedPosition < positions.Items.Count))
            {
                var item = positions.Items[displayedPosition];
                lbInfo.Text = $"{Resources.GetString(Resource.String.s92)} (" + (displayedPosition + 1).ToString() + "/" + positions.Items.Count + ")";

                tbIdent.Text = item.GetString("IdentName");
                tbSSCC.Text = item.GetString("SSCC");
                tbSerialNumber.Text = item.GetString("SerialNo");
                if (CommonData.GetSetting("ShowNumberOfUnitsField") == "1")
                {
                    tbQty.Text = item.GetDouble("Factor").ToString() + " x " + item.GetDouble("Packing").ToString();
                }
                else
                {
                    tbQty.Text = item.GetDouble("Qty").ToString();
                }
                tbLocation.Text = item.GetString("LocationName");

                var created = item.GetDateTime("DateInserted");
                tbCreatedBy.Text = created == null ? "" : ((DateTime)created).ToString("dd.MM.") + " " + item.GetString("ClerkName");


                tbIdent.Enabled = false;
                tbSSCC.Enabled = false;
                tbSerialNumber.Enabled = false;
                tbQty.Enabled = false;
                tbLocation.Enabled = false;
                tbCreatedBy.Enabled = false;


            }
            else
            {
                lbInfo.Text = $"{Resources.GetString(Resource.String.s267)}";

                tbIdent.Text = "";
                tbSSCC.Text = "";
                tbSerialNumber.Text = "";
                tbQty.Text = "";
                tbLocation.Text = "";
                tbCreatedBy.Text = "";
                tbIdent.Enabled = false;
                tbSSCC.Enabled = false;
                tbSerialNumber.Enabled = false;
                tbQty.Enabled = false;
                tbLocation.Enabled = false;
                tbCreatedBy.Enabled = false;
                btUpdate.Enabled = false;
                btDelete.Enabled = false;
                btNext.Enabled = false;
            }
        }
    }
}