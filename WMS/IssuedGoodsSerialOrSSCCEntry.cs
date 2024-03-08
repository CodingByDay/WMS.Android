using Stream = Android.Media.Stream;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.Media;
using Android.Net;
using Android.Nfc;
using Android.OS;
using Android.Runtime;
using Android.Text.Util;
using Android.Views;
using Android.Widget;
using BarCode2D_Receiver;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Newtonsoft.Json;
using WMS.App;
using TrendNET.WMS.Core.Data;
using TrendNET.WMS.Device.App;
using TrendNET.WMS.Device.Services;
using static Android.App.ActionBar;
using static Android.Graphics.Paint;
using static Android.Icu.Text.Transliterator;
using WebApp = TrendNET.WMS.Device.Services.WebApp;
using System.Text.Json;

using AndroidX.AppCompat.App;using AlertDialog = Android.App.AlertDialog;

namespace WMS
{
    [Activity(Label = "IssuedGoodsSerialOrSSCCEntry", ScreenOrientation = ScreenOrientation.Portrait)]
    public class IssuedGoodsSerialOrSSCCEntry : AppCompatActivity, IBarcodeResult
    {
      
        private EditText tbIdent;
        private EditText tbSSCC;
        private EditText tbSerialNum;
        private EditText tbLocation;
        private EditText tbPacking;
        private EditText tbUnits;
        private EditText tbPalette;
        private NameValueObject openIdent = (NameValueObject)InUseObjects.Get("OpenIdent");
        private NameValueObject openOrder = (NameValueObject)InUseObjects.Get("OpenOrder");
        private ApiResultSet OpenOrderItem = (ApiResultSet)InUseObjects.Get("OpenOrderItem");
        private NameValueObject moveHead = (NameValueObject)InUseObjects.Get("MoveHead");
        private NameValueObject moveItem = (NameValueObject)InUseObjects.Get("MoveItem");
        private NameValueObject CurrentFlow = (NameValueObject)InUseObjects.Get("CurrentClientFlow");
        private NameValueObject extraData = (NameValueObject)InUseObjects.Get("ExtraData");
        private NameValueObject lastItem = (NameValueObject)InUseObjects.Get("LastItem");

        private Button btCreateSame;
        private Button btCreate;
        private Button btFinish;
        private Button btOverview;
        private Button btExit;

        private TextView lbQty;
        private bool isPackaging = false;
        private TextView lbUnits;
        private TextView lbPalette; 
        SoundPool soundPool;
        int soundPoolId;
        private bool isOpened = false;
        private Trail receivedTrail;
        private List<string> locations = new List<string>();
        double qtyCheck = 0;
        private LinearLayout ssccRow;
        private LinearLayout serialRow;


        protected async override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetTheme(Resource.Style.AppTheme_NoActionBar);
            LoaderManifest.LoaderManifestLoopResources(this);
            SetContentView(Resource.Layout.IssuedGoodsSerialOrSSCCEntry);

            // Definitions

            AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
            var _customToolbar = new CustomToolbar(this, toolbar, Resource.Id.navIcon);
            _customToolbar.SetNavigationIcon(settings.RootURL + "/Services/Logo");
            SetSupportActionBar(_customToolbar._toolbar);
            SupportActionBar.SetDisplayShowTitleEnabled(false);
            tbIdent = FindViewById<EditText>(Resource.Id.tbIdent);
            tbSSCC = FindViewById<EditText>(Resource.Id.tbSSCC);
            tbSerialNum = FindViewById<EditText>(Resource.Id.tbSerialNum);
            tbLocation = FindViewById<EditText>(Resource.Id.tbLocation);
            tbPacking = FindViewById<EditText>(Resource.Id.tbPacking);
            tbUnits = FindViewById<EditText>(Resource.Id.tbUnits);
            tbPalette = FindViewById<EditText>(Resource.Id.tbPalette);
            tbIdent.InputType = Android.Text.InputTypes.ClassNumber;
            tbSSCC.InputType = Android.Text.InputTypes.ClassNumber;
            tbLocation.InputType = Android.Text.InputTypes.ClassText;
            tbUnits.InputType = Android.Text.InputTypes.ClassNumber;
            tbPalette.InputType = Android.Text.InputTypes.ClassNumber;
            lbQty = FindViewById<TextView>(Resource.Id.lbQty);
            lbUnits = FindViewById<TextView>(Resource.Id.lbUnits);
            lbPalette = FindViewById<TextView>(Resource.Id.lbPalette);        
            soundPool = new SoundPool(10, Stream.Music, 0);
            soundPoolId = soundPool.Load(this, Resource.Raw.beep, 1);
            Barcode2D barcode2D = new Barcode2D();
            barcode2D.open(this, this);


