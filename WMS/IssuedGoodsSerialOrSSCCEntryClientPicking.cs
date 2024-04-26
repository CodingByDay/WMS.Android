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
using static Android.App.DownloadManager;
using static Android.Graphics.Paint;
using static Android.Icu.Text.Transliterator;
using WebApp = TrendNET.WMS.Device.Services.WebApp;

using AndroidX.AppCompat.App;
using AlertDialog = Android.App.AlertDialog;
using Android.Graphics.Drawables;
using System.Data.Common;
namespace WMS
{
    [Activity(Label = "IssuedGoodsSerialOrSSCCEntryClientPicking", ScreenOrientation = ScreenOrientation.Portrait)]
    public class IssuedGoodsSerialOrSSCCEntryClientPicking : CustomBaseActivity, IBarcodeResult
    {

        private EditText tbIdent;
        private EditText tbSSCC;
        private EditText tbSerialNum;
        private EditText tbLocation;
        private EditText tbPacking;

        private NameValueObject openIdent = (NameValueObject)InUseObjects.Get("OpenIdent");
        private NameValueObject openOrder = (NameValueObject)InUseObjects.Get("OpenOrder");
        private ApiResultSet OpenOrderItem = (ApiResultSet)InUseObjects.Get("OpenOrderItem");
        private NameValueObject moveHead = (NameValueObject)InUseObjects.Get("MoveHead");
        private NameValueObject moveItem = (NameValueObject)InUseObjects.Get("MoveItem");
        private NameValueObject extraData = (NameValueObject)InUseObjects.Get("ExtraData");
        private NameValueObject lastItem = (NameValueObject)InUseObjects.Get("LastItem");

        private Button btCreateSame;
        private Button btCreate;
        private Button btFinish;
        private Button btOverview;
        private Button btExit;

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
        private List<IssuedGoods> dist;
        private List<LocationClass> items = new List<LocationClass>();
        private TextView lbQty;
        private bool isPackaging = false;

        private SoundPool soundPool;
        private int soundPoolId;
        private bool isOpened = false;
        private ClientPickingPosition receivedTrail;
        private List<string> locations = new List<string>();
        private double qtyCheck = 0;
        private LinearLayout ssccRow;
        private LinearLayout serialRow;
        private List<IssuedGoods> connectedPositions = new List<IssuedGoods>();
        private bool createPositionAllowed = false;
        private double stock;
        private ListView listData;
        private UniversalAdapter<LocationClass> dataAdapter;
        private AdapterLocation lcAdapter;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetTheme(Resource.Style.AppTheme_NoActionBar);
            // Start the loader
            LoaderManifest.LoaderManifestLoopResources(this);
            if (settings.tablet)
            {
                RequestedOrientation = ScreenOrientation.Landscape;
                SetContentView(Resource.Layout.IssuedGoodsSerialOrSSCCEntryClientPickingTablet);
                listData = FindViewById<ListView>(Resource.Id.listData);
                dataAdapter = UniversalAdapterHelper.GetIssuedGoodsSerialOrSSCCEntryClientPicking(this, items);
                listData.Adapter = dataAdapter;

            }
            else
            {
                RequestedOrientation = ScreenOrientation.Portrait;
                SetContentView(Resource.Layout.IssuedGoodsSerialOrSSCCEntryClientPicking);
            }
            // Definitions
            AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
            var _customToolbar = new CustomToolbar(this, toolbar, Resource.Id.navIcon);
            _customToolbar.SetNavigationIcon(settings.RootURL + "/Services/Logo");
            SetSupportActionBar(_customToolbar._toolbar);
            SupportActionBar.SetDisplayShowTitleEnabled(false);
            Window.SetSoftInputMode(Android.Views.SoftInput.AdjustResize);
            tbIdent = FindViewById<EditText>(Resource.Id.tbIdent);
            tbSSCC = FindViewById<EditText>(Resource.Id.tbSSCC);
            tbSerialNum = FindViewById<EditText>(Resource.Id.tbSerialNum);
            tbLocation = FindViewById<EditText>(Resource.Id.tbLocation);
            tbPacking = FindViewById<EditText>(Resource.Id.tbPacking);
            tbIdent.InputType = Android.Text.InputTypes.ClassNumber;
            tbSSCC.InputType = Android.Text.InputTypes.ClassNumber;
            tbLocation.InputType = Android.Text.InputTypes.ClassText;
            lbQty = FindViewById<TextView>(Resource.Id.lbQty);
            soundPool = new SoundPool(10, Stream.Music, 0);
            soundPoolId = soundPool.Load(this, Resource.Raw.beep, 1);
            Barcode2D barcode2D = new Barcode2D();
            barcode2D.open(this, this);
            btCreateSame = FindViewById<Button>(Resource.Id.btCreateSame);
            btCreate = FindViewById<Button>(Resource.Id.btCreate);
            btFinish = FindViewById<Button>(Resource.Id.btFinish);
            btOverview = FindViewById<Button>(Resource.Id.btOverview);
            btExit = FindViewById<Button>(Resource.Id.btExit);
            // Events
            tbPacking.KeyPress += TbPacking_KeyPress;
            tbSSCC.KeyPress += TbSSCC_KeyPress;
            tbSerialNum.KeyPress += TbSerialNum_KeyPress;
            btCreateSame.Click += BtCreateSame_Click;
            btCreate.Click += BtCreate_Click;
            btFinish.Click += BtFinish_Click;
            btExit.Click += BtExit_Click;
            btOverview.Click += BtOverview_Click;
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


