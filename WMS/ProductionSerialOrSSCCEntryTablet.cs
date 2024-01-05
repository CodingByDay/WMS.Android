using Stream = Android.Media.Stream;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Android.App;
using Android.Content;
using Android.Graphics.Drawables;
using Android.Media;
using Android.Net;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using BarCode2D_Receiver;
using Com.Jsibbold.Zoomage;
using Microsoft.AppCenter.Crashes;
using Newtonsoft.Json;
using WMS.App;
using TrendNET.WMS.Core.Data;
using TrendNET.WMS.Device.App;
using TrendNET.WMS.Device.Services;
using static Android.App.ActionBar;
using WebApp = TrendNET.WMS.Device.Services.WebApp;

using AndroidX.AppCompat.App;using AlertDialog = Android.App.AlertDialog;namespace WMS
{
    [Activity(Label = "ProductionSerialOrSSCCEntryTablet", ScreenOrientation = Android.Content.PM.ScreenOrientation.Landscape)]
    public class ProductionSerialOrSSCCEntryTablet : AppCompatActivity, IBarcodeResult
    {
        private NameValueObject moveHead = (NameValueObject)InUseObjects.Get("MoveHead");
        private NameValueObject moveItem = (NameValueObject)InUseObjects.Get("MoveItem");
        private NameValueObject openWorkOrder = null;
        private bool editMode = false;
        private EditText tbIdent;
        private EditText tbSSCC;
        private EditText tbSerialNum;
        private EditText tbLocation;
        private EditText tbPacking;
        private EditText tbUnits;
        private Spinner spLocation;
        private TextView lbQty;
        private Button btSaveOrUpdate;
        private Button button3;
        private Button button4;
        private Button button5;
        SoundPool soundPool;
        private ListView listData;
        
        private ImageView imagePNG;
        private List<string> locations = new List<string>();

        private List<ProductionSerialOrSSCCList> data = new List<ProductionSerialOrSSCCList>();
        int soundPoolId;
        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            switch (keyCode)
            {

                case Keycode.F2:
                    if (btSaveOrUpdate.Enabled == true)
                    {
                        BtSaveOrUpdate_Click(this, null);
                    }
                    break;

                case Keycode.F3:
                    if (button3.Enabled == true)
                    {
                        Button3_Click(this, null);
                    }
                    break;

                case Keycode.F4://
                    if (button4.Enabled == true)
                    {
                        Button4_Click(this, null);
                    }
                    break;

                case Keycode.F8:
                    if (button5.Enabled == true)
                    {
                        Button5_Click(this, null);
                    }
                    break;

            }
            return base.OnKeyDown(keyCode, e);
        }
        public void GetBarcode(string barcode)
        {

            if (tbSSCC.HasFocus)
            {
                Sound();
                tbSSCC.Text = barcode;
                tbSerialNum.RequestFocus();
            }
            else if (tbSerialNum.HasFocus)
            {
                Sound();
                tbSerialNum.Text = barcode;
                ProcessSerialNum();
                tbLocation.RequestFocus();
            }
            else if (tbLocation.HasFocus)
            {
                Sound();
                tbLocation.Text = barcode;
            }
        }
        private static bool? checkWorkOrderOpenQty = null;

        private void Sound()
        {
            soundPool.Play(soundPoolId, 1, 1, 0, 0, 1);
        }


        private static bool? getWorkOrderDefaultQty = null;
        private string identCode;
        private ProgressDialogClass progress;
        private Dialog popupDialog;
        private ZoomageView image;
        private Button btnOK;
        private Dialog popupDialogConfirm;
        private Button btnYesConfirm;
        private Button btnNoConfirm;

        private void GetWorkOrderDefaultQty()
        {
            if (getWorkOrderDefaultQty == null)
            {

                try
                {
                    string error;
                    var useObj = Services.GetObject("wodqUse", "", out error);
                    getWorkOrderDefaultQty = useObj == null ? false : useObj.GetBool("Use");
                }
                catch (Exception err)
                {

                    Crashes.TrackError(err);
                    return;

                }
            }

            if ((bool)getWorkOrderDefaultQty)
            {


                try
                {
                    string error;
                    var qtyObj = Services.GetObject("wodq", openWorkOrder.GetString("Key") + "|" + openWorkOrder.GetString("Ident"), out error);
                    if (qtyObj != null)
                    {
                        var qty = qtyObj.GetDouble("DefaultQty");
                        if (qty < 0)
                        {
                            getWorkOrderDefaultQty = false;
                        }
                        else
                        {
                            tbPacking.Text = qty.ToString(CommonData.GetQtyPicture());
                        }
                    }
                }
                catch (Exception err)
                {

                    Crashes.TrackError(err);
                    return;

                }
            }
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
                    string SuccessMessage = string.Format("SSCC koda je obvezen podatek.");
                    DialogHelper.ShowDialogError(this, this, SuccessMessage);
                    tbSSCC.RequestFocus();
                });

                return false;
            }

            if (tbSerialNum.Enabled && string.IsNullOrEmpty(tbSerialNum.Text.Trim()))
            {
                tbSerialNum.Text = GetNextSerialNum();
                if (string.IsNullOrEmpty(tbSerialNum.Text.Trim()))
                {
                    RunOnUiThread(() =>
                    {
                        string SuccessMessage = string.Format("Ni mogoče pridobiti serijske št.");
                        DialogHelper.ShowDialogError(this, this, SuccessMessage);
                        tbSerialNum.RequestFocus();
                    });

                    return false;
                }
            }

            if (!CommonData.IsValidLocation(moveHead.GetString("Wharehouse"), tbLocation.Text.Trim()))
            {
                RunOnUiThread(() =>
                {
                    string SuccessMessage = string.Format("Lokacija '" + tbLocation.Text.Trim() + "' ni veljavna za skladišče '" + moveHead.GetString("Wharehouse") + "'!");
                    DialogHelper.ShowDialogError(this, this, SuccessMessage);
                    tbLocation.RequestFocus();
                });

                return false;
            }

            string error;
            try
            {


                if (tbSSCC.Enabled)
                {
                    var stock = Services.GetObject("sts", tbSSCC.Text.Trim(), out error);
                    if (stock == null)
                    {
                        RunOnUiThread(() =>
                        {
                            string SuccessMessage = string.Format("Napaka pri dostopu do web aplikacije" + error);
                            DialogHelper.ShowDialogError(this, this, SuccessMessage);
                        });


                        return false;
                    }

                    if (stock.GetBool("ExistsSSCC"))
                    {
                        RunOnUiThread(() =>
                        {
                            string SuccessMessage = string.Format("SSCC koda že obstaja");
                            DialogHelper.ShowDialogError(this, this, SuccessMessage);
                        });


                        return false;
                    }
                }
                if (string.IsNullOrEmpty(tbPacking.Text.Trim()))
                {
                    RunOnUiThread(() =>
                    {
                        string SuccessMessage = string.Format("Količina je obvezan podatek");
                        DialogHelper.ShowDialogError(this, this, SuccessMessage);
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
                                string SuccessMessage = string.Format("Količina je obvezen podatek in mora bit različna od nič.");
                                DialogHelper.ShowDialogError(this, this, SuccessMessage);
                            });


                            return false;
                        }

                        if (CheckWorkOrderOpenQty())
                        {
                            var max = Math.Abs(openWorkOrder.GetDouble("OpenQty"));
                            if (Math.Abs(qty) > max)
                            {
                                RunOnUiThread(() =>
                                {
                                    string SuccessMessage = string.Format("Količina (" + qty.ToString(CommonData.GetQtyPicture()) + ") ne sme presegati max. količine (" + max.ToString(CommonData.GetQtyPicture()) + ")!");
                                    DialogHelper.ShowDialogError(this, this, SuccessMessage);
                                    tbPacking.RequestFocus();
                                });

                                return false;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        RunOnUiThread(() =>
                        {
                            string SuccessMessage = string.Format("Količina mora biti število (" + e.Message + ")!");
                            DialogHelper.ShowDialogError(this, this, SuccessMessage);

                            tbPacking.RequestFocus();
                        });

                        return false;
                    }
                }

                if (string.IsNullOrEmpty(tbUnits.Text.Trim()))
                {
                    RunOnUiThread(() =>
                    {
                        string SuccessMessage = string.Format("Št. enota je obvezen podatek!");
                        DialogHelper.ShowDialogError(this, this, SuccessMessage);
                        tbUnits.RequestFocus();
                    });


                    return false;
                }
                else
                {
                    try
                    {
                        var qty = Convert.ToDouble(tbUnits.Text.Trim());
                        if (qty == 0.0)
                        {
                            RunOnUiThread(() =>
                            {
                                string SuccessMessage = string.Format("Št. enota je obvezen podatek, in more biti različit od nič.");
                                DialogHelper.ShowDialogError(this, this, SuccessMessage);
                                tbUnits.RequestFocus();
                            });

                            return false;
                        }
                    }
                    catch (Exception e)
                    {
                        RunOnUiThread(() =>
                        {
                            string SuccessMessage = string.Format("Št. enot mora biti število (" + e.Message + ")!");
                            DialogHelper.ShowDialogError(this, this, SuccessMessage);
                            tbUnits.RequestFocus();
                        });

                        return false;
                    }
                }

                if (moveItem == null) { moveItem = new NameValueObject("MoveItem"); }
                moveItem.SetInt("HeadID", moveHead.GetInt("HeadID"));
                moveItem.SetString("LinkKey", openWorkOrder.GetString("Key"));
                moveItem.SetInt("LinkNo", 0);
                moveItem.SetString("Ident", openWorkOrder.GetString("Ident"));
                moveItem.SetString("SSCC", tbSSCC.Text.Trim());
                moveItem.SetString("SerialNo", tbSerialNum.Text.Trim());
                moveItem.SetDouble("Packing", Convert.ToDouble(tbPacking.Text.Trim()));
                moveItem.SetDouble("Factor", Convert.ToDouble(tbUnits.Text.Trim()));
                moveItem.SetDouble("Qty", Convert.ToDouble(tbPacking.Text.Trim()) * Convert.ToDouble(tbUnits.Text.Trim()));
                moveItem.SetString("Location", tbLocation.Text.Trim());
                moveItem.SetInt("Clerk", Services.UserID());

                moveItem = Services.SetObject("mi", moveItem, out error);

                var test = GetJSONforMoveItem(moveItem);
                if (moveItem == null)
                {
                    RunOnUiThread(() =>
                    {
                        string SuccessMessage = string.Format("Napaka pri dostopu do web aplikacije: " + error);
                        DialogHelper.ShowDialogError(this, this, SuccessMessage);
                    });


                    return false;
                }
                else
                {
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

            var jsonString = JsonConvert.SerializeObject(item);


            return jsonString;
        }
        private void ProcessSerialNum()
        {
            if (string.IsNullOrEmpty(tbSerialNum.Text.Trim()))
            {
                tbSerialNum.Text = GetNextSerialNum();
                if (string.IsNullOrEmpty(tbSerialNum.Text.Trim()))
                {

                    tbSerialNum.RequestFocus();
                    return;
                }
            }
            GetWorkOrderDefaultQty();
          
        }
        private bool CheckWorkOrderOpenQty()
        {
            if (checkWorkOrderOpenQty == null)
            {
                try
                {
                    string error;
                    var useObj = Services.GetObject("cwooqUse", "", out error);
                    checkWorkOrderOpenQty = useObj == null ? false : useObj.GetBool("Use");
                }
                catch (Exception err)
                {
                    Crashes.TrackError(err);
                    return false;
                }
            }
            return (bool)checkWorkOrderOpenQty;
        }

        private string GetNextSerialNum()
        {

            try
            {

                string error;
                var ident = openWorkOrder.GetString("Ident");

                var workOrder = openWorkOrder.GetString("Key");
                var serNumObj = Services.GetObject("sn", ident + "|" + workOrder, out error);
                if (serNumObj != null)
                {
                    return serNumObj.GetString("SerialNo");
                }
                else
                {
                    return "";
                }
            }
            catch (Exception err)
            {

                Crashes.TrackError(err);
                return "";

            }
        }
        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            
            SetContentView(Resource.Layout.ProductionSerialOrSSCCEntryTablet);
            AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
            var _customToolbar = new CustomToolbar(this, toolbar, Resource.Id.navIcon);
            _customToolbar.SetNavigationIcon(settings.RootURL + "/Services/Logo");
            tbIdent = FindViewById<EditText>(Resource.Id.tbIdent);
            spLocation = FindViewById<Spinner>(Resource.Id.spLocation);
            tbSSCC = FindViewById<EditText>(Resource.Id.tbSSCC);
            tbSerialNum = FindViewById<EditText>(Resource.Id.tbSerialNum);
            tbLocation = FindViewById<EditText>(Resource.Id.tbLocation);
            tbPacking = FindViewById<EditText>(Resource.Id.tbPacking);
            tbUnits = FindViewById<EditText>(Resource.Id.tbUnits);
            tbIdent.InputType = Android.Text.InputTypes.ClassNumber;
            tbSSCC.InputType = Android.Text.InputTypes.ClassNumber;
            tbSerialNum.InputType = Android.Text.InputTypes.ClassNumber;
            tbLocation.InputType = Android.Text.InputTypes.ClassNumber;
            tbUnits.InputType = Android.Text.InputTypes.ClassNumber;
            lbQty = FindViewById<TextView>(Resource.Id.lbQty);
            btSaveOrUpdate = FindViewById<Button>(Resource.Id.btSaveOrUpdate);
            button3 = FindViewById<Button>(Resource.Id.button3);
            button4 = FindViewById<Button>(Resource.Id.button4);
            button5 = FindViewById<Button>(Resource.Id.button5);
            listData = FindViewById<ListView>(Resource.Id.listData);
            ProductionSerialOrSSCCAdapter adapter = new ProductionSerialOrSSCCAdapter(this, data);
            listData.Adapter = adapter;
            listData.ItemClick += ListData_ItemClick;
            soundPool = new SoundPool(10, Stream.Music, 0);
            soundPoolId = soundPool.Load(this, Resource.Raw.beep, 1);
            color();
            imagePNG = FindViewById<ImageView>(Resource.Id.imagePNG);
            tbSSCC.RequestFocus();
            btSaveOrUpdate.Click += BtSaveOrUpdate_Click;
            button3.Click += Button3_Click;
            button4.Click += Button4_Click;
            button5.Click += Button5_Click;
            spLocation.ItemSelected += SpLocation_ItemSelected;
            tbSSCC.LongClick += TbSSCC_LongClick;
            
            Barcode2D barcode2D = new Barcode2D();
            barcode2D.open(this, this);
            tbSSCC.FocusChange += TbSSCC_FocusChange;
            try
            {
                string SuccessMessage = string.Format("Preverjam povezovani DN");
                Toast.MakeText(this, SuccessMessage, ToastLength.Long).Show();
                var key = moveHead.GetString("LinkKey");
                string error;
                openWorkOrder = Services.GetObject("wo", key, out error);
                if (openWorkOrder == null) { throw new ApplicationException("Neveljaven povezan dokument: " + key); }
                lbQty.Text = "Količina (" + openWorkOrder.GetDouble("OpenQty").ToString(CommonData.GetQtyPicture()) + ")";
            }
            catch (Exception err)
            {

                Crashes.TrackError(err);

            }

            editMode = moveItem != null;
            if(editMode)
            {
                tbSSCC.Text = moveItem.GetString("SSCC");
                tbSerialNum.Text = moveItem.GetString("SerialNo");
                tbPacking.Text = moveItem.GetDouble("Packing").ToString(CommonData.GetQtyPicture());
                tbUnits.Text = moveItem.GetDouble("Factor").ToString("###,###,##0.00");
                tbPacking.RequestFocus();
            }
            var ident = CommonData.LoadIdent(openWorkOrder.GetString("Ident"));
          
            tbIdent.Text = ident.GetString("Code") + " " + ident.GetString("Name");
            identCode = ident.GetString("Code");
            tbSSCC.Enabled = ident.GetBool("isSSCC");
            tbSerialNum.Enabled = ident.GetBool("HasSerialNumber");
            fillItems();
            await GetLocationsForGivenWarehouse(moveHead.GetString("Wharehouse"));
            var adapterReceive = new CustomAutoCompleteAdapter<String>(this, Android.Resource.Layout.SimpleSpinnerItem, locations);
            adapterReceive.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            
            spLocation.Adapter = adapterReceive;
            spLocation.SetSelection(locations.IndexOf("P01"), true);
            showPictureIdent(ident.GetString("Code"));

            if (String.IsNullOrEmpty(tbUnits.Text)) { tbUnits.Text = "1"; }




            tbLocation.InputType = Android.Text.InputTypes.ClassText;
            tbPacking.InputType = 
                       Android.Text.InputTypes.ClassNumber;

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

        private void TbSSCC_LongClick(object sender, View.LongClickEventArgs e)
        {
            tbSSCC.Text = "";
            tbSerialNum.Text = "";
            tbPacking.Text = "";
            tbLocation.Text = "";
            tbIdent.Text = "";
        }

        private void SpLocation_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            var element = e.Position;
            var item = locations.ElementAt(element);
            tbLocation.Text = item;
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

        private void BtnOK_Click(object sender, EventArgs e)
        {
            popupDialog.Dismiss();
            popupDialog.Hide();
        }
        private void TbSSCC_FocusChange(object sender, View.FocusChangeEventArgs e)
        {
            var warehouse = moveHead.GetString("Wharehouse");

            fillSugestedLocation(warehouse);         
        }

        private void fillSugestedLocation(string warehouse)
        {
            var ident = openWorkOrder.GetString("Ident");
            string result;
            if (WebApp.Get("mode=bestLoc&wh=" + warehouse + "&ident=" + HttpUtility.UrlEncode(ident) + "&locMode=incomming", out result))
            {
                var test = result;
                if (test != "Exception: The remote server returned an error: (404) Not Found.")
                {
                    if (!String.IsNullOrEmpty(result))
                    {
                        tbLocation.Text = result;
                    }
                }
                else
                {
                   
                }
            }

        }

        private void ListData_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            var selected = e.Position;
            var item = data.ElementAt(selected);
            tbLocation.Text = item.Location;

        }
        private async Task FinishMethod()
        {
            await Task.Run(() =>
            {
                var resultAsync = SaveMoveItem().Result;
                if (resultAsync)
                {
                    var headID = moveHead.GetInt("HeadID");
                    //
                    SelectSubjectBeforeFinish.ShowIfNeeded(headID);

                    RunOnUiThread(() =>
                    {
                        progress = new ProgressDialogClass();

                        progress.ShowDialogSync(this, "Zaključujem");
                    });
                    try
                    {

                        string result;
                        if (WebApp.Get("mode=finish&stock=add&print=" + Services.DeviceUser() + "&id=" + headID.ToString(), out result))
                        {
                            if (result.StartsWith("OK!"))
                            {
                                RunOnUiThread(() =>
                                {
                                    progress.StopDialogSync();
                                    var id = result.Split('+')[1];

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

                            RunOnUiThread(() =>
                            {
                                progress.StopDialogSync();
                                AlertDialog.Builder alert = new AlertDialog.Builder(this);
                                alert.SetTitle("Napaka");
                                alert.SetMessage("Napaka pri klicu web aplikacije: " + result);

                                alert.SetPositiveButton("Ok", (senderAlert, args) =>
                                {
                                    alert.Dispose();

                                });

                                Dialog dialog = alert.Create();
                                dialog.Show();
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
                }
            });

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
                    Toast.MakeText(this, "Prišlo je do napake", ToastLength.Long).Show();

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


            private void fillItems()
        {

            string error;
            var stock = Services.GetObjectList("str", out error, moveHead.GetString("Wharehouse") + "||" + identCode); /* Defined at the beggining of the activity. */
            var number = stock.Items.Count();
         

            if (stock != null)
            {
                stock.Items.ForEach(x =>
                {
                    data.Add(new ProductionSerialOrSSCCList
                    {
                        Ident = x.GetString("Ident"),
                        Location = x.GetString("Location"),
                        Qty = x.GetDouble("RealStock").ToString(CommonData.GetQtyPicture()),
                        SerialNumber = x.GetString("SerialNo")

                    });
                });

            }



        }
        private void color()
        {

            tbSSCC.SetBackgroundColor(Android.Graphics.Color.Aqua);
            tbSerialNum.SetBackgroundColor(Android.Graphics.Color.Aqua);
            tbLocation.SetBackgroundColor(Android.Graphics.Color.Aqua);
        }


        private void Button5_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(MainMenuTablet));
            HelpfulMethods.clearTheStack(this);
        }

        private async void Button4_Click(object sender, EventArgs e)
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

        private void Button3_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(ProductionEnteredPositionsViewTablet));
            HelpfulMethods.clearTheStack(this);
        }

        private void BtSaveOrUpdate_Click(object sender, EventArgs e)
        {
            if (SaveMoveItem().Result)
            {
                // There is something missing here.
                if (editMode)
                {
                    StartActivity(typeof(ProductionEnteredPositionsViewTablet));
                    HelpfulMethods.clearTheStack(this);
                }
                else
                {
                    StartActivity(typeof(ProductionSerialOrSSCCEntryTablet));
                    HelpfulMethods.clearTheStack(this);
                }
            }
        }
    }
}