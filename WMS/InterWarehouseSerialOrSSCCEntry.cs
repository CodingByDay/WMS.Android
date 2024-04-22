using Stream = Android.Media.Stream;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics.Drawables;
using Android.Media;
using Android.Net;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using BarCode2D_Receiver;
using Com.Barcode;
using Microsoft.AppCenter.Crashes;
using Newtonsoft.Json;

using WMS.App;
using TrendNET.WMS.Core.Data;
using TrendNET.WMS.Device.App;
using TrendNET.WMS.Device.Services;
using static Android.App.ActionBar;
using static Android.App.DownloadManager;
using WebApp = TrendNET.WMS.Device.Services.WebApp;

using AndroidX.AppCompat.App;
using AlertDialog = Android.App.AlertDialog;
using Android.Graphics;

namespace WMS
{
    [Activity(Label = "InterWarehouseSerialOrSSCCEntry", ScreenOrientation = ScreenOrientation.Portrait)]
    public class InterWarehouseSerialOrSSCCEntry : CustomBaseActivity, IBarcodeResult
    {
        private EditText? tbIdent;
        private EditText? tbSSCC;
        private EditText? tbSerialNum;
        private EditText? tbIssueLocation;
        private EditText? tbLocation;
        private EditText? tbPacking;
        private TextView? lbQty;
        private ImageView? imagePNG;
        private EditText? lbIdentName;
        private Button? btSaveOrUpdate;
        private Button? btCreate;
        private Button? btFinish;
        private Button? btOverview;
        private Button? btExit;
        private SoundPool soundPool;
        private int soundPoolId;
        private LinearLayout ssccRow;
        private LinearLayout serialRow;
        private NameValueObject openIdent = (NameValueObject)InUseObjects.Get("OpenIdent");
        private NameValueObject openOrder = (NameValueObject)InUseObjects.Get("OpenOrder");
        private NameValueObject moveHead = (NameValueObject)InUseObjects.Get("MoveHead");
        private NameValueObject moveItem = (NameValueObject)InUseObjects.Get("MoveItem");
        
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetTheme(Resource.Style.AppTheme_NoActionBar);
            SetContentView(Resource.Layout.InterWarehouseSerialOrSSCCEntry);
            AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
            var _customToolbar = new CustomToolbar(this, toolbar, Resource.Id.navIcon);
            _customToolbar.SetNavigationIcon(settings.RootURL + "/Services/Logo");
            SetSupportActionBar(_customToolbar._toolbar);
            SupportActionBar.SetDisplayShowTitleEnabled(false);
            Window.SetSoftInputMode(Android.Views.SoftInput.AdjustResize);

            btSaveOrUpdate = FindViewById<Button>(Resource.Id.btSaveOrUpdate);
            btCreate = FindViewById<Button>(Resource.Id.btCreate);
            btFinish = FindViewById<Button>(Resource.Id.btFinish);
            btOverview = FindViewById<Button>(Resource.Id.btOverview);
            btExit = FindViewById<Button>(Resource.Id.btExit);

            btSaveOrUpdate.Click += BtSaveOrUpdate_Click;
            btCreate.Click += BtCreate_Click;
            btFinish.Click += BtFinish_Click;
            btOverview.Click += BtOverview_Click;
            btExit.Click += BtExit_Click;


            tbIdent = FindViewById<EditText>(Resource.Id.tbIdent);
            tbSSCC = FindViewById<EditText>(Resource.Id.tbSSCC);
            tbSerialNum = FindViewById<EditText>(Resource.Id.tbSerialNum);
            tbIssueLocation = FindViewById<EditText>(Resource.Id.tbIssueLocation);
            tbLocation = FindViewById<EditText>(Resource.Id.tbLocation);
            tbPacking = FindViewById<EditText>(Resource.Id.tbPacking);
            tbSSCC.InputType = Android.Text.InputTypes.ClassNumber;
            lbQty = FindViewById<TextView>(Resource.Id.lbQty);
            imagePNG = FindViewById<ImageView>(Resource.Id.imagePNG);
            lbIdentName = FindViewById<EditText>(Resource.Id.lbIdentName);
            lbIdentName = FindViewById<EditText>(Resource.Id.lbIdentName);
            ssccRow = FindViewById<LinearLayout>(Resource.Id.sscc_row);
            serialRow = FindViewById<LinearLayout>(Resource.Id.serial_row);
            soundPool = new SoundPool(10, Android.Media.Stream.Music, 0);
            soundPoolId = soundPool.Load(this, Resource.Raw.beep, 1);
            Barcode2D barcode2D = new Barcode2D();
            barcode2D.open(this, this);
            tbLocation.FocusChange += TbLocation_FocusChange;
            var _broadcastReceiver = new NetworkStatusBroadcastReceiver();
            _broadcastReceiver.ConnectionStatusChanged += OnNetworkStatusChanged;
            Application.Context.RegisterReceiver(_broadcastReceiver,
            new IntentFilter(ConnectivityManager.ConnectivityAction));

            // Method calls

            CheckIfApplicationStopingException();

            // Color the fields that can be scanned
            ColorFields();

            SetUpProcessDependentButtons();

            // Main logic for the entry
            SetUpForm();
        }

        private void BtExit_Click(object? sender, EventArgs e)
        {
            
        }

        private void BtOverview_Click(object? sender, EventArgs e)
        {
            
        }

        private void BtFinish_Click(object? sender, EventArgs e)
        {
            
        }

        private void BtCreate_Click(object? sender, EventArgs e)
        {
            
        }

        private void BtSaveOrUpdate_Click(object? sender, EventArgs e)
        {
            
        }

        private void CheckIfApplicationStopingException()
        {
            if(moveItem == null && moveHead == null)
            {
                // Destroy the activity
                Finish();
                StartActivity(typeof(MainMenu));
            }
        }


        private void SetUpForm()
        {
            // This is the default focus of the view.
            tbSSCC.RequestFocus();

            if (Base.Store.isUpdate)
            {

            }
            else
            {
               
                

            }


        }


        private void ColorFields()
        {
            tbIdent.SetBackgroundColor(Android.Graphics.Color.Aqua);
            tbSSCC.SetBackgroundColor(Android.Graphics.Color.Aqua);
            tbSerialNum.SetBackgroundColor(Android.Graphics.Color.Aqua);
            tbLocation.SetBackgroundColor(Android.Graphics.Color.Aqua);
            tbIssueLocation.SetBackgroundColor(Android.Graphics.Color.Aqua);
        }
        private void SetUpProcessDependentButtons()
        {
            // This method changes the UI so it shows in a visible way that it is the update screen. - 18.03.2024
            if (Base.Store.isUpdate)
            {
                btSaveOrUpdate.Visibility = ViewStates.Gone;
                btCreate.Text = $"{Resources.GetString(Resource.String.s290)}";
            }

        }

        private void TbLocation_FocusChange(object? sender, View.FocusChangeEventArgs e)
        {
            
        }

        private void OnNetworkStatusChanged(object? sender, EventArgs e)
        {
            
        }

        public void GetBarcode(string barcode)
        {
            
        }
    }
}