            if (settings.tablet)
            {
                fillItems();
            }
        }

        private async void fillItems()
        {
            var code = openIdent.GetString("Code");
            var wh = moveHead.GetString("Wharehouse");
            items = await AdapterStore.getStockForWarehouseAndIdent(code, wh);
        }

        private void BtOverview_Click(object? sender, EventArgs e)
        {
            StartActivity(typeof(IssuedGoodsEnteredPositionsView));
            Finish();
        }




        private void TbPacking_KeyPress(object? sender, View.KeyEventArgs e)
        {
            if (e.KeyCode == Keycode.Enter && e.Event.Action == KeyEventActions.Down)
            {
                FilterData();
            }
            e.Handled = false;

        }

        private void BtFinish_Click(object? sender, EventArgs e)
        {
            popupDialogConfirm = new Dialog(this);
            popupDialogConfirm.SetContentView(Resource.Layout.Confirmation);
            popupDialogConfirm.Window.SetSoftInputMode(SoftInput.AdjustResize);
            popupDialogConfirm.Show();
            popupDialogConfirm.Window.SetLayout(LayoutParams.MatchParent, LayoutParams.WrapContent);
            popupDialogConfirm.Window.SetBackgroundDrawable(new ColorDrawable(Color.ParseColor("#081a45")));
            btnYesConfirm = popupDialogConfirm.FindViewById<Button>(Resource.Id.btnYes);
            btnNoConfirm = popupDialogConfirm.FindViewById<Button>(Resource.Id.btnNo);
            btnYesConfirm.Click += BtnYesConfirm_Click;
            btnNoConfirm.Click += BtnNoConfirm_Click;
        }

        private async void BtCreate_Click(object? sender, EventArgs e)
        {
            double parsed;

            if (createPositionAllowed && double.TryParse(tbPacking.Text, out parsed) && stock >= parsed)
            {
                await CreateMethodFromStart();
            }
            else
            {
                Toast.MakeText(this, $"{Resources.GetString(Resource.String.s270)}", ToastLength.Long).Show();
            }
        }

        private async void BtCreateSame_Click(object? sender, EventArgs e)
        {
            double parsed;
            if (createPositionAllowed && double.TryParse(tbPacking.Text, out parsed) && stock >= parsed)
            {
                await CreateMethodSame();
            }
            else
            {
                Toast.MakeText(this, $"{Resources.GetString(Resource.String.s270)}", ToastLength.Long).Show();
            }
        }

