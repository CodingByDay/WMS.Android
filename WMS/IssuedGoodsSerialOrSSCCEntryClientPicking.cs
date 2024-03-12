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
namespace WMS
{
    [Activity(Label = "IssuedGoodsSerialOrSSCCEntryClientPicking", ScreenOrientation = ScreenOrientation.Portrait)]
    public class IssuedGoodsSerialOrSSCCEntryClientPicking : AppCompatActivity, IBarcodeResult
    {

        private EditText tbIdent;
        private EditText tbSSCC;
        private EditText tbSerialNum;
        private EditText tbLocation;
        private EditText tbPacking;
        private EditText tbUnits;
        private EditText tbPalette;
        private Button button1;
        private Button btSaveOrUpdate;
        private Button button4;
        private Button button6;
        private Button button5;
        private Button button7;
        private NameValueObject openIdent = (NameValueObject)InUseObjects.Get("OpenIdent");
        private NameValueObject openOrder = (NameValueObject)InUseObjects.Get("OpenOrder");
        private ApiResultSet OpenOrderItem = (ApiResultSet)InUseObjects.Get("OpenOrderItem");
        private bool isSkipable = false;
        private NameValueObject moveHead = (NameValueObject)InUseObjects.Get("MoveHead");
        private NameValueObject moveItem = (NameValueObject)InUseObjects.Get("MoveItem");
        private NameValueObject CurrentFlow = (NameValueObject)InUseObjects.Get("CurrentClientFlow");
        private NameValueObject extraData = (NameValueObject)InUseObjects.Get("ExtraData");
        private NameValueObject lastItem = (NameValueObject)InUseObjects.Get("LastItem");
        private List<MorePallets> data = new List<MorePallets>();
        private bool enabledSerial;
        private NameValueObjectList docTypes = null;
        private NameValueObject stock = null;
        private TextView lbQty;
        private bool editMode = false;
        private bool isPackaging = false;
        private TextView lbUnits;
        private TextView lbPalette;
        SoundPool soundPool;
        int soundPoolId;
        private bool isOpened = false;

        private ClientPickingPosition receivedTrail;
        private List<string> locations = new List<string>();
        double qtyCheck = 0;
        private Button btMorePallets;
        protected async override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetTheme(Resource.Style.AppTheme_NoActionBar);
            SetContentView(Resource.Layout.IssuedGoodsSerialOrSSCCEntryClientPicking);
            var s = CurrentFlow;
            AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
            var _customToolbar = new CustomToolbar(this, toolbar, Resource.Id.navIcon);
            _customToolbar.SetNavigationIcon(settings.RootURL + "/Services/Logo");
            SetSupportActionBar(_customToolbar._toolbar);
            SupportActionBar.SetDisplayShowTitleEnabled(false);
            if (CurrentFlow != null)
            {
                if (String.IsNullOrEmpty(CurrentFlow.GetString("CurrentFlow")))
                {
                    AlertDialog.Builder alert = new AlertDialog.Builder(this);
                    alert.SetTitle("Napaka na aplikaciji");
                    alert.SetMessage("Prišlo je do napake.");
                    alert.SetPositiveButton("Ok", (senderAlert, args) =>
                    {
                        StartActivity(typeof(MainMenu));
                    });
                    Dialog dialog = alert.Create();
                    dialog.Show();
                }
            }
     
