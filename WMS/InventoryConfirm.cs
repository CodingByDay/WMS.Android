using Android.Content;
using Android.Content.PM;
using Android.Net;
using Android.Views;
using TrendNET.WMS.Core.Data;
using TrendNET.WMS.Device.App;
using TrendNET.WMS.Device.Services;
using WMS.App;
using WMS.ExceptionStore;
using WebApp = TrendNET.WMS.Device.Services.WebApp;
namespace WMS
{
    [Activity(Label = "WMS")]
    public class InventoryConfirm : CustomBaseActivity
    {
        private TextView lbInfo;
        private EditText tbWarehouse;
        private EditText tbTitle;
        private EditText tbDate;
        private EditText tbItems;
        private EditText tbCreatedBy;
        private EditText tbCreatedAt;
        private ProgressBar progres;
        private Button btNext;
        private Button target;
        private Button button3;
        private int displayedPosition = 0;

        private NameValueObjectList positions = null;
        private int output;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                SetTheme(Resource.Style.AppTheme_NoActionBar);
                if (App.Settings.tablet)
                {
                    base.RequestedOrientation = ScreenOrientation.Landscape;
                    base.SetContentView(Resource.Layout.InventoryConfirmTablet);
                }
                else
                {
                    base.RequestedOrientation = ScreenOrientation.Portrait;
                    base.SetContentView(Resource.Layout.InventoryConfirm);
                }
                AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
                var _customToolbar = new CustomToolbar(this, toolbar, Resource.Id.navIcon);
                _customToolbar.SetNavigationIcon(App.Settings.RootURL + "/Services/Logo");
                SetSupportActionBar(_customToolbar._toolbar);
                SupportActionBar.SetDisplayShowTitleEnabled(false);
                lbInfo = FindViewById<TextView>(Resource.Id.lbInfo);
                tbWarehouse = FindViewById<EditText>(Resource.Id.tbWarehouse);
                tbTitle = FindViewById<EditText>(Resource.Id.tbTitle);
                tbDate = FindViewById<EditText>(Resource.Id.tbDate);
                tbItems = FindViewById<EditText>(Resource.Id.tbItems);
                tbCreatedBy = FindViewById<EditText>(Resource.Id.tbCreatedBy);
                tbCreatedAt = FindViewById<EditText>(Resource.Id.tbCreatedAt);
                target = FindViewById<Button>(Resource.Id.target);
                target.Click += Target_Click;
                btNext = FindViewById<Button>(Resource.Id.btNext);

                button3 = FindViewById<Button>(Resource.Id.button3);

                btNext.Click += BtNext_Click;

                button3.Click += Button3_Click;

                InUseObjects.Clear();

                LoadPositions();

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



