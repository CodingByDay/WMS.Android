using Android.App;
using Android.Content;
using Android.Graphics.Drawables;
using Android.Media;
using Android.Net;
using Android.OS;
using Android.Views;
using Android.Widget;
using BarCode2D_Receiver;
using Com.Jsibbold.Zoomage;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Newtonsoft.Json;
using WMS.App;
using Stream = Android.Media.Stream;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using TrendNET.WMS.Core.Data;
using TrendNET.WMS.Device.App;
using TrendNET.WMS.Device.Services;
using static Android.App.ActionBar;
using WebApp = TrendNET.WMS.Device.Services.WebApp;

namespace WMS
{
    [Activity(Label = "IssuedGoodsSerialOrSSCCEntryTablet", ScreenOrientation = Android.Content.PM.ScreenOrientation.Landscape)]
    public class IssuedGoodsSerialOrSSCCEntryTablet : Activity, IBarcodeResult
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
        private ListView listData;
        private Button button4;
        private List<string> locations = new List<string>();
        private Button button6;
        private Button button5;
        private Button button7;
        private NameValueObject openIdent = (NameValueObject)InUseObjects.Get("OpenIdent");
        private NameValueObject openOrder = (NameValueObject)InUseObjects.Get("OpenOrder");
        private NameValueObject moveHead = (NameValueObject)InUseObjects.Get("MoveHead");
        private NameValueObject moveItem = (NameValueObject)InUseObjects.Get("MoveItem");
        private NameValueObject extraData = (NameValueObject)InUseObjects.Get("ExtraData");
        private NameValueObject lastItem = (NameValueObject)InUseObjects.Get("LastItem");
        private NameValueObjectList docTypes = null;
        private NameValueObject stock = null;
        private TextView lbQty;
        private bool editMode = false;
        private bool isPackaging = false;
        private TextView lbUnits;
        private TextView lbPalette;
        SoundPool soundPool;
        private Spinner spLocation;
        int soundPoolId;
        private ZoomageView imagePNG;
        private ProgressDialogClass progress;
        private List<LocationClass> items = new List<LocationClass>();
        private List<MorePallets> data = new List<MorePallets>();
        private Button btMorePallets;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Create your application here
            SetContentView(Resource.Layout.IssuedGoodsSerialOrSSCCEntryTablet);
            AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
            var _customToolbar = new CustomToolbar(this, toolbar, Resource.Id.navIcon);
            _customToolbar.SetNavigationIcon(settings.RootURL + "/Services/Logo");
            tbIdent = FindViewById<EditText>(Resource.Id.tbIdent);
            tbSSCC = FindViewById<EditText>(Resource.Id.tbSSCC);
            listData = FindViewById<ListView>(Resource.Id.listData);
            tbSerialNum = FindViewById<EditText>(Resource.Id.tbSerialNum);
            tbLocation = FindViewById<EditText>(Resource.Id.tbLocation);
            tbPacking = FindViewById<EditText>(Resource.Id.tbPacking);
            tbUnits = FindViewById<EditText>(Resource.Id.tbUnits);
            tbPalette = FindViewById<EditText>(Resource.Id.tbPalette);
            button1 = FindViewById<Button>(Resource.Id.button1);
            btSaveOrUpdate = FindViewById<Button>(Resource.Id.btSaveOrUpdate);
            button4 = FindViewById<Button>(Resource.Id.button4);
            button6 = FindViewById<Button>(Resource.Id.button6);
            button5 = FindViewById<Button>(Resource.Id.button5);
            button7 = FindViewById<Button>(Resource.Id.button7);
            lbQty = FindViewById<TextView>(Resource.Id.lbQty);
            lbUnits = FindViewById<TextView>(Resource.Id.lbUnits);
            lbPalette = FindViewById<TextView>(Resource.Id.lbPalette);
            imagePNG = FindViewById<ZoomageView>(Resource.Id.imagePNG);
            spLocation = FindViewById<Spinner>(Resource.Id.spLocation);
            btMorePallets = FindViewById<Button>(Resource.Id.btMorePallets);
            tbIdent.InputType = Android.Text.InputTypes.ClassNumber;
            tbSSCC.InputType = Android.Text.InputTypes.ClassNumber;
            tbLocation.InputType = Android.Text.InputTypes.ClassText;
            tbUnits.InputType = Android.Text.InputTypes.ClassNumber;
            tbPacking.InputType = Android.Text.InputTypes.ClassNumber;
            tbPalette.InputType = Android.Text.InputTypes.ClassNumber;
            soundPool = new SoundPool(10, Stream.Music, 0);
            soundPoolId = soundPool.Load(this, Resource.Raw.beep, 1);
            Barcode2D barcode2D = new Barcode2D();
            barcode2D.open(this, this);
            spLocation.ItemSelected += SpLocation_ItemSelected;
            button1.Click += Button1_Click;
            btSaveOrUpdate.Click += BtSaveOrUpdate_Click;
            button4.Click += Button4_Click;
            button5.Click += Button5_Click;
            button6.Click += Button6_Click;
            button7.Click += Button7_Click;
            tbSSCC.KeyPress += TbSSCC_KeyPress;
            tbPacking.FocusChange += TbPacking_FocusChange;
            imagePNG.Visibility = ViewStates.Invisible;
            tbSSCC.LongClick += TbSSCC_LongClick;
            btMorePallets.Click += BtMorePallets_Click;
            tbPacking.SetSelectAllOnFocus(true);

            AdapterLocation adapter = new AdapterLocation(this, items);
            listData.Adapter = adapter;

            btSaveOrUpdate.LongClick += BtSaveOrUpdate_LongClick;
            button4.LongClick += Button4_LongClick;


            colorFields();

            if (moveHead == null)
            {
                AlertDialog.Builder alert = new AlertDialog.Builder(this);
                alert.SetTitle("Napaka na aplikaciji");
                alert.SetMessage("Prišlo je do napake in aplikacija se bo zaprla.");

                alert.SetPositiveButton("Ok", (senderAlert, args) =>
                {
                    alert.Dispose();
                    System.Threading.Thread.Sleep(500);
                    throw new ApplicationException("Error. moveHead.");
                });

                Dialog dialog = alert.Create();
                dialog.Show();
            }
            if (openIdent == null)
            {
                AlertDialog.Builder alert = new AlertDialog.Builder(this);
                alert.SetTitle("Napaka na aplikaciji");
                alert.SetMessage("Prišlo je do napake in aplikacija se bo zaprla.");

                alert.SetPositiveButton("Ok", (senderAlert, args) =>
                {
                    alert.Dispose();
                    System.Threading.Thread.Sleep(500);
                    throw new ApplicationException("Error. openIdent.");
                });

                Dialog dialog = alert.Create();
                dialog.Show();
            }

