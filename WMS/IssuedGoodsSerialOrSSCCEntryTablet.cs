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
using AlertDialog = Android.App.AlertDialog;
using AndroidX.AppCompat.App;
using Android.Graphics;

namespace WMS
{
    [Activity(Label = "IssuedGoodsSerialOrSSCCEntryTablet", ScreenOrientation = Android.Content.PM.ScreenOrientation.Landscape)]
    public class IssuedGoodsSerialOrSSCCEntryTablet : CustomBaseActivity, IBarcodeResult
    {
        private EditText tbIdent;
        private EditText tbSSCC;
        private EditText tbSerialNum;
        private EditText tbLocation;
        private EditText tbPacking;
        private EditText tbPalette;
        private Button btSaveOrUpdate;
        private ListView listData;
        private List<string> locations = new List<string>();
        private NameValueObject openIdent = (NameValueObject)InUseObjects.Get("OpenIdent");
        private NameValueObject openOrder = (NameValueObject)InUseObjects.Get("OpenOrder");
        private NameValueObject moveHead = (NameValueObject)InUseObjects.Get("MoveHead");
        private NameValueObject moveItem = (NameValueObject)InUseObjects.Get("MoveItem");
        private NameValueObject extraData = (NameValueObject)InUseObjects.Get("ExtraData");
        private NameValueObject lastItem = (NameValueObject)InUseObjects.Get("LastItem");
        private NameValueObjectList docTypes = null;
        private TextView lbQty;
        private bool editMode = false;
        private bool isPackaging = false;
        private TextView lbUnits;
        private TextView lbPalette;
        SoundPool soundPool;
        int soundPoolId;
        private ZoomageView imagePNG;
        private ProgressDialogClass progress;
        private List<LocationClass> items = new List<LocationClass>();
        private Button btCreateSame;
        private Button btCreate;
        private Button btFinish;
        private Button btOverview;
        private Button btExit;
        private LinearLayout ssccRow;
        private LinearLayout serialRow;
        private List<IssuedGoods> connectedPositions = new List<IssuedGoods>();
        private bool createPositionAllowed;
        private List<IssuedGoods> data;
        private double stock;
        private Dialog popupDialogConfirm;
        private Button? btnYesConfirm;
        private Button? btnNoConfirm;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetTheme(Resource.Style.AppTheme_NoActionBar);
            SetContentView(Resource.Layout.IssuedGoodsSerialOrSSCCEntryTablet);

            AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
            var _customToolbar = new CustomToolbar(this, toolbar, Resource.Id.navIcon);
            _customToolbar.SetNavigationIcon(settings.RootURL + "/Services/Logo");
            SetSupportActionBar(_customToolbar._toolbar);
            SupportActionBar.SetDisplayShowTitleEnabled(false);
            Window.SetSoftInputMode(Android.Views.SoftInput.AdjustResize);
            tbIdent = FindViewById<EditText>(Resource.Id.tbIdent);
            tbSSCC = FindViewById<EditText>(Resource.Id.tbSSCC);
            listData = FindViewById<ListView>(Resource.Id.listData);
            tbSerialNum = FindViewById<EditText>(Resource.Id.tbSerialNum);
            tbLocation = FindViewById<EditText>(Resource.Id.tbLocation);
            tbPacking = FindViewById<EditText>(Resource.Id.tbPacking);
            tbPalette = FindViewById<EditText>(Resource.Id.tbPalette);
            btSaveOrUpdate = FindViewById<Button>(Resource.Id.btSaveOrUpdate);
            lbQty = FindViewById<TextView>(Resource.Id.lbQty);
            lbUnits = FindViewById<TextView>(Resource.Id.lbUnits);
            lbPalette = FindViewById<TextView>(Resource.Id.lbPalette);
            imagePNG = FindViewById<ZoomageView>(Resource.Id.imagePNG);
            btCreateSame = FindViewById<Button>(Resource.Id.btCreateSame);
            btCreate = FindViewById<Button>(Resource.Id.btCreate);
            btFinish = FindViewById<Button>(Resource.Id.btFinish);
            btOverview = FindViewById<Button>(Resource.Id.btOverview);
            btExit = FindViewById<Button>(Resource.Id.btExit);
            ssccRow = FindViewById<LinearLayout>(Resource.Id.sscc_row);
            serialRow = FindViewById<LinearLayout>(Resource.Id.serial_row);
            // Properties
            tbIdent.InputType = Android.Text.InputTypes.ClassNumber;
            tbSSCC.InputType = Android.Text.InputTypes.ClassNumber;
            tbLocation.InputType = Android.Text.InputTypes.ClassText;
            tbPacking.InputType = Android.Text.InputTypes.ClassNumber;
            tbPalette.InputType = Android.Text.InputTypes.ClassNumber;
            // Methods
            tbSSCC.KeyPress += TbSSCC_KeyPress;
            tbSerialNum.KeyPress += TbSerialNum_KeyPress;
            btCreateSame.Click += BtCreateSame_Click;
            btCreate.Click += BtCreate_Click;
            btFinish.Click += BtFinish_Click;
            btOverview.Click += BtOverview_Click;
            btExit.Click += BtExit_Click;
            soundPool = new SoundPool(10, Stream.Music, 0);
            soundPoolId = soundPool.Load(this, Resource.Raw.beep, 1);
            Barcode2D barcode2D = new Barcode2D();
            barcode2D.open(this, this);
            imagePNG.Visibility = ViewStates.Invisible;
            tbPacking.SetSelectAllOnFocus(true);
            AdapterLocation adapter = new AdapterLocation(this, items);
            listData.Adapter = adapter;
            docTypes = CommonData.ListDocTypes("P|N");           
            var code = openIdent.GetString("Code");
            tbSSCC.FocusedByDefault = true;
            var _broadcastReceiver = new NetworkStatusBroadcastReceiver();
            _broadcastReceiver.ConnectionStatusChanged += OnNetworkStatusChanged;
            Application.Context.RegisterReceiver(_broadcastReceiver,
            new IntentFilter(ConnectivityManager.ConnectivityAction));



            // Method calls

            CheckIfApplicationStopingException();

            // Color the fields that can be scanned
            ColorFields();

            // Main logic for the entry
            SetUpForm();

            // Stop the loader
            LoaderManifest.LoaderManifestLoopStop(this);

            SetUpUpdate();
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