        public async Task DoWorkAsync()
        {
            try
            {
                await Task.Run(async () =>
                {

                    var moveHead = positions.Items[displayedPosition];

                    try
                    {
                        var headID = moveHead.GetInt("HeadID");

                        var (success, result) = await WebApp.GetAsync("mode=finish&id=" + headID.ToString(), this);

                        if (success)
                        {
                            if (result.StartsWith("OK!"))
                            {
                                // UI changes.
                                RunOnUiThread(() =>
                                {
                                    var id = result.Split('+')[1];
                                    DialogHelper.ShowDialogSuccess(this, this, $"{Resources.GetString(Resource.String.s279)}" + id);
                                    output = 1;
                                    StartActivity(typeof(MainMenu));
                                });

                            }
                            else
                            {
                                // UI changes.
                                RunOnUiThread(() =>
                                {
                                    output = 2;
                                    DialogHelper.ShowDialogError(this, this, $"{Resources.GetString(Resource.String.s280)}" + result);
                                });

                            }
                        }
                        else
                        {
                            // UI changes.
                            RunOnUiThread(() =>
                            {
                                DialogHelper.ShowDialogError(this, this, $"{Resources.GetString(Resource.String.s218)}" + result);
                            });
                        }
                    }
                    catch (Exception err)
                    {

                        SentrySdk.CaptureException(err);
                        return;

                    }
                });
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }



        private async void Target_Click(object sender, EventArgs e)
        {
            try
            {
                await DoWorkAsync();

                if (output == 1)
                {
                    Toast.MakeText(this, $"{Resources.GetString(Resource.String.s243)}", ToastLength.Long).Show();
                }
                else
                {
                    Toast.MakeText(this, $"{Resources.GetString(Resource.String.s244)}", ToastLength.Long).Show();
                }
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }


        private void Button3_Click(object sender, EventArgs e)
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


        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            try
            {
                switch (keyCode)
                {

                    case Keycode.F2:
                        if (btNext.Enabled == true)
                        {
                            BtNext_Click(this, null);
                        }
                        break;


                    case Keycode.F3:
                        if (target.Enabled == true)
                        {
                            Target_Click(this, null);
                        }
                        break;

                    case Keycode.F8:
                        if (button3.Enabled == true)
                        {
                            Button3_Click(this, null);
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



        private void BtNext_Click(object sender, EventArgs e)
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

        private void LoadPositions()
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
                            positions = Services.GetObjectList("mh", out error, "N");
                        }
                        if (positions == null)
                        {
                            DialogHelper.ShowDialogError(this, this, $"{Resources.GetString(Resource.String.s213)}" + error);

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
                    lbInfo.Text = $"{Resources.GetString(Resource.String.s123)} (" + (displayedPosition + 1).ToString() + "/" + positions.Items.Count + ")";
                    var item = positions.Items[displayedPosition];

                    tbWarehouse.Text = item.GetString("Wharehouse");
                    tbTitle.Text = item.GetString("WharehouseName");
                    var date = item.GetDateTime("Date");
                    tbDate.Text = date == null ? "" : ((DateTime)date).ToString("dd.MM.yyyy");
                    tbItems.Text = item.GetInt("ItemCount").ToString();
                    tbCreatedBy.Text = item.GetString("ClerkName");

                    var created = item.GetDateTime("DateInserted");
                    tbCreatedAt.Text = created == null ? "" : ((DateTime)created).ToString("dd.MM.yyyy");
                    btNext.Enabled = true;
                    target.Enabled = true;
                    tbWarehouse.Enabled = false;
                    tbTitle.Enabled = false;
                    tbDate.Enabled = false;
                    tbItems.Enabled = false;
                    tbCreatedBy.Enabled = false;
                    tbCreatedAt.Enabled = false;
                    tbWarehouse.SetTextColor(Android.Graphics.Color.Black);
                    tbTitle.SetTextColor(Android.Graphics.Color.Black);
                    tbDate.SetTextColor(Android.Graphics.Color.Black);
                    tbItems.SetTextColor(Android.Graphics.Color.Black);
                    tbCreatedBy.SetTextColor(Android.Graphics.Color.Black);
                    tbCreatedAt.SetTextColor(Android.Graphics.Color.Black);

                }
                else
                {
                    lbInfo.Text = $"{Resources.GetString(Resource.String.s281)}";

                    tbWarehouse.Text = "";
                    tbTitle.Text = "";
                    tbDate.Text = "";
                    tbItems.Text = "";
                    tbCreatedBy.Text = "";
                    tbCreatedAt.Text = "";
                    btNext.Enabled = false;
                    target.Enabled = false;
                    tbWarehouse.Enabled = false;
                    tbTitle.Enabled = false;
                    tbDate.Enabled = false;
                    tbItems.Enabled = false;
                    tbCreatedBy.Enabled = false;
                    tbCreatedAt.Enabled = false;
                    tbWarehouse.SetTextColor(Android.Graphics.Color.Black);
                    tbTitle.SetTextColor(Android.Graphics.Color.Black);
                    tbDate.SetTextColor(Android.Graphics.Color.Black);
                    tbItems.SetTextColor(Android.Graphics.Color.Black);
                    tbCreatedBy.SetTextColor(Android.Graphics.Color.Black);
                    tbCreatedAt.SetTextColor(Android.Graphics.Color.Black);
                }
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

    }
}