            docTypes = CommonData.ListDocTypes("P|N");

            if ((lastItem != null) && lastItem.GetBool("IsLastItem"))
            {
                InUseObjects.Invalidate("LastItem");
                button4.Enabled = false;
            }

            LoadRelatedOrder();
            SetUpForm();
            FillTheIdentLocationList();

            await GetLocationsForGivenWarehouse(moveHead.GetString("Wharehouse"));


            await Update();

            adapterReceive = new CustomAutoCompleteAdapter<String>(this, Android.Resource.Layout.SimpleSpinnerItem, locations);
            adapterReceive.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            // Spinner 
            spLocation.Adapter = adapterReceive;

            spLocation.SetSelection(locations.IndexOf("P01"), true);
            var code = openIdent.GetString("Code");
            showPictureIdent(code);
            tbSSCC.FocusedByDefault = true;
            var _broadcastReceiver = new NetworkStatusBroadcastReceiver();
            _broadcastReceiver.ConnectionStatusChanged += OnNetworkStatusChanged;
            Application.Context.RegisterReceiver(_broadcastReceiver,
            new IntentFilter(ConnectivityManager.ConnectivityAction));


            // tbUnits.Enabled = false;
            // tbPalette.Enabled = false;

            if(editMode)
            {
                btMorePallets.Visibility = ViewStates.Gone;
            }
        }



        private async Task Update()
        {
            await Task.Run(() =>
            {

                if (openOrder != null)
                {
                    string error;
                    string linkkey = openOrder.GetString("Key");
                    int linkno = openOrder.GetInt("No");
                    openOrder = Services.GetObject("oobl", linkkey + "|" + linkno.ToString(), out error);
                }

            });

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

        private void Button4_LongClick(object sender, View.LongClickEventArgs e)
        {
            if (isMorePalletsMode && data.Count != 0)
            {
                isMorePalletsMode = false;

                button4.Enabled = true;
                tbSSCC.Text = "";
                tbSerialNum.Text = "";
                tbPalette.Text = "";
                tbPacking.Text = "";
                tbLocation.Text = "";
                tbSSCC.RequestFocus();

            }
        }

        private void BtSaveOrUpdate_LongClick(object sender, View.LongClickEventArgs e)
        {
            if (isMorePalletsMode && data.Count != 0)
            {
                btSaveOrUpdate.Enabled = true;
                tbSSCC.Text = "";
                tbSerialNum.Text = "";
                tbPalette.Text = "";
                tbPacking.Text = "";
                tbLocation.Text = "";

                tbSSCC.RequestFocus();
            }
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



        private void TbSSCCpopup_FocusChange(object sender, View.FocusChangeEventArgs e)
        {
            FillData(tbSSCCpopup.Text, false);
        }
        private void TbSSCCpopup_KeyPress(object sender, View.KeyEventArgs e)
        {
            e.Handled = false;
            if (e.Event.Action == KeyEventActions.Down && e.KeyCode == Keycode.Enter)
            {
                // Add your logic here 
                FillData(tbSSCCpopup.Text, false);

            }
        }
        private async Task<bool> SaveMoveItemWithParams(MorePallets objectItem, bool isFirst)
        {
            moveHead = (NameValueObject)InUseObjects.Get("MoveHead");

            if (string.IsNullOrEmpty(objectItem.Quantity.Trim()))
            {
                return true;
            }

            if (string.IsNullOrEmpty(objectItem.SSCC.Trim()))
            {
                RunOnUiThread(() =>
                {
                    // Toast.MakeText(this, "SSCC koda je obvezen podatek", ToastLength.Long).Show();
                    DialogHelper.ShowDialogError(this, this, "SSCC koda je obvezen podatek");


                });

                return false;
            }



            if (!CommonData.IsValidLocation(moveHead.GetString("Wharehouse"), objectItem.Location.Trim()))
            {
                RunOnUiThread(() =>
                {
                    DialogHelper.ShowDialogError(this, this, "Lokacija '" + objectItem.Location.Trim() + "' ni veljavna za skladišče '" + moveHead.GetString("Wharehouse") + "'!");

                });

                return false;
            }
            if (!LoadStock(moveHead.GetString("Wharehouse"), objectItem.Location.Trim(), objectItem.SSCC.Trim(), objectItem.Serial.Trim(), openIdent.GetString("Code")))
            {
                return false;
            }

            if (string.IsNullOrEmpty(objectItem.Quantity.Trim()))
            {
                RunOnUiThread(() =>
                {
                    DialogHelper.ShowDialogError(this, this, "Količina je obvezen podatek");
                });

                return false;
            }
            else
            {
                try
                {
                    var qty = Convert.ToDouble(objectItem.Quantity.Trim());
                    if (qty == 0.0)
                    {
                        RunOnUiThread(() =>
                        {
                            DialogHelper.ShowDialogError(this, this, "Količina je obvezen podatek in mora biti različna od nič.");


                        });

                        return false;
                    }

                    if (moveHead.GetBool("ByOrder") && !isPackaging && CheckIssuedOpenQty())
                    {
                        var tolerance = openIdent.GetDouble("TolerancePercent");
                        var maxVal = Math.Abs(openOrder.GetDouble("OpenQty") * (1.0 + tolerance / 100));
                        if (Math.Abs(qty) > maxVal)
                        {
                            RunOnUiThread(() =>
                            {
                                DialogHelper.ShowDialogError(this, this, "Količina presega (" + qty.ToString(CommonData.GetQtyPicture()) + ") naročilo (" + maxVal.ToString(CommonData.GetQtyPicture()) + ")!");
                                tbPacking.RequestFocus();
                            });

                            return false;
                        }
                    }

                    /*
                    if ((qty > 0) && (qty > stock.GetDouble("RealStock")))
                    {
                        MessageForm.Show("Količina (" + qty.ToString(CommonData.GetQtyPicture ()) + ") presega zalogo (" + stock.GetDouble("RealStock").ToString(CommonData.GetQtyPicture ()) + ")!");
                        tbQty.Focus();
                        return false;
                    }
                    */
                }
                catch (Exception e)
                {
                    RunOnUiThread(() =>
                    {
                        DialogHelper.ShowDialogError(this, this, "Količina mora biti število (" + e.Message + ")!");

                    });

                    return false;
                }
            }

            if (string.IsNullOrEmpty(tbUnits.Text.Trim()))
            {
                RunOnUiThread(() =>
                {
                    DialogHelper.ShowDialogError(this, this, "Število enota je obvezan podatek");
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
                            DialogHelper.ShowDialogError(this, this, "Število enota je obvezan podatek in more biti raličit o nič");
                            tbUnits.RequestFocus();
                        });

                        return false;
                    }
                }
                catch (Exception e)
                {
                    RunOnUiThread(() =>
                    {
                        DialogHelper.ShowDialogError(this, this, "Število enota mora biti število (" + e.Message);

                        tbUnits.RequestFocus();
                    });

                    return false;
                }
            }

            if (CommonData.GetSetting("IssuedGoodsPreventSerialDups") == "1")
            {

                try
                {


                    var headID = moveHead.GetInt("HeadID");


                    string result;
                    if (WebApp.Get("mode=canAddSerial2&hid=" + headID.ToString() + "&sn=" + objectItem.Serial + "&sscc=" + objectItem.SSCC + "&ident=" + openIdent.GetString("Code"), out result))

                        if (WebApp.Get("mode=canAddSerial2&hid=" + headID.ToString() + "&sn=" + objectItem.Serial + "&sscc=" + objectItem.SSCC + "&ident=" + openIdent.GetString("Code"), out result))
                        {
                            if (!result.StartsWith("OK!"))
                            {
                                RunOnUiThread(() =>
                                {
                                    DialogHelper.ShowDialogError(this, this, result);

                                });
                                return false;
                            }
                        }
                        else
                        {
                            RunOnUiThread(() =>
                            {
                                DialogHelper.ShowDialogError(this, this, "Napaka pri klicu web aplikacije" + result);


                            });
                            return false;
                        }
                }
                catch (Exception err)
                {

                    Crashes.TrackError(err);
                    return false;

                }
            }


            try
            {


                if (moveItem == null) { moveItem = new NameValueObject("MoveItem"); }


                moveItem.SetInt("HeadID", moveHead.GetInt("HeadID"));
                moveItem.SetString("LinkKey", openOrder.GetString("Key"));
                moveItem.SetInt("LinkNo", openOrder.GetInt("No"));
                moveItem.SetString("Ident", openIdent.GetString("Code"));
                moveItem.SetString("SSCC", objectItem.SSCC.Trim());
                moveItem.SetString("SerialNo", objectItem.Serial.Trim());
                moveItem.SetDouble("Packing", Convert.ToDouble(objectItem.Quantity.Trim()));
                moveItem.SetDouble("Factor", Convert.ToDouble(tbUnits.Text.Trim()));
                moveItem.SetDouble("Qty", Convert.ToDouble(tbUnits.Text.Trim()) * Double.Parse(objectItem.Quantity));
                moveItem.SetInt("Clerk", Services.UserID());
                moveItem.SetString("Location", objectItem.Location.Trim());
                moveItem.SetString("Palette", tbPalette.Text.Trim());

                string error;

                moveItem = Services.SetObject("mi", moveItem, out error);

                if (moveItem == null)
                {
                    RunOnUiThread(() =>
                    {
                        var debug = error;

                        DialogHelper.ShowDialogError(this, this, "Napaka pri dostopu web aplikacije." + error);
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
            catch (Exception err)
            {

                Crashes.TrackError(err);
                return false;

            }
        }

        private void updateTheHead()
        {
            moveHead.SetInt("HeadID", 0); // da ga "mh" API shrani kot novega, ne pod starim ID
            string error;
            var savedMoveHead = Services.SetObject("mh", moveHead, out error);
            if (savedMoveHead == null)
            {
                RunOnUiThread(() =>
                {
                    DialogHelper.ShowDialogError(this, this, "Napaka pri zaklepanju nove medskladišnice.");

                });

                return;
            }
            else
            {
                if (!Services.TryLock("MoveHead" + savedMoveHead.GetInt("HeadID").ToString(), out error))
                {


                    RunOnUiThread(() =>
                    {
                        DialogHelper.ShowDialogError(this, this, "Napaka pri zaklepanju nove medskladišnice.");

                    });
                    return;
                }

                moveHead.SetInt("HeadID", savedMoveHead.GetInt("HeadID"));
                moveHead.SetBool("Saved", true);
                InUseObjects.Set("MoveHead", moveHead);

                var tests = moveHead.GetInt("HeadID");

            }
        }
        private async Task FinishMethodBatch()
        {
            await Task.Run(async () =>
            {

                RunOnUiThread(() =>
                {
                    progress = new ProgressDialogClass();

                    progress.ShowDialogSync(this, "Zaključujem več paleta na odpremi.");
                });

                try
                {

                    var headID = moveHead.GetInt("HeadID");

                    string result;

                    if (WebApp.Get("mode=finish&stock=remove&print=" + Services.DeviceUser() + "&id=" + headID.ToString(), out result))
                    {
                        if (result.StartsWith("OK!"))
                        {
                            check += 1;
                            RunOnUiThread(() =>
                            {
                                progress.StopDialogSync();

                                var id = result.Split('+')[1];

                                //    Toast.MakeText(this, "Zaključevanje uspešno! Št. izdaje:\r\n" + id, ToastLength.Long).Show();

                                InvalidateAndClose();

                                AlertDialog.Builder alert = new AlertDialog.Builder(this);

                                alert.SetTitle("Zaključevanje uspešno");

                                alert.SetMessage("Zaključevanje uspešno! Št.prevzema:\r\n" + id);

                                alert.SetPositiveButton("Ok", (senderAlert, args) =>
                                {
                                    alert.Dispose();
                                    System.Threading.Thread.Sleep(500);

                                    StartActivity(typeof(MainMenuTablet));
                                    HelpfulMethods.clearTheStack(this);


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
                                    StartActivity(typeof(MainMenuTablet));

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
                            DialogHelper.ShowDialogError(this, this, "Napaka pri klicu do web aplikacije" + result);



                        });

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
        private void TbSSCC_LongClick(object sender, View.LongClickEventArgs e)
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
        private void BtExit_Click(object sender, EventArgs e)
        {
            popupDialogMain.Dismiss();
            popupDialogMain.Hide();
        }
    
        private MorePallets ProcessQtyWithParams(MorePallets obj, string location)
        {
            try
            {
                if (!CommonData.IsValidLocation(moveHead.GetString("Wharehouse"), obj.Location.Trim()))
                {
                    DialogHelper.ShowDialogError(this, this, "Lokacija '" + obj.Location.Trim() + "' ni veljavna za skladišče '" + moveHead.GetString("Wharehouse") + "'!");

                    tbLocation.RequestFocus();
                    return null;
                }

                if (!LoadStock(moveHead.GetString("Wharehouse"), obj.Location.Trim(), obj.SSCC.Trim(), obj.Serial.Trim(), openIdent.GetString("Code")))
                {
                    DialogHelper.ShowDialogError(this, this, "Zaloga za SSCC/Serijsko št. ne obstaja.");

                    return null;
                }
                else
                {
                    string error;
                    // var fulfilledOrder = Services.GetObject("miho", openOrder.GetString("Key") + "|" + openOrder.GetInt("No") + "|" + openIdent.GetString("Code"), out error);
                    // var fulfilledQty = fulfilledOrder == null ? 0.0 : fulfilledOrder.GetDouble("Qty");

                    obj.Quantity = stock.GetDouble("RealStock").ToString(CommonData.GetQtyPicture());
                    lbQty.Text = "Kol. (" + (openOrder.GetDouble("OpenQty")).ToString(CommonData.GetQtyPicture()) + ")";

                    return obj;
                }
            }
            catch (Exception err)
            {

                Crashes.TrackError(err);
                return null;
            }

        }




        private void UpdateTheLocationAPICall(string location)
        {

            try
            {

                var headID = moveHead.GetInt("HeadID");

                string result;
                if (WebApp.Get("mode=mhLoc&id=" + headID + "&loc=" + location, out result))
                {
                    if (result.StartsWith("OK!"))
                    {

                        RunOnUiThread(() =>
                        {

                        });
                    }
                    else
                    {
                        RunOnUiThread(() =>
                        {




                        });

                    }
                }
                else
                {
                    string SuccessMessage = string.Format("Napaka pri klicu web aplikacije");
                    DialogHelper.ShowDialogError(this, this, SuccessMessage);
                }
            }
            catch (Exception err)
            {

                Crashes.TrackError(err);
                return;

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
                catch (Exception e)
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
                catch (Exception e)
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
                                DialogHelper.ShowDialogError(this, this, result);

                            });
                            return false;
                        }
                    }
                    else
                    {
                        RunOnUiThread(() =>
                        {
                            DialogHelper.ShowDialogError(this, this, "Napaka pri klicu web aplikacije" + result);


                        });
                        return false;
                    }
                }
                catch (Exception err)
                {

                    Crashes.TrackError(err);
                    return false;

                }
            }


            try
            {


                moveItemNew = new NameValueObject("MoveItem");

                moveItemNew.SetInt("HeadID", moveHead.GetInt("HeadID"));
                moveItemNew.SetString("LinkKey", openOrder.GetString("Key"));
                moveItemNew.SetInt("LinkNo", openOrder.GetInt("No"));
                moveItemNew.SetString("Ident", openIdent.GetString("Code"));
                moveItemNew.SetString("SSCC", obj.SSCC.Trim());
                moveItemNew.SetString("SerialNo", obj.Serial.Trim());
                moveItemNew.SetDouble("Packing", Convert.ToDouble(obj.Quantity.Trim()));
                moveItemNew.SetDouble("Factor", Convert.ToDouble(tbUnits.Text.Trim()));
                moveItemNew.SetDouble("Qty", Convert.ToDouble(tbUnits.Text.Trim()) * Convert.ToDouble(obj.Quantity.Trim()));
                moveItemNew.SetInt("Clerk", Services.UserID());
                moveItemNew.SetString("Location", obj.Location);
                moveItemNew.SetString("Palette", "1"); // Ask about this.

                string error;
                moveItemNew = Services.SetObject("mi", moveItemNew, out error);
                var jsonobj = GetJSONforMoveItem(moveItemNew);

                if (moveItemNew == null)
                {
                    RunOnUiThread(() =>
                    {
                        var debug = error;

                        DialogHelper.ShowDialogError(this, this, "Napaka pri dostopu web aplikacije." + error);
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
            catch (Exception err)
            {

                Crashes.TrackError(err);
                return false;

            }
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
                isBatch = true;
                isMorePalletsMode = true;
                btSaveOrUpdate.LongClickable = true;
                button4.LongClickable = true;
                popupDialogMain.Dismiss();
                popupDialogMain.Hide();

            }
            else
            {
                popupDialogMain.Dismiss();
                popupDialogMain.Hide();
                Toast.MakeText(this, "Manjkajo podatki", ToastLength.Long).Show();

            }
        }

        private void LvCardMore_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            var index = e.Position;
            var element = data.ElementAt(index);
            string formated = $"Izbrali ste {element.SSCC}.";
            Toast.MakeText(this, formated, ToastLength.Long).Show();
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
        private void DeleteFromTouch(int index)
        {
            popupDialog = new Dialog(this);
            popupDialog.SetContentView(Resource.Layout.YesNoPopUp);
            popupDialog.Window.SetSoftInputMode(SoftInput.AdjustResize);
            popupDialog.Show();

            popupDialog.Window.SetLayout(LayoutParams.MatchParent, LayoutParams.WrapContent);
            popupDialog.Window.SetBackgroundDrawableResource(Android.Resource.Color.HoloOrangeLight);

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
        private void SpLocation_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            var element = e.Position;
            var item = locations.ElementAt(element);


            tbLocation.Text = item;
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

                                    //Toast.MakeText(this, "Zaključevanje uspešno! Št. izdaje:\r\n" + id, ToastLength.Long).Show();
                                    InvalidateAndClose();
                                    AlertDialog.Builder alert = new AlertDialog.Builder(this);
                                    alert.SetTitle("Zaključevanje uspešno");
                                    alert.SetMessage("Zaključevanje uspešno! Št.prevzema:\r\n" + id);

                                    alert.SetPositiveButton("Ok", (senderAlert, args) =>
                                    {
                                        alert.Dispose();
                                        System.Threading.Thread.Sleep(500);
                                        StartActivity(typeof(MainMenuTablet));
                                        HelpfulMethods.clearTheStack(this);
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
                                        StartActivity(typeof(MainMenuTablet));
                                        HelpfulMethods.clearTheStack(this);

                                    });



                                    Dialog dialog = alert.Create();
                                    dialog.Show();
                                });
                            }
                        }
                        else
                        {
                            DialogHelper.ShowDialogError(this, this, "Napaka pri klicu do web aplikacije" + result);

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
        private void TbSSCC_KeyPress(object sender, View.KeyEventArgs e)
        {
            try
            {
                if (e.KeyCode == Keycode.Enter)
                {

                        tbSerialNum.Text = "";
                        tbPalette.Text = "";
                        tbPacking.Text = "";
                        tbLocation.Text = "";

                      
                        var dataObject = FillRelatedData(tbSSCC.Text);

                        if (dataObject != null && !String.IsNullOrEmpty(dataObject.Ident))
                        {
                            string error;
                            tbSSCC.Text = dataObject.SSCC;
                            var ident = dataObject.Ident;
                            var loadIdent = CommonData.LoadIdent(ident);
                            string idname = loadIdent.GetString("Name");
                            if (!String.IsNullOrEmpty(idname))
                            {
                                ProcessQty();
                                AskAboutQuantityCorrection();
                                tbSerialNum.RequestFocus();
                            }
                            else
                            {
                                tbSSCC.Text = String.Empty;
                                return;
                            }

                        }
                } else
                {
                    e.Handled = false;
                }
                   
            }
            catch (Exception ee)
            {
                Crashes.TrackError(ee);
            }
        }






        private async Task GetLocationsForGivenWarehouse(string warehouse)
        {
            await Task.Run(() =>
            {
                List<string> result = new List<string>();
                string error;
                var issuerLocs = Services.GetObjectList("lo", out error, warehouse);

                if (issuerLocs == null)
                {
                    RunOnUiThread(() =>
                    {
                        Toast.MakeText(this, "Prišlo je do napake", ToastLength.Long).Show();

                    });

                }
                else
                {
                    issuerLocs.Items.ForEach(x =>
                    {
                        var location = x.GetString("LocationID");

                        locations.Add(location);
                    });

                }


            });

        }

        private void TbPacking_FocusChange(object sender, View.FocusChangeEventArgs e)
        {
            ProcessQty();
        }

        private void Button5_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(IssuedGoodsEnteredPositionsViewTablet));
            HelpfulMethods.clearTheStack(this);
        }



        private void FillTheIdentLocationList()
        {
            var code = openIdent.GetString("Code");

            var wh = moveHead.GetString("Wharehouse");
            // Found the error here the receiver returns not the warehouse but the PETPAK receiver
            var list = GetIdentLocationList.fillItemsOfListLocationClass(wh, code);
            Fill(list);
        }



        private void Fill(List<LocationClass> list)
        {
            try
            {
                items = list;
                listData.Adapter = null;
                AdapterLocation adapter = new AdapterLocation(this, items);
                listData.Adapter = adapter;
            }
            catch
            {
            }
            ///
        }

        private void fillSugestedLocation(string warehouse)
        {
            var ident = openIdent.GetString("Code");

            string result;

            if (WebApp.Get("mode=bestLoc&wh=" + warehouse + "&ident=" + HttpUtility.UrlEncode(ident) + "&locMode=outgoing", out result))
            {
                var test = result;
                if (test != "Exception: The remote server returned an error: (404) Not Found.")
                {
                    tbLocation.Text = result;
                }
                else
                {
                    // Pass for now ie not supported.
                }
            }

            else
            { // Do nothing. }

            }
        }
        private void TransportOneObject(string sscc)
        {
            if (!String.IsNullOrEmpty(sscc))
            {
                string error;
                var dataObject = Services.GetObject("sscc", sscc, out error);
                if (dataObject != null)
                {
                    var ident = dataObject.GetString("Ident");
                    var loadIdent = CommonData.LoadIdent(ident);
                    var name = dataObject.GetString("IdentName");
                    var serial = dataObject.GetString("SerialNo");
                    var location = dataObject.GetString("Location");
                    MorePallets pallets = new MorePallets();
                    pallets.Ident = ident;
                    string idname = loadIdent.GetString("Name");
                    pallets.Location = location;
                    if (idname.Length > 20)
                    {
                        pallets.Name = idname.Trim().Substring(0, 20);
                    }
                    else
                    {
                        pallets.Name = idname;
                    }

                    pallets.Quantity = sscc;
                    pallets.SSCC = sscc;
                    pallets.Serial = serial;

                    pallets.friendlySSCC = pallets.SSCC;


                    enabledSerial = loadIdent.GetBool("HasSerialNumber");


#nullable enable        
                    MorePallets? obj = ProcessQtyWithParams(pallets, location);
#nullable disable
                    /* Adds an object to the list. */

                    if (obj is null)
                    {
                        Toast.MakeText(this, "Ne obstaja.", ToastLength.Long).Show();
                    }
                    else
                    {
                        data.Add(obj);


                    }
                }
                else
                {
                    return;
                }
            }
        }



        private void AskAboutQuantityCorrection()
        {
            if (openOrder != null && stock != null)
            {
                // Return in the case that its not needed.
                if (openOrder.GetDouble("OpenQty") >= stock.GetDouble("RealStock"))
                {
                    return;
                }

                AlertDialog.Builder alertDialogBuilder = new AlertDialog.Builder(this);
                // Set the title and message
                alertDialogBuilder.SetTitle("Izbor količine");
                alertDialogBuilder.SetMessage("Ali želite izdati celotno količino ali samo naročeno?");
                // Add two buttons with their respective actions
                alertDialogBuilder
                   .SetPositiveButton("Celotno", (senderAlert, args) =>
                   {
                       return;
                   })
                    .SetNegativeButton("Naročeno", (senderAlert, args) =>
                    {
                        tbPacking.Text = openOrder.GetDouble("OpenQty").ToString(CommonData.GetQtyPicture());
                    });
                AlertDialog alertDialog = alertDialogBuilder.Create();
                alertDialog.Show();
            }
        }

        private MorePallets FillData(string barcode, bool transport)
        {
            MorePallets morePallets = new MorePallets();
            if (!String.IsNullOrEmpty(barcode) && barcode != "Scan fail")
            {
                existsDuplicate = data.Where(x => x.SSCC == barcode).FirstOrDefault();
                if (existsDuplicate != null)
                {
                    Toast.MakeText(this, $"Ne morete skenirati {existsDuplicate.SSCC} še enkrat.", ToastLength.Long);
                    return morePallets;
                }
                if (openIdent == null)
                {
                    return morePallets;
                }
                ident = openIdent.GetString("Code");
                sscc = barcode;
                warehouse = moveHead.GetString("Wharehouse");
                var keyr = openOrder;

                query = $"SELECT * FROM uWMSItemBySSCCWarehouseItem WHERE acIdent = '{ident}' AND acSSCC = '{sscc}' AND acWarehouse = '{warehouse}' AND acKey = '{keyr.GetString("Key")}'";

                result = Services.GetObjectListBySql(query);

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
                    if (!transport)
                    {
                        adapterNew.NotifyDataSetChanged();
                    }
                    else
                    {
                        if (tbSerialNum.Enabled == true)
                        {
                            tbSerialNum.Text = serial;
                            tbLocation.Text = location;
                        }
                        else
                        {
                            tbLocation.Text = location;
                        }
                    }
                    return instance;
                }
            }
            return morePallets;
        }

        private MorePallets FillRelatedData(string text)
        {
            return FillData(text, true);
        }
        private void showPictureIdent(string ident)
        {
            try
            {
                Android.Graphics.Bitmap show = Services.GetImageFromServerIdent(moveHead.GetString("Wharehouse"), ident);
                var debug = moveHead.GetString("Wharehouse");
                Drawable d = new BitmapDrawable(Resources, show);

                imagePNG.SetImageDrawable(d);
                imagePNG.Visibility = ViewStates.Visible;


                imagePNG.Click += (e, ev) => { ImageClick(d); };

            }
            catch (Exception error)
            {
                return;
            }

        }
        private void showPicture()
        {
            try
            {
                Android.Graphics.Bitmap show = Services.GetImageFromServer(moveHead.GetString("Wharehouse"));

                Drawable d = new BitmapDrawable(Resources, show);

                imagePNG.SetImageDrawable(d);
                imagePNG.Visibility = ViewStates.Visible;


                imagePNG.Click += (e, ev) => { ImageClick(d); };

            }
            catch (Exception error)
            {
                return;
            }

        }

        private void ImageClick(Drawable d)
        {
            popupDialog = new Dialog(this);
            popupDialog.SetContentView(Resource.Layout.WarehousePicture);
            popupDialog.Window.SetSoftInputMode(SoftInput.AdjustResize);
            popupDialog.Show();

            popupDialog.Window.SetLayout(LayoutParams.MatchParent, LayoutParams.WrapContent);
            popupDialog.Window.SetBackgroundDrawableResource(Android.Resource.Color.HoloBlueBright);
            image = popupDialog.FindViewById<ZoomageView>(Resource.Id.image);
            image.SetMinimumHeight(500);
            image.SetMinimumWidth(800);
            image.SetImageDrawable(d);
            // Access Popup layout fields like below

        }




        private void BtnOK_Click(object sender, EventArgs e)
        {
            popupDialog.Dismiss();
            popupDialog.Hide();
        }
        private void colorFields()
        {
            tbSSCC.SetBackgroundColor(Android.Graphics.Color.Aqua);
            tbSerialNum.SetBackgroundColor(Android.Graphics.Color.Aqua);
            tbLocation.SetBackgroundColor(Android.Graphics.Color.Aqua);

        }


        private void LoadRelatedOrder()
        {
            if (moveItem != null)
            {
                try
                {
                    string error = "N/A";

                    editMode = true;
                    if (moveItem == null) { throw new ApplicationException("moveItem not known at this point?!"); }
                    if (string.IsNullOrEmpty(moveItem.GetString("LinkKey")))
                    {
                        openOrder = new NameValueObject("OpenOrder");
                    }
                    else
                    {
                        openOrder = Services.GetObject("oobl", moveItem.GetString("LinkKey") + "|" + moveItem.GetInt("LinkNo").ToString(), out error);
                        if (openOrder == null)
                        {
                            DialogHelper.ShowDialogError(this, this, "Napaka pri dostopu do web aplikacije: " + error);
                            return;
                        }
                    }
                }
                catch (Exception err)
                {

                }
            }
        }


        private void Button7_Click(object sender, EventArgs e)
        {
            {
                StartActivity(typeof(IssuedGoodsEnteredPositionsViewTablet));
                HelpfulMethods.clearTheStack(this);
                InvalidateAndClose();
            }

        }

        private void InvalidateAndClose()
        {
            InUseObjects.Invalidate("ExtraData");

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
            if (!isBatch)
            {
                await FinishMethod();
            }
            else
            {
                await FinishMethodBatch();
            }

        }

        private async void Button4_Click(object sender, EventArgs e)
        {
            if (!tbIdent.Text.Contains("..."))
            {
                
                var correct = SaveMoveItem().Result;
                if (correct)
                {
                    if (moveHead.GetBool("ByOrder") && CommonData.GetSetting("UseSingleOrderIssueing") == "1")
                    {
                        StartActivity(typeof(IssuedGoodsIdentEntryWithTrailTablet));
                        HelpfulMethods.clearTheStack(this);
                    }
                    else
                    {
                        StartActivity(typeof(IssuedGoodsIdentEntryTablet));
                        HelpfulMethods.clearTheStack(this);
                    }
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

                if (moveHead.GetBool("ByOrder") && CommonData.GetSetting("UseSingleOrderIssueing") == "1")
                {
                    StartActivity(typeof(IssuedGoodsIdentEntryWithTrailTablet));
                    Finish();
                }
                else
                {
                    StartActivity(typeof(IssuedGoodsIdentEntryTablet));
                    Finish();
                }
                InvalidateAndClose();

            }
        }

        private async void BtSaveOrUpdate_Click(object sender, EventArgs e)
        {
            if (!tbIdent.Text.Contains("..."))
            {
                var correct = SaveMoveItem().Result;
                if (correct)
                {
                    if (editMode)
                    {
                        StartActivity(typeof(IssuedGoodsEnteredPositionsViewTablet));
                        HelpfulMethods.clearTheStack(this);

                    }

                    else
                    {
                        StartActivity(typeof(IssuedGoodsSerialOrSSCCEntryTablet));
                        HelpfulMethods.clearTheStack(this);
                    }



                    Finish();

                }
            }
            else
            {

                if (String.IsNullOrEmpty(tbLocation.Text))
                {
                    return;
                }

                SavePositions();

                if (moveHead.GetBool("ByOrder") && CommonData.GetSetting("UseSingleOrderIssueing") == "1")
                {
                    StartActivity(typeof(IssuedGoodsIdentEntryWithTrailTablet));
                    Finish();
                }
                else
                {
                    StartActivity(typeof(IssuedGoodsIdentEntryTablet));
                    Finish();
                }
                InvalidateAndClose();

            }
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
                btSaveOrUpdate.Text = "Serijska - F2";
            }
            else
            {
                tbIdent.Text = openIdent.GetString("Code") + " " + openIdent.GetString("Name");
                var warehouse = moveHead.GetString("Wharehouse");

                fillSugestedLocation(warehouse);
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

            // string error;
            // var s = moveHead.GetString("Issuer");
            // var recommededLocation = Services.GetObject("rl", ident + "|" + moveHead.GetString("Issuer"), out error);
            // if (recommededLocation != null)
            // {

            //    tbLocation.Text = recommededLocation.GetString("Location");
            // }

            var location = CommonData.GetSetting("DefaultProductionLocation");
            tbLocation.Text = location;
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
                    DialogHelper.ShowDialogError(this, this, "Napaka pri dostopu do web aplikacije" + error);
                    return false;
                }

                return true;
            }
            catch(Exception err)
            {
                Crashes.TrackError(err);
                return false;
            }
        }
        private static bool? checkIssuedOpenQty = null;
        private Dialog popupDialog;
        private ZoomageView image;
        private Button btnOK;
        private bool isBatch;
        private bool isMorePalletsMode = false;
        private int check;
        private bool isFirst;
        private bool isOkayToCallBarcode;
        private Dialog popupDialogMain;
        private Button btConfirm;
        private Button btExit;
        private EditText tbSSCCpopup;
        private ListView lvCardMore;
        private MorePalletsAdapter adapter;
        private bool enabledSerial;
        private Button btnYes;
        private Button btnNo;

        private CustomAutoCompleteAdapter<string> adapterReceive;
        private NameValueObject moveItemNew;
        private MorePalletsAdapter adapterMore;
        private Dialog popupDialogConfirm;
        private Button btnYesConfirm;
        private Button btnNoConfirm;
        private MorePalletsAdapter adapterNew;
        private MorePallets existsDuplicate;
        private string ident;
        private string sscc;
        private string warehouse;
        private string query;
        private ApiResultSet result;

        private bool CheckIssuedOpenQty()
        {
            if (checkIssuedOpenQty == null)
            {

                ///
                try
                {
                    string error;
                    var useObj = Services.GetObject("cioqUse", "", out error);
                    checkIssuedOpenQty = useObj == null ? false : useObj.GetBool("Use");
                }
                catch (Exception err)
                {

                    Crashes.TrackError(err);
                    return false;

                }
            }
            return (bool)checkIssuedOpenQty;
        }
        // ---
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
        private async Task<bool> SaveMoveItem()
        {
            if (string.IsNullOrEmpty(tbPacking.Text.Trim()))
            {
                return true;
            }

            if (tbSSCC.Enabled && string.IsNullOrEmpty(tbSSCC.Text.Trim()))
            {
                RunOnUiThread(() =>
                {

                    DialogHelper.ShowDialogError(this, this, "SSCC koda je obvezen podatek.");

                    //Toast.MakeText(this, "SSCC koda je obvezen podatek.", ToastLength.Long).Show();

                    tbSSCC.RequestFocus();
                });

                return false;
            }

            if (tbSerialNum.Enabled && string.IsNullOrEmpty(tbSerialNum.Text.Trim()))
            {
                RunOnUiThread(() =>
                {
                    DialogHelper.ShowDialogError(this, this, "Serijska številka je obvezen podatek!");

                    tbSerialNum.RequestFocus();
                });

                return false;
            }

            if (!CommonData.IsValidLocation(moveHead.GetString("Wharehouse"), tbLocation.Text.Trim()))
            {
                RunOnUiThread(() =>
                {
                    // Toast.MakeText(this, "Lokacija '" + tbLocation.Text.Trim() + "' ni veljavna za skladišče '" + moveHead.GetString("Wharehouse") + "'!", ToastLength.Long).Show();
                    DialogHelper.ShowDialogError(this, this, "Lokacija '" + tbLocation.Text.Trim() + "' ni veljavna za skladišče '" + moveHead.GetString("Wharehouse") + "'!");

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
                    DialogHelper.ShowDialogError(this, this, "Količina je obvezen podatek");
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
                            DialogHelper.ShowDialogError(this, this, "Količina je obvezen podatek in mora biti različna od nič.");

                            tbPacking.RequestFocus();
                        });

                        return false;
                    }

                    if (moveHead.GetBool("ByOrder") && !isPackaging && CheckIssuedOpenQty())
                    {
                        var tolerance = openIdent.GetDouble("TolerancePercent");
                        var maxVal = Math.Abs(openOrder.GetDouble("OpenQty") * (1.0 + tolerance / 100));
                        if (Math.Abs(qty) > maxVal)
                        {
                            RunOnUiThread(() =>
                            {
                                DialogHelper.ShowDialogError(this, this, "Količina presega (" + qty.ToString(CommonData.GetQtyPicture()) + ") naročilo (" + maxVal.ToString(CommonData.GetQtyPicture()) + ")!");
                                tbPacking.RequestFocus();
                            });

                            return false;
                        }
                    }

                    /*
                    if ((qty > 0) && (qty > stock.GetDouble("RealStock")))
                    {
                        MessageForm.Show("Količina (" + qty.ToString(CommonData.GetQtyPicture ()) + ") presega zalogo (" + stock.GetDouble("RealStock").ToString(CommonData.GetQtyPicture ()) + ")!");
                        tbQty.Focus();
                        return false;
                    }
                    */
                }
                catch (Exception e)
                {
                    RunOnUiThread(() =>
                    {
                        DialogHelper.ShowDialogError(this, this, "Količina mora biti število (" + e.Message + ")!");

                        tbPacking.RequestFocus();
                    });

                    return false;
                }
            }

            if (string.IsNullOrEmpty(tbUnits.Text.Trim()))
            {
                RunOnUiThread(() =>
                {
                    DialogHelper.ShowDialogError(this, this, "Število enota je obvezan podatek");
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
                            DialogHelper.ShowDialogError(this, this, "Število enota je obvezan podatek in more biti raličit o nič");
                            tbUnits.RequestFocus();
                        });

                        return false;
                    }
                }
                catch (Exception e)
                {
                    RunOnUiThread(() =>
                    {
                        DialogHelper.ShowDialogError(this, this, "Število enota mora biti število (" + e.Message);

                        tbUnits.RequestFocus();
                    });

                    return false;
                }
            }

            if (CommonData.GetSetting("IssuedGoodsPreventSerialDups") == "1")
            {

                try
                {

                    var headID = moveHead.GetInt("HeadID");
                    var serialNo = tbSerialNum.Text.Trim();
                    var sscc = tbSSCC.Text.Trim();

                    string result;

                    if (WebApp.Get("mode=canAddSerial2&hid=" + headID.ToString() + "&sn=" + serialNo + "&sscc=" + sscc + "&ident=" + openIdent.GetString("Code"), out result))
                    {
                        if (!result.StartsWith("OK!"))
                        {
                            RunOnUiThread(() =>
                            {
                                DialogHelper.ShowDialogError(this, this, result);

                            });
                            return false;
                        }
                    }
                    else
                    {
                        RunOnUiThread(() =>
                        {
                            DialogHelper.ShowDialogError(this, this, "Napaka pri klicu web aplikacije" + result);


                        });
                        return false;
                    }
                }
                catch(Exception err)
                {
                    Crashes.TrackError(err);
                    return false;
                }
            }


            try
            {


                if (moveItem == null) { moveItem = new NameValueObject("MoveItem"); }
                moveItem.SetInt("HeadID", moveHead.GetInt("HeadID"));
                moveItem.SetString("LinkKey", openOrder.GetString("Key"));
                moveItem.SetInt("LinkNo", openOrder.GetInt("No"));
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
                var jsonobj = GetJSONforMoveItem(moveItem);
                if (moveItem == null)
                {
                    RunOnUiThread(() =>
                    {
                        var debug = error;

                        DialogHelper.ShowDialogError(this, this, "Napaka pri dostopu web aplikacije." + error);
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
            catch(Exception ex)
            {
                Crashes.TrackError(ex);
                return false;
            }
        }


        private void ProcessQty()
        {
            try
            {
                if (moveHead != null)
                {
                    if (!CommonData.IsValidLocation(moveHead.GetString("Wharehouse"), tbLocation.Text.Trim()))
                    {
                        // Toast.MakeText(this, "Lokacija '" + tbLocation.Text.Trim() + "' ni veljavna za skladišče '" + moveHead.GetString("Wharehouse") + "'!", ToastLength.Long).Show();
                        DialogHelper.ShowDialogError(this, this, "Lokacija '" + tbLocation.Text.Trim() + "' ni veljavna za skladišče '" + moveHead.GetString("Wharehouse") + "'!");
                        tbLocation.RequestFocus();
                        return;
                    }
                    if (!LoadStock(moveHead.GetString("Wharehouse"), tbLocation.Text.Trim(), tbSSCC.Text.Trim(), tbSerialNum.Text.Trim(), openIdent.GetString("Code")))
                    {
                        DialogHelper.ShowDialogError(this, this, "Zaloga za SSCC/Serijsko št. ne obstaja.");
                        return;
                    }
                    else
                    {
                        string error;
                        var qty = openOrder.GetDouble("OpenQty");
                        var key = openOrder.GetString("Key");
                        var number = openOrder.GetString("No");
                        var code = openIdent.GetString("Code");
                        var openQty = openOrder.GetDouble("OpenQty");
                        var fulfilledOrder = Services.GetObject("miho", openOrder.GetString("Key") + "|" + openOrder.GetInt("No") + "|" + openIdent.GetString("Code"), out error);
                        var fulfilledQty = fulfilledOrder == null ? 0.0 : fulfilledOrder.GetDouble("Qty");
                        tbPacking.Text = stock.GetDouble("RealStock").ToString(CommonData.GetQtyPicture());
                        lbQty.Text = "Kol. (" + (openOrder.GetDouble("OpenQty")).ToString(CommonData.GetQtyPicture()) + ")";
                    }
                }
            }
            catch (Exception ex)
            {
               Crashes.TrackError(ex);
            }
        }



        private void Button3_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(IssuedGoodsIdentEntryWithTrailTablet));
            HelpfulMethods.clearTheStack(this);
            Finish();

        }

        public void GetBarcode(string barcode)
        {


            if (tbSSCC.HasFocus)
            {
                if (barcode != "Scan fail")
                {
                    Sound();
                    tbSSCC.Text = barcode;
                    FillRelatedData(tbSSCC.Text);
                    tbSerialNum.RequestFocus();
                }
            }
            else if (tbSerialNum.HasFocus)
            {
                if (barcode != "Scan fail")
                {
                    Sound();
                    tbSerialNum.Text = barcode;
                    tbLocation.RequestFocus();
                }
            }
            else if (tbLocation.HasFocus)
            {
                if (barcode != "Scan fail")
                {
                    Sound();
                    tbLocation.Text = barcode;
                    tbPacking.RequestFocus();
                }
            }
        }

        private void Sound()
        {
            soundPool.Play(soundPoolId, 1, 1, 0, 0, 1);
        }
    }
}