            btCreateSame = FindViewById<Button>(Resource.Id.btCreateSame);
            btCreate = FindViewById<Button>(Resource.Id.btCreate);
            btFinish = FindViewById<Button>(Resource.Id.btFinish);
            btOverview = FindViewById<Button>(Resource.Id.btOverview);
            btExit = FindViewById<Button>(Resource.Id.btExit);




            var _broadcastReceiver = new NetworkStatusBroadcastReceiver();
            _broadcastReceiver.ConnectionStatusChanged += OnNetworkStatusChanged;
            Application.Context.RegisterReceiver(_broadcastReceiver,
            new IntentFilter(ConnectivityManager.ConnectivityAction));
            ssccRow = FindViewById<LinearLayout>(Resource.Id.sscc_row);
            serialRow = FindViewById<LinearLayout>(Resource.Id.serial_row);

            // Method calls

            CheckIfApplicationStopingException();

            // Load the data for an order
            LoadRelatedOrder();         
            colorFields();
            SetUpForm();
                       
           
        }

        private void CheckIfApplicationStopingException()
        {
            if(CurrentFlow!=null&&!String.IsNullOrEmpty(CurrentFlow.GetString("CurrentFlow")) && moveHead !=null && openIdent!=null)
            {   
                // No error here, safe (ish) to continue
                return;
            } else
            {
                // Destroy the activity
                Finish();
                StartActivity(typeof(MainMenu));
            }
        }







     

        public bool IsOnline()
        {
            var cm = (ConnectivityManager) GetSystemService(ConnectivityService);
            return cm.ActiveNetworkInfo == null ? false : cm.ActiveNetworkInfo.IsConnected;
        }

        private void OnNetworkStatusChanged(object sender, EventArgs e)
        {
     
        }
        private void BtSaveOrUpdate_LongClick(object sender, View.LongClickEventArgs e)
        {
 
        }

        private void Button4_LongClick(object sender, View.LongClickEventArgs e)
        {

        }

