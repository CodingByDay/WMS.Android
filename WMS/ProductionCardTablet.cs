using Stream = Android.Media.Stream;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Net;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Microsoft.AppCenter.Crashes;
using WMS.App;
using WMS.Printing;

using TrendNET.WMS.Core.Data;
using TrendNET.WMS.Device.App;
using TrendNET.WMS.Device.Services;
using static Android.App.ActionBar;

using AndroidX.AppCompat.App;using AlertDialog = Android.App.AlertDialog;namespace WMS
{
    [Activity(Label = "ProductionCardTablet", ScreenOrientation = Android.Content.PM.ScreenOrientation.Landscape)]
    public class ProductionCardTablet : AppCompatActivity
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

        private bool Response()
        {
            popupDialog = new Dialog(this);
            popupDialog.SetContentView(Resource.Layout.YesNoProductionCard);
            popupDialog.Window.SetSoftInputMode(SoftInput.AdjustResize);
            popupDialog.Show();
            popupDialog.Window.SetLayout(LayoutParams.MatchParent, LayoutParams.WrapContent);
            popupDialog.Window.SetBackgroundDrawableResource(Android.Resource.Color.HoloOrangeLight);
            btnYes = popupDialog.FindViewById<Button>(Resource.Id.btnYes);
            btnNo = popupDialog.FindViewById<Button>(Resource.Id.btnNo);
            btnNo.Click += BtnNo_Click;
            btnYes.Click += BtnYes_Click;
            var res = target;




            return target;
        }

        private void BtnNo_Click(object sender, EventArgs e)
        {
            target = false;
            popupDialog.Hide();
            popupDialog.Dismiss();
        }

        private void BtnYes_Click(object sender, EventArgs e)
        {
            target = true;
            popupDialog.Hide();
            popupDialog.Dismiss();
        }

        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
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

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetTheme(Resource.Style.AppTheme_NoActionBar);




            SetContentView(Resource.Layout.ProductionCardTablet);
            AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
            var _customToolbar = new CustomToolbar(this, toolbar, Resource.Id.navIcon);
            _customToolbar.SetNavigationIcon(settings.RootURL + "/Services/Logo");
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
                    string SuccessMessage = string.Format("Napaka pri pridobivanju podatkov: " + error);
                    Toast.MakeText(this, SuccessMessage, ToastLength.Long).Show();

                }
                else
                {
                    if (data.GetBool("Warning"))
                    {
                        try
                        {
                            warning = (bool)await DialogAsync.Show(this, "Opozorilo", "Izpisanih je bilo zadostno št. etiket, ali želite zamenjati serijsko številko?");
                        }
                        catch (Exception err)
                        {
                            Crashes.TrackError(err);

                        }


                        if ((bool)warning)

                        {
                            data = Services.GetObject("cwns", tbWorkOrder.Text + "|" + tbIdent.Text + "|1", out error);
                            if (data == null)
                            {
                                string SuccessMessage = string.Format("Napaka pri pridobivanju podatkov: " + error);
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
                Crashes.TrackError(err);

            }


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

        private void BtExit_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(MainMenuTablet));
            HelpfulMethods.clearTheStack(this);
        }

        private void BtConfirm_Click(object sender, EventArgs e)
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
                    alert.SetTitle("Napaka");
                    alert.SetMessage("Shranjevanje neuspešno, napaka: " + error);

                    alert.SetPositiveButton("Ok", (senderAlert, args) =>
                    {
                        alert.Dispose();
                        System.Threading.Thread.Sleep(500);
                        this.Finish();
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
                    StartActivity(typeof(ProductionCardTablet));
                    this.Finish();

                    //
                }
            }
            catch (Exception err)
            {
                Crashes.TrackError(err);

            }

        }
    }
}