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
using WMS.Printing;
using static Android.App.ActionBar;
using AlertDialog = Android.App.AlertDialog;
namespace WMS
{
    [Activity(Label = "WMS")]
    public class ProductionCard : CustomBaseActivity
    {
        private EditText tbWorkOrder;
        private EditText tbIdent;
        private EditText tbCardNum;
        private EditText tbSerialNum;
        private EditText tbQty;
        private Button btConfirm;
        private Button btExit;
        private NameValueObject cardInfo = (NameValueObject)InUseObjects.Get("CardInfo");
        private Dialog popupDialog;
        private Button btnYes;
        private Button btnNo;
        private bool target;
        private bool warning;

 

  

        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            try
            {
                switch (keyCode)
                {

                    case Keycode.F4:
                        if (btConfirm.Enabled == true)
                        {
                            BtConfirm_Click(this, null);
                        }
                        break;

                    case Keycode.F8:
                        if (btExit.Enabled == true)
                        {

                            BtExit_Click(this, null);
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

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                SetTheme(Resource.Style.AppTheme_NoActionBar);
                if (App.Settings.tablet)
                {
                    base.RequestedOrientation = ScreenOrientation.Landscape;
                    base.SetContentView(Resource.Layout.ProductionCardTablet);
                }
                else
                {
                    base.RequestedOrientation = ScreenOrientation.Portrait;
                    base.SetContentView(Resource.Layout.ProductionCard);
                }
                AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
                var _customToolbar = new CustomToolbar(this, toolbar, Resource.Id.navIcon);
                _customToolbar.SetNavigationIcon(App.Settings.RootURL + "/Services/Logo");
                SetSupportActionBar(_customToolbar._toolbar);
                SupportActionBar.SetDisplayShowTitleEnabled(false);
                tbWorkOrder = FindViewById<EditText>(Resource.Id.tbWorkOrder);
                tbIdent = FindViewById<EditText>(Resource.Id.tbIdent);
                tbCardNum = FindViewById<EditText>(Resource.Id.tbCardNum);
                tbSerialNum = FindViewById<EditText>(Resource.Id.tbSerialNum);
                tbQty = FindViewById<EditText>(Resource.Id.tbQty);
                btConfirm = FindViewById<Button>(Resource.Id.btConfirm);
                btExit = FindViewById<Button>(Resource.Id.btExit);
                tbWorkOrder.Text = cardInfo.GetString("WorkOrder").Trim();
                tbIdent.Text = cardInfo.GetString("Ident").Trim();
                tbQty.Text = cardInfo.GetDouble("UM1toUM2").ToString("###,###,##0.00");
                btConfirm.Click += BtConfirm_Click;
                btExit.Click += BtExit_Click;

                try
                {
                    string error;
                    var data = Services.GetObject("cwns", tbWorkOrder.Text + "|" + tbIdent.Text + "|0", out error);
                    if (data == null)
                    {
                        string SuccessMessage = string.Format($"{Resources.GetString(Resource.String.s247)}" + error);
                        Toast.MakeText(this, SuccessMessage, ToastLength.Long).Show();

                    }
                    else
                    {
                        if (data.GetBool("Warning"))
                        {
                            try
                            {
                                warning = (bool)await DialogAsync.Show(this, $"{Resources.GetString(Resource.String.s303)}", $"{Resources.GetString(Resource.String.s210)}", Resources.GetString(Resource.String.s201), Resources.GetString(Resource.String.s202));
                            }
                            catch (Exception err)
                            {
                                SentrySdk.CaptureException(err);

                            }
                            if ((bool)warning)
                            {
                                data = Services.GetObject("cwns", tbWorkOrder.Text + "|" + tbIdent.Text + "|1", out error);
                                if (data == null)
                                {
                                    string SuccessMessage = string.Format($"{Resources.GetString(Resource.String.s247)}" + error);
                                    Toast.MakeText(this, SuccessMessage, ToastLength.Long).Show();

                                }
                            }
                            else
                            {
                            }
                        }

                        tbCardNum.Text = data.GetInt("CardNum").ToString();
                        tbSerialNum.Text = data.GetString("SerialNum");
                        btConfirm.Enabled = true;
                    }
                }
                catch (Exception err)
                {
                    SentrySdk.CaptureException(err);

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

        private void BtExit_Click(object sender, EventArgs e)
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

        private void BtConfirm_Click(object sender, EventArgs e)
        {
            try
            {
                var nvo = new NameValueObject("MoveCard");
                nvo.SetString("WorkOrder", tbWorkOrder.Text);
                nvo.SetString("Ident", tbIdent.Text);
                nvo.SetInt("CardNum", Convert.ToInt32(tbCardNum.Text));
                nvo.SetString("SerialNum", tbSerialNum.Text);
                nvo.SetDouble("Qty", Convert.ToDouble(tbQty.Text));
                nvo.SetInt("ClerkIns", Services.UserID());
                var progress = new ProgressDialogClass();

                try
                {
                    string error;
                    nvo = Services.SetObject("cwns", nvo, out error);
                    if (nvo == null)
                    {
                        AlertDialog.Builder alert = new AlertDialog.Builder(this);
                        alert.SetTitle($"{Resources.GetString(Resource.String.s265)}");
                        alert.SetMessage($"{Resources.GetString(Resource.String.s247)}" + error);

                        alert.SetPositiveButton("Ok", (senderAlert, args) =>
                        {
                            alert.Dispose();
                            StartActivity(typeof(MainMenu));
                            Finish();
                        });

                        Dialog dialog = alert.Create();
                        dialog.Show();

                    }
                    else
                    {
                        var pr = new NameValueObject("PrintCard");
                        PrintingCommon.SetNVOCommonData(ref pr);
                        pr.SetInt("CardID", nvo.GetInt("ID"));
                        PrintingCommon.SendToServer(pr);
                        StartActivity(typeof(ProductionCard));
                        Finish();
                    }
                }
                catch (Exception err)
                {
                    SentrySdk.CaptureException(err);

                }
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }
    }
}