            tbIdent = FindViewById<EditText>(Resource.Id.tbIdent);
            tbSSCC = FindViewById<EditText>(Resource.Id.tbSSCC);
            tbSerialNum = FindViewById<EditText>(Resource.Id.tbSerialNum);
            tbLocation = FindViewById<EditText>(Resource.Id.tbLocation);
            tbPacking = FindViewById<EditText>(Resource.Id.tbPacking);
            tbUnits = FindViewById<EditText>(Resource.Id.tbUnits);
            tbPalette = FindViewById<EditText>(Resource.Id.tbPalette);
            tbIdent.InputType = Android.Text.InputTypes.ClassNumber;
            tbSSCC.InputType = Android.Text.InputTypes.ClassNumber;
            tbLocation.InputType = Android.Text.InputTypes.ClassNumber;
            tbUnits.InputType = Android.Text.InputTypes.ClassNumber;
            tbPalette.InputType = Android.Text.InputTypes.ClassNumber;
            button1 = FindViewById<Button>(Resource.Id.button1);
            btSaveOrUpdate = FindViewById<Button>(Resource.Id.btSaveOrUpdate);
            button4 = FindViewById<Button>(Resource.Id.button4);
            button6 = FindViewById<Button>(Resource.Id.button6);
            button5 = FindViewById<Button>(Resource.Id.button5);
            button7 = FindViewById<Button>(Resource.Id.button7);
            lbQty = FindViewById<TextView>(Resource.Id.lbQty);
            lbUnits = FindViewById<TextView>(Resource.Id.lbUnits);
            lbPalette = FindViewById<TextView>(Resource.Id.lbPalette);
            soundPool = new SoundPool(10, Stream.Music, 0);
            soundPoolId = soundPool.Load(this, Resource.Raw.beep, 1);
            Barcode2D barcode2D = new Barcode2D();
            barcode2D.open(this, this);
            button1.Click += Button1_Click;
            btSaveOrUpdate.Click += BtSaveOrUpdate_Click;
            button4.Click += Button4_Click;
            button6.Click += Button6_Click;
            btMorePallets = FindViewById<Button>(Resource.Id.btMorePallets);
            btMorePallets.Click += BtMorePallets_Click;
            button7.Click += Button7_Click;  
            button5.Click += Button5_Click;
            colorFields();                
            SetUpForm();
            var r = openOrder;
            // tbLocation.KeyPress += TbLocation_KeyPress;
            button4.LongClick += Button4_LongClick;
            btSaveOrUpdate.LongClick += BtSaveOrUpdate_LongClick;
            var _broadcastReceiver = new NetworkStatusBroadcastReceiver();
            _broadcastReceiver.ConnectionStatusChanged += OnNetworkStatusChanged;
            Application.Context.RegisterReceiver(_broadcastReceiver,
            new IntentFilter(ConnectivityManager.ConnectivityAction));
            var oi = openIdent;
            var oo = openOrder;
         
            if (Intent.Extras != null && Intent.Extras.GetString("update") != "1")
            {
                try
                {
                    byte[] trailBytes = Intent.GetByteArrayExtra("selected");
                    // Deserialize the Trail object
                    receivedTrail = ClientPickingPosition.Deserialize<ClientPickingPosition>(trailBytes);
                    if (receivedTrail != null)
                    {
                        qtyCheck = Double.Parse(receivedTrail.Quantity);
                        string error;
                        openOrder = Services.GetObject("oobl", receivedTrail.Order + "|" + receivedTrail.No, out error);
                    }
                }
                catch (Exception ex) 
                {
                    Crashes.TrackError(ex);
                }
                var qty = Intent.Extras.GetString("qty");
                tbPacking.Text = qty;
                var serial = Intent.Extras.GetString("serial");
               
            } else
            {
                if(Intent.Extras.GetString("update") == "1")
                {
                    isUpdate = true;
                }
            }
           
            bool isSSCC = openIdent.GetBool("isSSCC");
            bool isSerialNumber = openIdent.GetBool("HasSerialNumber");

            if (!isSSCC)
            {
                btMorePallets.Visibility = ViewStates.Gone;
            }

            if (!isSerialNumber)
            {
                btSaveOrUpdate.Visibility = ViewStates.Gone;
            }

            tbPacking.FocusChange += TbPacking_FocusChange;
        }


        private void TbPacking_FocusChange(object sender, View.FocusChangeEventArgs e)
        {
            ProcessQty();
        }

        private void ProcessQty()
        {
   
            if (!CommonData.IsValidLocation(moveHead.GetString("Wharehouse"), tbLocation.Text.Trim()))
            {
                Toast.MakeText(this, "Lokacija '" + tbLocation.Text.Trim() + "' ni veljavna za skladišče '" + moveHead.GetString("Wharehouse") + "'!", ToastLength.Long).Show();
                tbLocation.RequestFocus();
                return;
            }

            if (!LoadStock(moveHead.GetString("Wharehouse"), tbLocation.Text.Trim(), tbSSCC.Text.Trim(), tbSerialNum.Text.Trim(), openIdent.GetString("Code")))
            {
                Toast.MakeText(this, "Zaloga za SSCC/Serijsko št. ne obstaja.", ToastLength.Long).Show();
                return;
            }
            else
            {
                var stock = LoadStockDouble(moveHead.GetString("Wharehouse"), tbLocation.Text.Trim(), tbSSCC.Text.Trim(), tbSerialNum.Text.Trim(), openIdent.GetString("Code"));


                qtyCheck = stock;

                if (receivedTrail != null)
                {
                    if (isUpdate)
                    {
                        stock = stock + Double.Parse(receivedTrail.Quantity);
                        qtyCheck = stock;
                    }
                    lbQty.Text = "Kol. (" + stock.ToString(CommonData.GetQtyPicture()) + ")";
                    tbPacking.Text = receivedTrail.Quantity.ToString(); 
                    
                } else if (moveItem != null)
                {
                    if (isUpdate)
                    {
                        stock = stock + moveItem.GetDouble("Packing");
                        qtyCheck = stock;
                    }
                    lbQty.Text = "Kol. (" + stock.ToString(CommonData.GetQtyPicture()) + ")";
                    tbPacking.Text = moveItem.GetDouble("Packing").ToString();
                }
            }
        }