        private void TbSerialNum_KeyPress(object? sender, View.KeyEventArgs e)
        {
            if (e.KeyCode == Keycode.Enter && e.Event.Action == KeyEventActions.Down)
            {
                FilterData();
            }

            e.Handled = false;
        }

        private void TbSSCC_KeyPress(object? sender, View.KeyEventArgs e)
        {
            if (e.KeyCode == Keycode.Enter && e.Event.Action == KeyEventActions.Down)
            {
                FilterData();
            }

            e.Handled = false;
        }

        private void BtExit_Click(object? sender, EventArgs e)
        {
            StartActivity(typeof(MainMenu));
        }

        private void CheckIfApplicationStopingException()
        {
            if (moveHead != null && openIdent != null)
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







        private async Task CreateMethodFromStart()
        {
            await Task.Run(() =>
            {
                if (dist.Count == 1)
                {

                    if (moveItem == null)
                    {
                        moveItem = new NameValueObject("MoveItem");
                    }

                    moveItem.SetInt("HeadID", moveHead.GetInt("HeadID"));
                    moveItem.SetString("LinkKey", receivedTrail.Order);
                    moveItem.SetInt("LinkNo", receivedTrail.No);
                    moveItem.SetString("Ident", openIdent.GetString("Code"));
                    moveItem.SetString("SSCC", tbSSCC.Text.Trim());
                    moveItem.SetString("SerialNo", tbSerialNum.Text.Trim());
                    moveItem.SetDouble("Packing", Convert.ToDouble(tbPacking.Text.Trim()));
                    moveItem.SetDouble("Factor", 1);
                    moveItem.SetDouble("Qty", Convert.ToDouble(tbPacking.Text.Trim()));
                    moveItem.SetInt("Clerk", Services.UserID());
                    moveItem.SetString("Location", tbLocation.Text.Trim());
                    moveItem.SetString("Palette", "1");

                    string error;
                    moveItem = Services.SetObject("mi", moveItem, out error);

                    if (moveItem != null && error == string.Empty)
                    {
                        RunOnUiThread(() =>
                        {
                            if (Base.Store.modeIssuing == 2)
                            {
                                StartActivity(typeof(IssuedGoodsIdentEntryWithTrail));
                                Finish();
                            }
                        });


                        dist = new List<IssuedGoods>();
                        createPositionAllowed = false;
                        GetConnectedPositions(receivedTrail.Order, receivedTrail.No, receivedTrail.Ident, receivedTrail.Location);
                    }

                }
                else
                {
                    return;
                }
            });
        }







        private async Task CreateMethodSame()
        {
            await Task.Run(() =>
            {
                if (dist.Count == 1)
                {

                    if (moveItem == null)
                    {
                        moveItem = new NameValueObject("MoveItem");
                    }

                    moveItem.SetInt("HeadID", moveHead.GetInt("HeadID"));
                    moveItem.SetString("LinkKey", receivedTrail.Order);
                    moveItem.SetInt("LinkNo", receivedTrail.No);
                    moveItem.SetString("Ident", openIdent.GetString("Code"));
                    moveItem.SetString("SSCC", tbSSCC.Text.Trim());
                    moveItem.SetString("SerialNo", tbSerialNum.Text.Trim());
                    moveItem.SetDouble("Packing", Convert.ToDouble(tbPacking.Text.Trim()));
                    moveItem.SetDouble("Factor", 1);
                    moveItem.SetDouble("Qty", Convert.ToDouble(tbPacking.Text.Trim()));
                    moveItem.SetInt("Clerk", Services.UserID());
                    moveItem.SetString("Location", tbLocation.Text.Trim());
                    moveItem.SetString("Palette", "1");

                    string error;
                    moveItem = Services.SetObject("mi", moveItem, out error);

                    if (moveItem != null && error == string.Empty)
                    {
                        RunOnUiThread(() =>
                        {
                            // Succesfull position creation
                            if (ssccRow.Visibility == ViewStates.Visible)
                            {
                                tbSSCC.Text = string.Empty;
                                tbSSCC.RequestFocus();
                            }
                            if (serialRow.Visibility == ViewStates.Visible)
                            {
                                tbSerialNum.Text = string.Empty;

                                if (ssccRow.Visibility == ViewStates.Gone)
                                {
                                    tbSerialNum.RequestFocus();
                                }
                            }
                            Toast.MakeText(this, "Pozicija kreirana", ToastLength.Long);
                        });

                        dist = new List<IssuedGoods>();
                        createPositionAllowed = false;
                        GetConnectedPositions(receivedTrail.Order, receivedTrail.No, receivedTrail.Ident, receivedTrail.Location);
                    }

                }
                else
                {
                    return;
                }
            });
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

                    string result;

                    if (WebApp.Get("mode=finish&stock=remove&print=" + Services.DeviceUser() + "&id=" + headID.ToString(), out result))
                    {
                        if (result.StartsWith("OK!"))
                        {

                            RunOnUiThread(() =>
                            {
                                progress.StopDialogSync();

                                var id = result.Split('+')[1];

                                Toast.MakeText(this, $"{Resources.GetString(Resource.String.s264)}" + id, ToastLength.Long).Show();

                                AlertDialog.Builder alert = new AlertDialog.Builder(this);

                                alert.SetTitle($"{Resources.GetString(Resource.String.s263)}");

                                alert.SetMessage($"{Resources.GetString(Resource.String.s264)}" + id);

                                alert.SetPositiveButton("Ok", (senderAlert, args) =>
                                {
                                    alert.Dispose();
                                    System.Threading.Thread.Sleep(500);
                                    StartActivity(typeof(IssuedGoodsBusinessEventSetupClientPicking));
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
                        Toast.MakeText(this, $"{Resources.GetString(Resource.String.s216)}" + result, ToastLength.Long).Show();

                    }
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




        private void SetUpForm()
        {


            // This is the default focus of the view.
            tbSSCC.RequestFocus();

            if (!openIdent.GetBool("isSSCC"))
            {
                ssccRow.Visibility = ViewStates.Gone;
                tbSerialNum.RequestFocus();
            }

            if (!openIdent.GetBool("HasSerialNumber"))
            {
                serialRow.Visibility = ViewStates.Gone;
                tbPacking.RequestFocus();
            }

            if (Base.Store.isUpdate && moveItem != null)
            {
                // Update logic ?? it seems to be true.
                tbIdent.Text = moveItem.GetString("IdentName");
                tbSerialNum.Text = moveItem.GetString("SerialNo");
                tbSSCC.Text = moveItem.GetString("SSCC");
                tbLocation.Text = moveItem.GetString("Location");
                tbPacking.Text = moveItem.GetDouble("Packing").ToString();
                btCreateSame.Text = $"{Resources.GetString(Resource.String.s293)}";
            }
            else
            {
                // Not the update ?? it seems to be true
                tbIdent.Text = openIdent.GetString("Code") + " " + openIdent.GetString("Name");

                if (Intent.Extras != null && Intent.GetByteArrayExtra("selected")!=null)
                {
                    byte[] trailBytes = Intent.GetByteArrayExtra("selected");
                    receivedTrail = ClientPickingPosition.Deserialize<ClientPickingPosition>(trailBytes);
                    qtyCheck = Double.Parse(receivedTrail.Quantity);
                    tbLocation.Text = receivedTrail.Location;
                    lbQty.Text = $"{Resources.GetString(Resource.String.s83)} ( " + qtyCheck.ToString(CommonData.GetQtyPicture()) + " )";
                    stock = qtyCheck;
                    tbPacking.Text = qtyCheck.ToString();
                    GetConnectedPositions(receivedTrail.Order, receivedTrail.No, receivedTrail.Ident, receivedTrail.Location);
                }
            }

            isPackaging = openIdent.GetBool("IsPackaging");

            if (isPackaging)
            {
                ssccRow.Visibility = ViewStates.Gone;
                serialRow.Visibility = ViewStates.Gone;
            }

   


        }


        /// <summary>
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
        private async void GetConnectedPositions(string acKey, int anNo, string acIdent, string acLocation)
        {
            var parameters = new List<Services.Parameter>();

            parameters.Add(new Services.Parameter { Name = "acKey", Type = "String", Value = acKey });
            parameters.Add(new Services.Parameter { Name = "anNo", Type = "Int32", Value = anNo });
            parameters.Add(new Services.Parameter { Name = "acIdent", Type = "String", Value = acIdent });
            parameters.Add(new Services.Parameter { Name = "acLocation", Type = "String", Value = acLocation });

            var subjects = await AsyncServices.AsyncServices.GetObjectListBySqlAsync($"SELECT * from uWMSOrderItemByKeyOut WHERE acKey = @acKey AND anNo = @anNo AND acIdent = @acIdent and acLocation=@acLocation;", parameters);

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
                if (subjects.Rows.Count > 0)
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
                            anQty = row.DoubleValue("anQty"),
                            aclocation = row.StringValue("aclocation")
                        });
                    }
                }
            }
        }

        public static List<IssuedGoods> FilterIssuedGoods(List<IssuedGoods> issuedGoodsList, string acSSCC = null, string acSerialNo = null, string acLocation = null)
        {
            var filtered = issuedGoodsList;

            if (!String.IsNullOrEmpty(acSSCC))
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



        private void ColorFields()
        {
            tbSSCC.SetBackgroundColor(Android.Graphics.Color.Aqua);
            tbSerialNum.SetBackgroundColor(Android.Graphics.Color.Aqua);
            tbLocation.SetBackgroundColor(Android.Graphics.Color.Aqua);
        }

        public void GetBarcode(string barcode)
        {
            try
            {
                if (tbSSCC.HasFocus)
                {
                    if (barcode != "Scan fail")
                    {
                        Sound();

                        tbSSCC.Text = barcode;

                        if (serialRow.Visibility == ViewStates.Visible)
                        {
                            tbSerialNum.RequestFocus();
                        }
                        else
                        {
                            tbPacking.RequestFocus();
                        }


                        FilterData();
                    }
                }
                else if (tbSerialNum.HasFocus)
                {
                    if (barcode != "Scan fail")
                    {
                        Sound();

                        tbSerialNum.Text = barcode;

                        tbPacking.RequestFocus();


                        FilterData();

                    }
                }
                else if (tbLocation.HasFocus)
                {
                    if (barcode != "Scan fail")
                    {
                        Sound();

                        tbLocation.Text = barcode;


                        FilterData();

                    }
                }
            }
            catch (Exception ex)
            {
                Crashes.TrackError(ex);
                Toast.MakeText(this, $"{Resources.GetString(Resource.String.s225)}", ToastLength.Long).Show();
            }
        }


        private void FilterData()
        {
            var data = FilterIssuedGoods(connectedPositions, tbSSCC.Text, tbSerialNum.Text, tbLocation.Text);

            // Temporary solution because of the SQL error.
            dist = data
                .GroupBy(x => new { x.acName, x.acSSCC, x.acSerialNo, x.aclocation, x.acSubject, x.anQty })
                .Select(g => g.First())
                .ToList();

            if (dist.Count == 1)
            {
                // Do stuff and allow creating the position
                createPositionAllowed = true;
                tbPacking.Text = dist.ElementAt(0).anQty.ToString();
            }

        }


        private void Sound()
        {
            soundPool.Play(soundPoolId, 1, 1, 0, 0, 1);
        }
    }
}