        private void Button5_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(IssuedGoodsEnteredPositionsView));
            Finish();
        }

        private void LoadRelatedOrder()
        {
            if (openOrder != null)
            {
                isOpened = false;           
            } else
            {
                isOpened = true;
            }
        }



        private void Button7_Click(object sender, EventArgs e)
        {     
            StartActivity(typeof(IssuedGoodsEnteredPositionsView));
            Finish();
        }

        



        private async Task FinishMethod()
        {
            await Task.Run(() =>
            {

            });
        }

      

     

       

        private async void Button6_Click(object sender, EventArgs e)
        {
            popupDialogConfirm = new Dialog(this);
            popupDialogConfirm.SetContentView(Resource.Layout.Confirmation);
            popupDialogConfirm.Window.SetSoftInputMode(SoftInput.AdjustResize);
            popupDialogConfirm.Show();
            popupDialogConfirm.Window.SetLayout(LayoutParams.MatchParent, LayoutParams.WrapContent);
            popupDialogConfirm.Window.SetBackgroundDrawableResource(Android.Resource.Color.HoloRedLight);
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


        private void SetUpForm()
        {
            tbSSCC.Enabled = openIdent.GetBool("isSSCC");
            tbSerialNum.Enabled = openIdent.GetBool("HasSerialNumber");
            if (moveItem != null)
            {
                tbIdent.Text = moveItem.GetString("IdentName");
                tbSerialNum.Text = moveItem.GetString("SerialNo");
                tbSSCC.Text = moveItem.GetString("SSCC");
                tbLocation.Text = moveItem.GetString("Location");
                tbPalette.Text = moveItem.GetString("Palette");
                tbPacking.Text = moveItem.GetDouble("Packing").ToString();
                tbUnits.Text = moveItem.GetDouble("Factor").ToString();
                btCreateSame.Text = "Serij. - F2";
            }
            isPackaging = openIdent.GetBool("IsPackaging");

            if (isPackaging)
            {
                ssccRow.Visibility = ViewStates.Gone;
                serialRow.Visibility = ViewStates.Gone;
            }
            if (CommonData.GetSetting("ShowPaletteField") == "1")
            {
                lbPalette.Visibility = ViewStates.Visible;
                tbPalette.Visibility = ViewStates.Visible;
            }
            if (string.IsNullOrEmpty(tbUnits.Text.Trim())) { tbUnits.Text = "1"; }
            if (CommonData.GetSetting("ShowNumberOfUnitsField") == "1")
            {
                lbUnits.Visibility = ViewStates.Visible;
                tbUnits.Visibility = ViewStates.Visible;
            }
        }
     

        private static bool? checkIssuedOpenQty = null;
        private ProgressDialogClass progress;
        private Dialog popupDialogMain;
        private Button btConfirm;
        private EditText tbSSCCpopup;
        private ListView lvCardMore;
        private MorePalletsAdapter adapter;
        private Dialog popupDialog;
        private Button btnYes;
        private Button btnNo;
        private bool isFirst;
        private bool isMorePalletsMode = false;
        private bool isBatch;
        private int check;
        private bool isOkayToCallBarcode;
        private MorePalletsAdapter adapterNew;
        private NameValueObject moveItemNew;
        private Dialog popupDialogConfirm;
        private Button btnYesConfirm;
        private Button btnNoConfirm;
        private CustomAutoCompleteAdapter<string> DataAdapter;
        private string qtyStock;
        private MorePallets existsDuplicate;
        private string error;
        private string query;
        private ApiResultSet result;
        private NameValueObject dataObject;
        private string ident;
        private string sscc;
        private string warehouse;
        private Dialog popupDialogMainIssueing;

 

        private void colorFields()
        {
            tbSSCC.SetBackgroundColor(Android.Graphics.Color.Aqua);
            tbSerialNum.SetBackgroundColor(Android.Graphics.Color.Aqua);
            tbLocation.SetBackgroundColor(Android.Graphics.Color.Aqua);            
        }
        private void Button3_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(IssuedGoodsIdentEntryWithTrail));
            Finish();
        }

        public void GetBarcode(string barcode)
        {
            try
            {
                if (tbSSCC.HasFocus && isOkayToCallBarcode == false)
                {
                    if (barcode != "Scan fail")
                    {
                        Sound();

                    }
                }
                else if (tbSerialNum.HasFocus && isOkayToCallBarcode == false)
                {
                    if (barcode != "Scan fail")
                    {
                        Sound();

                    }
                }
                else if (tbLocation.HasFocus && isOkayToCallBarcode == false)
                {
                    if (barcode != "Scan fail")
                    {
                        Sound();
                      
                    }
                }
          
            } catch (Exception ex)
            {
                Crashes.TrackError(ex);
                Toast.MakeText(this, "Prišlo je do napake", ToastLength.Long).Show();
            }
        }




        private void Sound()
        {
            soundPool.Play(soundPoolId, 1, 1, 0, 0, 1);
        }
    }
}