        private void FilData(string barcode)
        {
            if (!String.IsNullOrEmpty(barcode) && barcode != "Scan fail")
            {
                switch (CurrentFlow.GetString("CurrentFlow"))
                {
                    case "3":

                        existsDuplicate = data.Where(x => x.SSCC == barcode).FirstOrDefault();
                        if (existsDuplicate != null)
                        {
                            Toast.MakeText(this, $"Ne morete skenirati {existsDuplicate.SSCC} še enkrat.", ToastLength.Long);
                            return;
                        }
                        if (openIdent == null)
                        {
                            return;
                        }
                        client = moveHead.GetString("Receiver");
                        sscc = barcode;
                        warehouse = moveHead.GetString("Wharehouse");

                        var parameters = new List<Services.Parameter>();

                        parameters.Add(new Services.Parameter { Name = "acISubject", Type = "String", Value = client });
                        parameters.Add(new Services.Parameter { Name = "acWarehouse", Type = "String", Value = warehouse });
                        parameters.Add(new Services.Parameter { Name = "acSSCC", Type = "String", Value = barcode });


                        query = $"SELECT * FROM uWMSItemBySSCCWarehouseSubject WHERE acISubject = @acISubject AND acWarehouse = @acWarehouse AND acSSCC = @acSSCC;";

                        result = Services.GetObjectListBySql(query, parameters);

                        if (result.Success && result.Rows.Count > 0)
                        {
                            MorePallets instance = new MorePallets();
                            Row row = result.Rows[0];
                            string identValue = row.StringValue("acIdent");
                            string name = row.StringValue("acName");
                            double? qty = row.DoubleValue("anQty");
                            string location = row.StringValue("aclocation");
                            string serial = row.StringValue("acSerialNo");
                            string sscc = row.StringValue("acSSCC");
                            string warehouse = row.StringValue("acWarehouse");
                            long? no = row.IntValue("anNo");
                            string keyResponse = row.StringValue("acKey");
                            instance.Ident = identValue;
                            instance.Name = name;
                            instance.Quantity = qty.ToString();
                            instance.Location = location;
                            instance.Serial = serial;
                            instance.SSCC = sscc;
                            instance.no = (int)no;
                            instance.key = keyResponse;
                            instance.friendlySSCC = sscc.Substring(sscc.Length - 5);
                            data.Add(instance);
                            adapterNew.NotifyDataSetChanged();
                        }

                        break;
                }
            }
        }