        private void FilterData()
        {
            data = FilterIssuedGoods(connectedPositions, tbSSCC.Text, tbSerialNum.Text, tbLocation.Text);
            if (data.Count == 1)
            {
                var element = data.ElementAt(0);

    
                stock = element.anQty ?? 0;
                lbQty.Text = $"{Resources.GetString(Resource.String.s83)} ( " + element.anQty.ToString() + " )";
                tbPacking.Text = element.anQty.ToString();
                

                tbLocation.Text = element.aclocation;
                tbLocation.Enabled = false;

                // Do stuff and allow creating the position
                createPositionAllowed = true;
                tbPacking.Text = data.ElementAt(0).anQty.ToString();

                tbPacking.SetSelection(0, tbPacking.Text.Length);
                // This flow should end up with correct data in the fields and the select focus on the qty field. 
            }
            else
            {
                lbQty.Text = $"{Resources.GetString(Resource.String.s292)}";
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

        private void SetUpUpdate()
        {
            // This method changes the UI so it shows in a visible way that it is the update screen. - 18.03.2024
            if (Base.Store.isUpdate)
            {
                btCreateSame.Visibility = ViewStates.Gone;
                btCreate.Text = $"{Resources.GetString(Resource.String.s290)}";
            }
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

            if (Base.Store.isUpdate)
            {
                // Update logic ?? it seems to be true.
                tbIdent.Text = moveItem.GetString("IdentName");
                tbSerialNum.Text = moveItem.GetString("SerialNo");
                tbSSCC.Text = moveItem.GetString("SSCC");
                tbLocation.Text = moveItem.GetString("Location");
                tbPalette.Text = moveItem.GetString("Palette");
                tbPacking.Text = moveItem.GetDouble("Qty").ToString();
                lbQty.Text = $"{Resources.GetString(Resource.String.s83)} ( " + moveItem.GetDouble("Qty").ToString() + " )";
                btCreateSame.Text = $"{Resources.GetString(Resource.String.s293)}";
                // Lock down all other fields
                tbIdent.Enabled = false;
                tbSerialNum.Enabled = false;
                tbSSCC.Enabled = false;
                tbLocation.Enabled = false;
                tbPalette.Enabled = false;
            }
            else
            {
                tbIdent.Text = openIdent.GetString("Code") + " " + openIdent.GetString("Name");
                // This flow is for idents.
                var order = Base.Store.OpenOrder;
                GetConnectedPositions(order.Order, order.Position ?? -1, order.Ident);                
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
            // Test this function based on the proccess
        }



        private void GetConnectedPositions(string acKey, int anNo, string acIdent, string acLocation = null)
        {
            var sql = "SELECT * from uWMSOrderItemByKeyOut WHERE acKey = @acKey AND anNo = @anNo AND acIdent = @acIdent";
            var parameters = new List<Services.Parameter>();
            parameters.Add(new Services.Parameter { Name = "acKey", Type = "String", Value = acKey });
            parameters.Add(new Services.Parameter { Name = "anNo", Type = "Int32", Value = anNo });
            parameters.Add(new Services.Parameter { Name = "acIdent", Type = "String", Value = acIdent });
            if (acLocation != null)
            {
                parameters.Add(new Services.Parameter { Name = "acLocation", Type = "String", Value = acLocation });
                sql += " AND acLocation = @acLocation;";
            }
            var subjects = Services.GetObjectListBySql(sql, parameters);
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
                            aclocation = row.StringValue("aclocation"),
                            anNo = (int)(row.IntValue("anNo") ?? -1),
                            acKey = row.StringValue("acKey"),
                            acIdent = row.StringValue("acIdent")
                        });
                    }
                }
            }
        }
        private void ColorFields()
        {
            tbSSCC.SetBackgroundColor(Android.Graphics.Color.Aqua);
            tbSerialNum.SetBackgroundColor(Android.Graphics.Color.Aqua);
            tbLocation.SetBackgroundColor(Android.Graphics.Color.Aqua);
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
                StartActivity(typeof(MainMenuTablet));
            }
        }

        private void BtExit_Click(object? sender, EventArgs e)
        {
            StartActivity(typeof(MainMenuTablet));

        }

        private void BtOverview_Click(object? sender, EventArgs e)
        {
            StartActivity(typeof(IssuedGoodsEnteredPositionsViewTablet));
            Finish();
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


        private async void BtnYesConfirm_Click(object sender, EventArgs e)
        {
            await FinishMethod();
        }


        private void BtnNoConfirm_Click(object sender, EventArgs e)
        {
            popupDialogConfirm.Dismiss();
            popupDialogConfirm.Hide();
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
                                    StartActivity(typeof(IssuedGoodsBusinessEventSetupTablet));
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
                                    StartActivity(typeof(MainMenuTablet));
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
        private async Task CreateMethodFromStart()
        {
            await Task.Run(() =>
            {
                if (data.Count == 1)
                {
                    var element = data.ElementAt(0);
                    moveItem = new NameValueObject("MoveItem");
                    moveItem.SetInt("HeadID", moveHead.GetInt("HeadID"));
                    moveItem.SetString("LinkKey", element.acKey);
                    moveItem.SetInt("LinkNo", element.anNo);
                    moveItem.SetString("Ident", openIdent.GetString("Code"));
                    moveItem.SetString("SSCC", tbSSCC.Text.Trim());
                    moveItem.SetString("SerialNo", tbSerialNum.Text.Trim());
                    moveItem.SetDouble("Packing", Convert.ToDouble(tbPacking.Text.Trim()));
                    moveItem.SetDouble("Factor", 1);
                    moveItem.SetDouble("Qty", Convert.ToDouble(tbPacking.Text.Trim()));
                    moveItem.SetInt("Clerk", Services.UserID());
                    moveItem.SetString("Location", tbLocation.Text.Trim());
                    moveItem.SetString("Palette", tbPalette.Text.Trim());

                    string error;

                    moveItem = Services.SetObject("mi", moveItem, out error);

                    if (moveItem != null && error == string.Empty)
                    {
                        RunOnUiThread(() =>
                        {
                            if (Base.Store.modeIssuing == 2)
                            {
                                StartActivity(typeof(IssuedGoodsIdentEntryWithTrailTablet));
                                Finish();
                            } else if (Base.Store.modeIssuing == 1)
                            {
                                StartActivity(typeof(IssuedGoodsIdentEntryTablet));
                                Finish();
                            }
                        });

                        createPositionAllowed = false;
                        GetConnectedPositions(element.acKey, element.anNo, element.acIdent, element.aclocation);
                    }
                }
                else
                {
                    return;
                }
            });
        }

        private async void BtCreate_Click(object? sender, EventArgs e)
        {
            if (tbSSCC.HasFocus || tbSerialNum.HasFocus)
            {
                FilterData();
            }

            if (!Base.Store.isUpdate)
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
            else
            {
                // Update flow.
                double newQty;
                if (Double.TryParse(tbPacking.Text, out newQty))
                {
                    if (newQty > moveItem.GetDouble("Qty"))
                    {
                        Toast.MakeText(this, $"{Resources.GetString(Resource.String.s291)}", ToastLength.Long).Show();
                    }
                    else
                    {
                        var parameters = new List<Services.Parameter>();
                        var tt = moveItem.GetInt("ItemID");
                        parameters.Add(new Services.Parameter { Name = "anQty", Type = "Decimal", Value = newQty });
                        parameters.Add(new Services.Parameter { Name = "anItemID", Type = "Int32", Value = moveItem.GetInt("ItemID") });
                        string debugString = $"UPDATE uWMSMoveItem SET anQty = {newQty} WHERE anIDItem = {moveItem.GetInt("ItemID")}";
                        var subjects = Services.Update($"UPDATE uWMSMoveItem SET anQty = @anQty WHERE anIDItem = @anItemID;", parameters);
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
                            StartActivity(typeof(IssuedGoodsEnteredPositionsViewTablet));
                            Finish();
                        }
                    }
                }
                else
                {
                    Toast.MakeText(this, $"{Resources.GetString(Resource.String.s270)}", ToastLength.Long).Show();
                }
            }
        }
        private async Task CreateMethodSame()
        {
            await Task.Run(() =>
            {
                if (data.Count == 1)
                {
                    var element = data.ElementAt(0);
                    // This solves the problem of updating the item. The problem occurs because of the old way of passing data.
                    moveItem = new NameValueObject("MoveItem");
                    moveItem.SetInt("HeadID", moveHead.GetInt("HeadID"));
                    moveItem.SetString("LinkKey", element.acKey);
                    moveItem.SetInt("LinkNo", element.anNo);
                    moveItem.SetString("Ident", openIdent.GetString("Code"));
                    moveItem.SetString("SSCC", tbSSCC.Text.Trim());
                    moveItem.SetString("SerialNo", tbSerialNum.Text.Trim());
                    moveItem.SetDouble("Packing", Convert.ToDouble(tbPacking.Text.Trim()));
                    moveItem.SetDouble("Factor", 1);
                    moveItem.SetDouble("Qty", Convert.ToDouble(tbPacking.Text.Trim()));
                    moveItem.SetInt("Clerk", Services.UserID());
                    moveItem.SetString("Location", tbLocation.Text.Trim());
                    moveItem.SetString("Palette", tbPalette.Text.Trim());
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

                            tbLocation.Text = string.Empty;
                            tbPacking.Text = string.Empty;
                            lbQty.Text = $"{Resources.GetString(Resource.String.s292)}";

                        });

                        createPositionAllowed = false;
                        GetConnectedPositions(element.acKey, element.anNo, element.acIdent, element.aclocation);
                    }
                }
                else
                {
                    return;
                }
            });
        }


        private async void BtCreateSame_Click(object? sender, EventArgs e)
        {
            if (tbSSCC.HasFocus || tbSerialNum.HasFocus)
            {
                FilterData();
            }

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

 
        public void GetBarcode(string barcode)
        {

            if (tbSSCC.HasFocus)
            {
                if (barcode != "Scan fail")
                {
                    Sound();
                }
            }
            else if (tbSerialNum.HasFocus)
            {
                if (barcode != "Scan fail")
                {
                    Sound();
                }
            }
            else if (tbLocation.HasFocus)
            {
                if (barcode != "Scan fail")
                {
                    Sound();
                }
            }
        }

        private void Sound()
        {
            soundPool.Play(soundPoolId, 1, 1, 0, 0, 1);
        }
    }
}