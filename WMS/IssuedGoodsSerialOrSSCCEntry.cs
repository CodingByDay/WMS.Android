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

using AndroidX.AppCompat.App;
using AlertDialog = Android.App.AlertDialog;
using Aspose.Words.Drawing;

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
        private SoundPool soundPool;
        private int soundPoolId;
        private bool isOpened = false;
        private Trail receivedTrail;
        private List<string> locations = new List<string>();
        private double qtyCheck = 0;
        private LinearLayout ssccRow;
        private LinearLayout serialRow;
        private List<IssuedGoods> connectedPositions = new List<IssuedGoods>();
        private bool createPositionAllowed = false;
        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetTheme(Resource.Style.AppTheme_NoActionBar);

            // Start the loader
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

            // Events

            tbSSCC.KeyPress += TbSSCC_KeyPress;
            tbSerialNum.KeyPress += TbSerialNum_KeyPress;

            btCreateSame = FindViewById<Button>(Resource.Id.btCreateSame);
            btCreate = FindViewById<Button>(Resource.Id.btCreate);
            btFinish = FindViewById<Button>(Resource.Id.btFinish);
            btOverview = FindViewById<Button>(Resource.Id.btOverview);
            btExit = FindViewById<Button>(Resource.Id.btExit);

            btExit.Click += BtExit_Click;

            var _broadcastReceiver = new NetworkStatusBroadcastReceiver();
            _broadcastReceiver.ConnectionStatusChanged += OnNetworkStatusChanged;
            Application.Context.RegisterReceiver(_broadcastReceiver,
            new IntentFilter(ConnectivityManager.ConnectivityAction));
            ssccRow = FindViewById<LinearLayout>(Resource.Id.sscc_row);
            serialRow = FindViewById<LinearLayout>(Resource.Id.serial_row);

            // Method calls

            CheckIfApplicationStopingException();

            // Color the fields that can be scanned
            ColorFields();

            // Main logic for the entry
            SetUpForm();

            // Stop the loader
            LoaderManifest.LoaderManifestLoopStop(this);
        }

        private void TbSerialNum_KeyPress(object? sender, View.KeyEventArgs e)
        {
            if(e.KeyCode == Keycode.Enter)
            {
                FilterData();
            }
            e.Handled = false;

        }

        private void TbSSCC_KeyPress(object? sender, View.KeyEventArgs e)
        {
            if (e.KeyCode == Keycode.Enter)
            {
                FilterData();
            }
        }

        private void BtExit_Click(object? sender, EventArgs e)
        {
            StartActivity(typeof(MainMenu));
        }

        private void CheckIfApplicationStopingException()
        {
            if (CurrentFlow != null && !String.IsNullOrEmpty(CurrentFlow.GetString("CurrentFlow")) && moveHead != null && openIdent != null)
            {
                // No error here, safe (ish) to continue
                return;
            }
            else
            {
                // Destroy the activity
                Finish();
                StartActivity(typeof(MainMenu));
            }
        }

        public bool IsOnline()
        {
            var cm = (ConnectivityManager)GetSystemService(ConnectivityService);
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

            // This is the default focus of the view.
            tbSSCC.RequestFocus();

            if(!openIdent.GetBool("isSSCC"))
            {
                ssccRow.Visibility = ViewStates.Gone;
                tbSerialNum.RequestFocus();
            }

            if(!openIdent.GetBool("HasSerialNumber"))
            {
                serialRow.Visibility = ViewStates.Gone;
                tbPacking.RequestFocus();
            }


            if (moveItem != null)
            {
                // Update logic ?? it seems to be true.
                tbIdent.Text = moveItem.GetString("IdentName");
                tbSerialNum.Text = moveItem.GetString("SerialNo");
                tbSSCC.Text = moveItem.GetString("SSCC");
                tbLocation.Text = moveItem.GetString("Location");
                tbPalette.Text = moveItem.GetString("Palette");
                tbPacking.Text = moveItem.GetDouble("Packing").ToString();
                tbUnits.Text = moveItem.GetDouble("Factor").ToString();
                btCreateSame.Text = "Serij. - F2";


            }
            else
            {
                // Not the update ?? it seems to be true
                tbIdent.Text = openIdent.GetString("Code") + " " + openIdent.GetString("Name");

                if (Intent.Extras != null && !String.IsNullOrEmpty(Intent.Extras.GetString("selected")))
                {
                    string trailBytes = Intent.Extras.GetString("selected");
                    Trail receivedTrail = JsonConvert.DeserializeObject<Trail>(trailBytes);
                    qtyCheck = Double.Parse(receivedTrail.Qty);
                    tbLocation.Text = receivedTrail.Location;
                    lbQty.Text = "Zaloga ( " + qtyCheck.ToString(CommonData.GetQtyPicture()) + " )";
                    tbPacking.Text = qtyCheck.ToString();
                    GetConnectedPositions(receivedTrail.Key, receivedTrail.No, receivedTrail.Ident, receivedTrail.Location);            
                }
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

            // Test this function based on the proccess
        }


        /// <summary>
        ///
        /// Podatke preneseš v masko - kličeš NE isti view ampak vedno "uWMSOrderItemByKeyOut", ker moraš
        /// tudi pri subjektih zapisati na katero naročilo z pozicijo(acKey in anNo) se vrši izdaja.
        /// uWMSOrderItemByKeyOut; vhodni parameter acKey varchar(13), anNo int, acIdent varchar(16), acLocation varchar(50);
        /// izhod: acName varchar(80), acSubject varchar(30), acSerialNo varchar(100), acSSCC varchar(18), anQty decimal (19,6)
        /// če je zapis 1 potem prikažeš tiste podatke in uporabnik le potrdi
        /// če je zapisov več si jih shraniš in z dodatnimi vpisi/skeniranji(SSCC ali serijska) "filtriraš" podatke, ko prideš na enega izpolniš vse podatke, uporabnik lahko spremeni količino - v oklepaju je že od vsega začetka vpisan anQty.
        /// če uporabnik klikne na gumb serijska, se iz seznama pobriše ta vrsitca in maska ostane kot je bila po koncu koraka 4.
        /// lahko pa enostavno ponoviš klic view-a ki bi že moral imeti zapisane podatke in osvežene, če ne bo kaj težav z asinhronimi klici...
        ///
        /// </summary>
        /// <param name="acKey">Številka naročila</param>
        /// <param name="anNo">Pozicija znotraj naročila</param>
        /// <param name="acIdent">Ident</param>
        private void GetConnectedPositions(string acKey, int anNo, string acIdent, string acLocation)
        {
            var parameters = new List<Services.Parameter>();

            parameters.Add(new Services.Parameter { Name = "acKey", Type = "String", Value = acKey });
            parameters.Add(new Services.Parameter { Name = "anNo", Type = "Int32", Value = anNo });
            parameters.Add(new Services.Parameter { Name = "acIdent", Type = "String", Value = acIdent });
            parameters.Add(new Services.Parameter { Name = "acLocation", Type = "String", Value = acLocation });

            var subjects = Services.GetObjectListBySql($"SELECT * from uWMSOrderItemByKeyOut WHERE acKey = @acKey AND anNo = @anNo AND acIdent = @acIdent and acLocation=@acLocation;", parameters);

            if (!subjects.Success)
            {
                RunOnUiThread(() =>
                {
                    Analytics.TrackEvent(subjects.Error);
                    return;
                });
            }
            else
            {
                if(subjects.Rows.Count > 0)
                {
                    for (int i = 0; i < subjects.Rows.Count; i++)
                    {
                        var row = subjects.Rows[i];
                        connectedPositions.Add(new IssuedGoods
                        {

                            acName = row.StringValue("acName"),
                            acSubject = row.StringValue("acSubject"),
                            acSerialNo = row.StringValue("acSerialNo"),
                            acSSCC = row.StringValue("acSSCC"),
                            anQty = row.DoubleValue("anQty")

                        });
                    }                                         
                }
            }
        }

        public static List<IssuedGoods> FilterIssuedGoods(List<IssuedGoods> issuedGoodsList, string acSSCC = null, string acSerialNo = null, string acLocation = null)
        {
            var filtered = issuedGoodsList;

            if(!String.IsNullOrEmpty(acSSCC))
            {
                filtered = filtered.Where(x => x.acSSCC == acSSCC).ToList();
            }

            if (!String.IsNullOrEmpty(acSerialNo))
            {
                filtered = filtered.Where(x => x.acSerialNo == acSerialNo).ToList();
            }

            if (!String.IsNullOrEmpty(acLocation))
            {
                filtered = filtered.Where(x => x.aclocation == acLocation).ToList();
            }

            return filtered;
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

        private void ColorFields()
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
            }
            catch (Exception ex)
            {
                Crashes.TrackError(ex);
                Toast.MakeText(this, "Prišlo je do napake", ToastLength.Long).Show();
            }
        }


        private void FilterData()
        {
            var data = FilterIssuedGoods(connectedPositions, tbSSCC.Text, tbSerialNum.Text, tbLocation.Text);

            if(data.Count == 1)
            {
                // Do stuff and allow creating the position
                createPositionAllowed = true;
            } 
        }


        private void Sound()
        {
            soundPool.Play(soundPoolId, 1, 1, 0, 0, 1);
        }
    }
}