        private void TbSSCCpopup_KeyPress(object sender, View.KeyEventArgs e)
        {
            e.Handled = false;
            if (e.Event.Action == KeyEventActions.Down && e.KeyCode == Keycode.Enter)
            {
                // Add your logic here 
                FilData(tbSSCCpopup.Text);
            }
        }
        private void LvCardMore_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            var index = e.Position;
            var element = data.ElementAt(index);
            string formated = $"Izbrali ste {element.SSCC}.";
            Toast.MakeText(this, formated, ToastLength.Long).Show();
        }

        private void BtConfirm_Click(object sender, EventArgs e)
        {
            if (data.Count != 0)
            {
                string formatedString = $"{data.Count} skeniranih SSCC koda.";
                tbSSCC.Text = formatedString;
                tbSerialNum.Text = "...";
                tbLocation.Text = "...";
                tbIdent.Text = "...";
                tbPacking.Text = "...";
                tbLocation.RequestFocus();
                isMorePalletsMode = true;
                isBatch = true;
                popupDialogMain.Dismiss();
                popupDialogMain.Hide();
            }
            else
            {
                popupDialogMain.Dismiss();
                popupDialogMain.Hide();
            }
        }
        private void BtExit_Click(object sender, EventArgs e)
        {
            popupDialogMain.Dismiss();
            popupDialogMain.Hide();
            isOkayToCallBarcode = false;
        }


        private void LvCardMore_ItemLongClick(object sender, AdapterView.ItemLongClickEventArgs e)
        {
            var index = e.Position;
            var item = adapterNew.retunObjectAt(index);



            AlertDialog.Builder alert = new AlertDialog.Builder(this);
            alert.SetTitle("Podatki o kodi");
            alert.SetMessage($"Podatki:\nIdent: {item.Ident}\nNaziv: {item.Name}\nSSCC: {item.SSCC}\nKoličina: {item.Quantity}\n");
            // Close button
            alert.SetNegativeButton("Zapri", (senderAlert, args) =>
            {

            });
            alert.SetPositiveButton("Pobriši", (senderAlert, args) =>
            {
                DeleteFromTouch(index);
            });

            Dialog dialog = alert.Create();
            dialog.Show();

        }

        private void BtMorePallets_Click(object sender, EventArgs e)
        {
            isOkayToCallBarcode = true;
            // StartActivity(typeof(MorePalletsClass));
            popupDialogMain = new Dialog(this);
            popupDialogMain.SetContentView(Resource.Layout.MorePalletsClass);
            popupDialogMain.Window.SetSoftInputMode(SoftInput.AdjustResize);
            popupDialogMain.Show();
            popupDialogMain.Window.SetLayout(LayoutParams.MatchParent, LayoutParams.WrapContent);
            btConfirm = popupDialogMain.FindViewById<Button>(Resource.Id.btConfirm);
            btExit = popupDialogMain.FindViewById<Button>(Resource.Id.btExit);
            tbSSCCpopup = popupDialogMain.FindViewById<EditText>(Resource.Id.tbSSCC);
            tbSSCCpopup.SetBackgroundColor(Android.Graphics.Color.Aqua);
            tbSSCCpopup.KeyPress += TbSSCCpopup_KeyPress;
            lvCardMore = popupDialogMain.FindViewById<ListView>(Resource.Id.lvCardMore);
            lvCardMore.ItemLongClick += LvCardMore_ItemLongClick;
            adapterNew = new MorePalletsAdapter(this, data);
            lvCardMore.Adapter = adapterNew;
            lvCardMore.ItemSelected += LvCardMore_ItemSelected;
            btConfirm.Click += BtConfirm_Click;
            btExit.Click += BtExit_Click;
            tbSSCCpopup.RequestFocus();
        }

     /*   private void SearchableSpinner_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            try
            {
                if (receivedTrail != null)
                {
                    // Recalculate stock
                    string location = locations[e.Position];
                    qtyStock = receivedTrail.locationQty.GetValueOrDefault(location).ToString(CommonData.GetQtyPicture());
                    string qtyOrdered = receivedTrail.Quantity;
                    if (double.Parse(qtyStock) > double.Parse(qtyOrdered))
                    {
                        qtyStock = qtyOrdered;
                    }
                    lbQty.Text = "Kol. ( " + qtyStock + " )";
                    tbPacking.Text = double.Parse(qtyStock).ToString(CommonData.GetQtyPicture());                    
                    tbLocation.Text = location;
                    qtyCheck = receivedTrail.locationQty.GetValueOrDefault(location);

                }
            }
            catch
            {
                return;
            }
        }

        */

       

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
        private void BtSaveOrUpdate_LongClick(object sender, View.LongClickEventArgs e)
        {
            isMorePalletsMode = false;
            tbSSCC.Text = "";
            tbSerialNum.Text = "";
            tbPalette.Text = "";
            tbPacking.Text = "";
            tbLocation.Text = "";
            tbIdent.Text = "";
            tbSSCC.RequestFocus();
        }

        private void Button4_LongClick(object sender, View.LongClickEventArgs e)
        {
            isMorePalletsMode = false;
            tbSSCC.Text = "";
            tbSerialNum.Text = "";
            tbPalette.Text = "";
            tbPacking.Text = "";
            tbLocation.Text = "";
            tbIdent.Text = "";
            tbSSCC.RequestFocus();
        }

      
        private void DeleteFromTouch(int index)
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
        }

        private void No(int index)
        {
            popupDialog.Dismiss();
            popupDialog.Hide();
        }

        private void Yes(int index)
        {
            data.RemoveAt(index);
            lvCardMore.Adapter = null;
            lvCardMore.Adapter = adapter;
            popupDialog.Dismiss();
            popupDialog.Hide();
        }

     

       
  


        private void Button5_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(IssuedGoodsEnteredPositionsView));
            Finish();
            InvalidateAndClose();
        }




        private void Button7_Click(object sender, EventArgs e)
        {
            
                StartActivity(typeof(IssuedGoodsEnteredPositionsView));
                Finish();
                InvalidateAndClose();
            

        }

        private void InvalidateAndClose()
        {
            InUseObjects.Invalidate("ExtraData");

        }


        private async Task FinishMethod()
        {
            await Task.Run(async () =>
            {
                var resultAsync = SaveMoveItem().Result;
                if (resultAsync)
                {
                    RunOnUiThread(() =>
                    {
                        progress = new ProgressDialogClass();
                        progress.ShowDialogSync(this, "Zaključujem");
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

                                    Toast.MakeText(this, "Zaključevanje uspešno! Št. izdaje:\r\n" + id, ToastLength.Long).Show();

                                    InvalidateAndClose();

                                    AlertDialog.Builder alert = new AlertDialog.Builder(this);

                                    alert.SetTitle("Zaključevanje uspešno");

                                    alert.SetMessage("Zaključevanje uspešno! Št.prevzema:\r\n" + id);

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
                                    alert.SetTitle("Napaka");
                                    alert.SetMessage("Napaka pri zaključevanju: " + result);

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
                            Toast.MakeText(this, "Napaka pri klicu do web aplikacije" + result, ToastLength.Long).Show();

                        }
                    }
                    finally
                    {
                        RunOnUiThread(() =>
                        {
                            progress.StopDialogSync();

                        });
                    }
                }
            });
        }

     

       
        private async void Button6_Click(object sender, EventArgs e)
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

        private async void Button4_Click(object sender, EventArgs e)
        {
            if (!isMorePalletsMode)
            {
                var result = SaveMoveItem().Result;
                if (result)
                {
                    StartActivity(typeof(ClientPicking));
                    Finish();
                    InvalidateAndClose();
                }
            }
            else
            {
                if (String.IsNullOrEmpty(tbLocation.Text))
                {
                    return;
                }
                SavePositions();

                if (moveHead.GetBool("ByOrder") && CurrentFlow.GetString("CurrentFlow") == "2")
                {
                    StartActivity(typeof(IssuedGoodsIdentEntryWithTrail));
                    Finish();
                }
                else if (CurrentFlow.GetString("CurrentFlow") == "1")
                {
                    StartActivity(typeof(IssuedGoodsIdentEntry));
                    Finish();
                }
                InvalidateAndClose();

            }
        }

        private async void BtSaveOrUpdate_Click(object sender, EventArgs e)
        {
            if (!isMorePalletsMode)
            {
                var result = SaveMoveItem().Result;
                if (result)
                {
                    if (editMode)
                    {
                        StartActivity(typeof(IssuedGoodsEnteredPositionsView));
                        Finish();

                    }
                    else
                    {
                        StartActivity(typeof(IssuedGoodsSerialOrSSCCEntryClientPicking));
                        Finish();

                    }

                }
                else
                {
                    Toast.MakeText(this, "Obstaja napaka na podatkih.", ToastLength.Long).Show();
                }

            }
            else
            {
                if (String.IsNullOrEmpty(tbLocation.Text))
                {
                    return;
                }
                SavePositions();

                if (moveHead.GetBool("ByOrder") && CurrentFlow.GetString("CurrentFlow") == "2")
                {
                    StartActivity(typeof(IssuedGoodsIdentEntryWithTrail));
                    Finish();
                }
                else if (CurrentFlow.GetString("CurrentFlow") == "1")
                {
                    StartActivity(typeof(IssuedGoodsIdentEntry));
                    Finish();
                }
                InvalidateAndClose();

            }
        }
        private async void SavePositions()
        {
            progress = new ProgressDialogClass();
            progress.ShowDialogSync(this, "Shranjujem pozicije.");
            foreach (var x in data)
            {

                await SaveMoveItemBatch(x);

            }
            progress.StopDialogSync();
        }

        private async Task<bool> SaveMoveItemBatch(MorePallets obj)
        {

            if (string.IsNullOrEmpty(obj.Quantity.Trim()))
            {
                return true;
            }

            if (tbSSCC.Enabled && string.IsNullOrEmpty(obj.SSCC))
            {
                return false;
            }

            if (tbSerialNum.Enabled && string.IsNullOrEmpty(obj.Serial))
            {

                return false;
            }

            if (!CommonData.IsValidLocation(moveHead.GetString("Wharehouse"), obj.Location))
            {
                return false;
            }
            if (!LoadStock(moveHead.GetString("Wharehouse"), obj.Location, obj.SSCC, obj.Serial, openIdent.GetString("Code")))
            {
                return false;
            }

            if (string.IsNullOrEmpty(tbPacking.Text.Trim()))
            {
                return false;
            }
            else
            {
                try
                {
                    var qty = Convert.ToDouble(obj.Quantity);
                    if (qty == 0.0)
                    {
                        return false;
                    }

                    if (moveHead.GetBool("ByOrder") && !isPackaging && CheckIssuedOpenQty())
                    {
                        var tolerance = openIdent.GetDouble("TolerancePercent");
                        var maxVal = Math.Abs(openOrder.GetDouble("OpenQty") * (1.0 + tolerance / 100));
                        if (Math.Abs(qty) > maxVal)
                        {

                            return false;
                        }
                    }
                }
                catch (Exception)
                {
                    return false;
                }
            }
            if (string.IsNullOrEmpty(tbUnits.Text.Trim()))
            {
                return false;
            }
            else
            {
                try
                {
                    var units = Convert.ToDouble(tbUnits.Text.Trim());
                    if (units == 0.0)
                    {

                        return false;
                    }
                }
                catch (Exception)
                {

                    return false;
                }
            }

            if (CommonData.GetSetting("IssuedGoodsPreventSerialDups") == "1")
            {
                try
                {
                    var headID = moveHead.GetInt("HeadID");
                    var serialNo = obj.Serial;
                    var sscc = obj.SSCC;

                    string result;
                    if (WebApp.Get("mode=canAddSerial2&hid=" + headID.ToString() + "&sn=" + serialNo + "&sscc=" + sscc + "&ident=" + openIdent.GetString("Code"), out result))
                    {
                        if (!result.StartsWith("OK!"))
                        {
                            RunOnUiThread(() =>
                            {
                                Toast.MakeText(this, result, ToastLength.Long).Show();

                            });
                            return false;
                        }
                    }
                    else
                    {
                        RunOnUiThread(() =>
                        {
                            Toast.MakeText(this, "Napaka pri klicu web aplikacije" + result, ToastLength.Long).Show();


                        });
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    Crashes.TrackError(ex);
                    return false;
                }

            }
            try
            {
                moveItemNew = new NameValueObject("MoveItem");
                moveItemNew.SetInt("HeadID", moveHead.GetInt("HeadID"));
                moveItemNew.SetString("LinkKey", obj.key);
                moveItemNew.SetInt("LinkNo", obj.no);
                moveItemNew.SetString("Ident", obj.Ident);
                moveItemNew.SetString("SSCC", obj.SSCC.Trim());
                moveItemNew.SetString("SerialNo", obj.Serial.Trim());
                moveItemNew.SetDouble("Packing", Convert.ToDouble(obj.Quantity.Trim()));
                moveItemNew.SetDouble("Factor", Convert.ToDouble(tbUnits.Text.Trim()));
                moveItemNew.SetDouble("Qty", Convert.ToDouble(tbUnits.Text.Trim()) * Convert.ToDouble(obj.Quantity.Trim()));
                moveItemNew.SetInt("Clerk", Services.UserID());
                moveItemNew.SetString("Location", obj.Location);
                moveItemNew.SetString("Palette", "1");
                string error;
                moveItemNew = Services.SetObject("mi", moveItemNew, out error);
                var jsonobj = GetJSONforMoveItem(moveItemNew);
                if (moveItemNew == null)
                {
                    RunOnUiThread(() =>
                    {
                        var debug = error;
                        Toast.MakeText(this, "Napaka pri dostopu web aplikacije." + error, ToastLength.Long).Show();
                    });
                    return false;
                }
                else
                {
                    var debug = error;
                    InUseObjects.Invalidate("MoveItem");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Crashes.TrackError(ex);
                return false;
            }
        }
        private string GetJSONforMoveItem(NameValueObject moveItem)
        {
            moveItem item = new moveItem();
            item.HeadID = moveHead.GetInt("HeadID");
            item.LinkKey = moveItem.GetString("LinkKey");
            item.LinkNo = moveItem.GetInt("LinkNo");
            item.Ident = moveItem.GetString("Ident");
            item.SSCC = moveItem.GetString("SSCC");
            item.SerialNo = moveItem.GetString("SerialNo");
            item.Packing = moveItem.GetDouble("Packing");
            item.Factor = moveItem.GetDouble("Factor");
            item.Qty = moveItem.GetDouble("Qty");
            item.Clerk = moveItem.GetInt("Clerk");
            item.Location = moveItem.GetString("Location");
            item.IssueLocation = moveItem.GetString("IssueLocation");
            item.Pallete = moveItem.GetString("Pallete");
            var jsonString = JsonConvert.SerializeObject(item);
            return jsonString;
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            var qty = tbPacking.Text;
            if (qty.Trim().StartsWith("-"))
            {
                qty = qty.Trim().Substring(1);
            }
            else
            {
                qty = "-" + qty;
            }
            tbPacking.Text = qty;
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
                btSaveOrUpdate.Text = "Serij. - F2";
            }
            else
            {
                tbIdent.Text = openIdent.GetString("Code") + " " + openIdent.GetString("Name");

                if (extraData != null)
                {

                    tbLocation.Text = extraData.GetString("Location");
                    tbPacking.Text = extraData.GetDouble("Qty").ToString();
                }
            }

            isPackaging = openIdent.GetBool("IsPackaging");
            if (isPackaging)
            {
                tbSSCC.Enabled = false;
                tbSerialNum.Enabled = false;
            }
            else
            {

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


            tbIdent.RequestFocus();
            var ident = openIdent.GetString("Code");
            var location = CommonData.GetSetting("DefaultProductionLocation");
            if (tbLocation.Text == String.Empty)
            {
                if (location != null)
                {
                    tbLocation.Text = location;

                }

            }
            var warehouse = moveHead.GetString("Wharehouse");
          

            tbSSCC.RequestFocus();

        }


        private bool LoadStock(string warehouse, string location, string sscc, string serialNum, string ident)
        {
            try
            {


                string error;
                stock = Services.GetObject("str", warehouse + "|" + location + "|" + sscc + "|" + serialNum + "|" + ident, out error);
                if (stock == null)
                {
                    Toast.MakeText(this, "Napaka pri dostopu do web aplikacije" + error, ToastLength.Long).Show();
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Crashes.TrackError(ex);
                return false;
            }
        }




        private double LoadStockDouble(string warehouse, string location, string sscc, string serialNum, string ident)
        {
            try
            {


                string error;
                stock = Services.GetObject("str", warehouse + "|" + location + "|" + sscc + "|" + serialNum + "|" + ident, out error);
                if (stock == null)
                {
                    Toast.MakeText(this, "Napaka pri dostopu do web aplikacije" + error, ToastLength.Long).Show();
                    return 0;
                }

                return stock.GetDouble("RealStock");
            }
            catch (Exception ex)
            {
                Crashes.TrackError(ex);
                return 0;
            }
        }


        private static bool? checkIssuedOpenQty = null;

        private ProgressDialogClass progress;
        private Dialog popupDialogMain;
        private Button btConfirm;
        private Button btExit;
        private EditText tbSSCCpopup;
        private ListView lvCardMore;
        private MorePalletsAdapter adapter;
        private Dialog popupDialog;
        private Button btnYes;
        private Button btnNo;
        private bool isFirst;
        private bool isMorePalletsMode;
        private bool isBatch;
        private int check;
        private bool isOkayToCallBarcode;
        private MorePalletsAdapter adapterNew;
        private EditText tbLocationPopup;

        private NameValueObject moveItemNew;
        private Dialog popupDialogConfirm;
        private Button btnYesConfirm;
        private Button btnNoConfirm;
        private CustomAutoCompleteAdapter<string> DataAdapter;
        private string qtyStock;
        private MorePallets existsDuplicate;
        private string ident;
        private string client;
        private string sscc;
        private string warehouse;
        private string query;
        private ApiResultSet result;
        private bool isUpdate = false;

        private bool CheckIssuedOpenQty()
        {
            if (checkIssuedOpenQty == null)
            {


                try
                {
                    string error;
                    var useObj = Services.GetObject("cioqUse", "", out error);
                    checkIssuedOpenQty = useObj == null ? false : useObj.GetBool("Use");
                }
                catch (Exception ex) 
                {
                    Crashes.TrackError(ex);
                    
                }
            }
            return (bool)checkIssuedOpenQty;
        }


        private async Task<bool> SaveMoveItem()
        {
            // Is the user trying to issue more than the available stock
            try
            {
                double result;
                if (double.TryParse(tbPacking.Text, out result))
                {
                    if (qtyCheck < double.Parse(tbPacking.Text) && CommonData.GetSetting("CheckIssuedOpenQty") == "1")
                    {
                        RunOnUiThread(() =>
                        {
                            Toast.MakeText(this, "Ne morete izdati več kot je trenutno na zalogi!", ToastLength.Long).Show();
                        });
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch
            {

                RunOnUiThread(() =>
                {
                    Toast.MakeText(this, "Nepravilen vnos", ToastLength.Long).Show();
                });
                return false;
            }

            if (string.IsNullOrEmpty(tbPacking.Text.Trim()))
            {
                return true;
            }

            if (tbSSCC.Enabled && string.IsNullOrEmpty(tbSSCC.Text.Trim()))
            {
                RunOnUiThread(() =>
                {
                    Toast.MakeText(this, "SSCC koda je obvezen podatek", ToastLength.Long).Show();

                    tbSSCC.RequestFocus();
                });

                return false;
            }
            if (tbSerialNum.Enabled && string.IsNullOrEmpty(tbSerialNum.Text.Trim()))
            {
                RunOnUiThread(() =>
                {
                    Toast.MakeText(this, "Serijska številka je obvezen podatek!", ToastLength.Long).Show();

                    tbSerialNum.RequestFocus();
                });

                return false;
            }

            if (!CommonData.IsValidLocation(moveHead.GetString("Wharehouse"), tbLocation.Text.Trim()))
            {
                RunOnUiThread(() =>
                {
                    Toast.MakeText(this, "Lokacija '" + tbLocation.Text.Trim() + "' ni veljavna za skladišče '" + moveHead.GetString("Wharehouse") + "'!", ToastLength.Long).Show();

                    tbLocation.RequestFocus();
                });

                return false;
            }
            if (!LoadStock(moveHead.GetString("Wharehouse"), tbLocation.Text.Trim(), tbSSCC.Text.Trim(), tbSerialNum.Text.Trim(), openIdent.GetString("Code")))
            {
                return false;
            }

            if (string.IsNullOrEmpty(tbPacking.Text.Trim()))
            {
                RunOnUiThread(() =>
                {
                    Toast.MakeText(this, "Količina je obvezen podatek", ToastLength.Long).Show();
                    tbPacking.RequestFocus();
                });

                return false;
            }
            else
            {
                try
                {
                    var qty = Convert.ToDouble(tbPacking.Text.Trim());
                    if (qty == 0.0)
                    {
                        RunOnUiThread(() =>
                        {
                            Toast.MakeText(this, "Količina je obvezen podatek in mora biti različna od nič.", ToastLength.Long).Show();

                            tbPacking.RequestFocus();
                        });

                        return false;
                    }                  
                }
                catch (Exception e)
                {
                    RunOnUiThread(() =>
                    {
                        Toast.MakeText(this, "Količina mora biti število (" + e.Message + ")!", ToastLength.Long).Show();

                        tbPacking.RequestFocus();
                    });

                    return false;
                }
            }

            if (string.IsNullOrEmpty(tbUnits.Text.Trim()))
            {
                RunOnUiThread(() =>
                {
                    Toast.MakeText(this, "Število enota je obvezan podatek", ToastLength.Long).Show();
                    tbUnits.RequestFocus();
                });

                return false;
            }
            else
            {
                try
                {
                    var units = Convert.ToDouble(tbUnits.Text.Trim());
                    if (units == 0.0)
                    {
                        RunOnUiThread(() =>
                        {
                            Toast.MakeText(this, "Število enota je obvezan podatek in more biti raličit o nič", ToastLength.Long).Show();
                            tbUnits.RequestFocus();
                        });

                        return false;
                    }
                }
                catch (Exception e)
                {
                    RunOnUiThread(() =>
                    {
                        Toast.MakeText(this, "Število enota mora biti število (" + e.Message, ToastLength.Long).Show();

                        tbUnits.RequestFocus();
                    });

                    return false;
                }
            }
            try
            {
                if (moveItem == null)
                {
                    moveItem = new NameValueObject("MoveItem");
                }

                moveItem.SetInt("HeadID", moveHead.GetInt("HeadID"));
                var No = moveHead.GetInt("LinkNoTrail");
                if (receivedTrail != null)
                {
                    moveItem.SetString("LinkKey", receivedTrail.Order);
                    moveItem.SetInt("LinkNo", receivedTrail.No);
                } else
                {
                    moveItem.SetString("LinkKey", moveItem.GetString("LinkKey"));
                    moveItem.SetInt("LinkNo", moveItem.GetInt("LinkNo"));
                }
                moveItem.SetString("Ident", openIdent.GetString("Code"));
                moveItem.SetString("SSCC", tbSSCC.Text.Trim());
                moveItem.SetString("SerialNo", tbSerialNum.Text.Trim());
                moveItem.SetDouble("Packing", Convert.ToDouble(tbPacking.Text.Trim()));
                moveItem.SetDouble("Factor", Convert.ToDouble(tbUnits.Text.Trim()));
                moveItem.SetDouble("Qty", Convert.ToDouble(tbUnits.Text.Trim()) * Convert.ToDouble(tbPacking.Text.Trim()));
                moveItem.SetInt("Clerk", Services.UserID());
                moveItem.SetString("Location", tbLocation.Text.Trim());
                moveItem.SetString("Palette", tbPalette.Text.Trim());
                string error;
                moveItem = Services.SetObject("mi", moveItem, out error);
                if (moveItem == null)
                {
                    RunOnUiThread(() =>
                    {
                        var debug = error;
                        Toast.MakeText(this, "Napaka pri dostopu web aplikacije." + error, ToastLength.Long).Show();
                    });
                    return false;
                }
                else
                {
                    InUseObjects.Invalidate("MoveItem");
                    return true;
                }
            } catch (Exception ex)
            {
                Crashes.TrackError(ex);
                return false;
            }
        }


      
        private void colorFields()
        {
            tbSSCC.SetBackgroundColor(Android.Graphics.Color.Aqua);
            tbSerialNum.SetBackgroundColor(Android.Graphics.Color.Aqua);
            tbLocation.SetBackgroundColor(Android.Graphics.Color.Aqua);
        }
   

        public void GetBarcode(string barcode)
        {
            if (tbSSCC.HasFocus)
            {
                if (barcode != "Scan fail")
                {
                    Sound();
                    tbSSCC.Text = barcode;
                }
            }
            else if (tbSerialNum.HasFocus)
            {
                if (barcode != "Scan fail")
                {
                    Sound();

                    tbSerialNum.Text = barcode;

                }
            }
            else if (tbLocation.HasFocus)
            {
                if (barcode != "Scan fail")
                {
                    Sound();
                    
                    tbLocation.Text = barcode;
                }
            }

        }

    
    
        private void Sound()
        {
            soundPool.Play(soundPoolId, 1, 1, 0, 0, 1);
        }
    }
}