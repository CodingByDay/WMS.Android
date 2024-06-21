using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Media;
using Android.Net;
using Android.Views;
using BarCode2D_Receiver;
using TrendNET.WMS.Core.Data;
using TrendNET.WMS.Device.App;
using TrendNET.WMS.Device.Services;
using WMS.App;
using static Android.App.ActionBar;
using AlertDialog = Android.App.AlertDialog;



namespace WMS
{
    [Activity(Label = "ProductionPalette", ScreenOrientation = ScreenOrientation.Portrait)]
    public class ProductionPalette : CustomBaseActivity, IBarcodeResult

    {
        private EditText tbCard;
        private EditText tbWorkOrder;
        private EditText tbIdent;
        private EditText tbSSCC;
        private EditText tbSerialNum;
        private bool initial = false;
        private ListView lvCardList;
        SoundPool soundPool;
        int soundPoolId;
        private Barcode2D barcode2D;
        private Button btConfirm;
        private Button button2;
        private NameValueObject cardInfo = (NameValueObject)InUseObjects.Get("CardInfo");
        private double totalQty = 0.0;
        private List<ListViewItem> listItems = new List<ListViewItem>();
        private Dialog popupDialog;
        private Button btnYes;
        private Button btnNo;
        private TextView lbTotalQty;
        private EditText tbLegCode;
        private LinearLayout legLayout;
        private bool result;
        private bool target;
        private string stKartona;
        private double collectiveAmount;

        public void GetBarcode(string barcode)
        {
            if (tbSerialNum.HasFocus)
            {
                if (barcode != "Scan fail")
                {

                    tbSerialNum.Text = barcode;
                    ProcessSerialNum();
                    tbCard.RequestFocus();

                }
                else
                {
                    tbSerialNum.Text = "";
                }

            }
            else if (tbCard.HasFocus)
            {
                if (barcode != "")
                {
                    if (barcode != "Scan fail")
                    {

                        ProcessCard(barcode);
                    }
                }
            }
            else if (tbLegCode.HasFocus)
            {

                tbLegCode.Text = barcode;
            }
        }

        private void Move(ListViewItem ivis, double qty, double totalQty)
        {
            popupDialog = new Dialog(this);
            popupDialog.SetContentView(Resource.Layout.TransportPopup);
            popupDialog.Window.SetSoftInputMode(SoftInput.AdjustResize);
            popupDialog.Show();
            popupDialog.Window.SetLayout(LayoutParams.MatchParent, LayoutParams.WrapContent);
            popupDialog.Window.SetBackgroundDrawable(new ColorDrawable(Color.ParseColor("#081a45")));
            btnYes = popupDialog.FindViewById<Button>(Resource.Id.btnYes);
            btnNo = popupDialog.FindViewById<Button>(Resource.Id.btnNo);
            btnNo.Click += BtnNo_Click1;
            btnYes.Click += (e, ev) => { BtnYes_Click(ivis, qty); };
        }

        private void BtnYes_Click(ListViewItem ivis, double qty)
        {
            var ivi = new ListViewItem { stKartona = stKartona, quantity = qty.ToString("###,###,##0.00") };
            listItems.Add(ivi);
            lvCardList.Adapter = null;
            AdapterListViewItem adapter = new AdapterListViewItem(this, listItems);
            lvCardList.Adapter = adapter;
            totalQty += qty;
            btConfirm.Enabled = true;
            lbTotalQty.Text = $"{Resources.GetString(Resource.String.s304)}: {totalQty.ToString("###,###,##0.00")} / {collectiveAmount.ToString("###,###,##0.00")}";
            popupDialog.Dismiss();
            popupDialog.Cancel();
        }



        private void BtnNo_Click1(object sender, EventArgs e)
        {
            popupDialog.Dismiss();
            popupDialog.Cancel();
        }

        private void color()
        {
            tbSerialNum.SetBackgroundColor(Android.Graphics.Color.Aqua);
            tbCard.SetBackgroundColor(Android.Graphics.Color.Aqua);
            tbLegCode.SetBackgroundColor(Android.Graphics.Color.Aqua);
        }

        private void ProcessSerialNum()
        {
            try
            {
                string error;
                var cardObj = Services.GetObject("cq", tbSerialNum.Text + "|1|" + tbIdent.Text, out error);
                if (cardObj == null)
                {
                    string WebError = string.Format($"{Resources.GetString(Resource.String.s216)}" + error);
                    Toast.MakeText(this, WebError, ToastLength.Long).Show();
                    return;
                }

                var qty = cardObj.GetDouble("Qty");
                if (qty > 0)
                {
                    tbSerialNum.Enabled = false;
                    lvCardList.Enabled = true;
                }
                else
                {
                    string WebError = string.Format($"{Resources.GetString(Resource.String.s305)}");
                    Toast.MakeText(this, WebError, ToastLength.Long).Show();

                    return;
                }
            }
            catch (Exception err)
            {

                SentrySdk.CaptureException(err);
                return;

            }
        }


        private IEnumerable<int> ScannedCardNumbers()
        {
            foreach (ListViewItem lvi in listItems)
            {
                yield return Convert.ToInt32(lvi.stKartona);
            }
        }



        private void ProcessCard(string data)
        {
            try
            {
                if (data.Length > tbSerialNum.Text.Length)
                {
                    try
                    {
                        stKartona = Convert.ToInt32(data.Substring(tbSerialNum.Text.Length)).ToString();

                    }
                    catch (Exception)
                    {
                        Toast.MakeText(this, $"{Resources.GetString(Resource.String.s265)}", ToastLength.Long).Show();
                    }
                }
                else { stKartona = Convert.ToInt32(tbCard.Text).ToString(); }

                if (stKartona != null)
                {
                    var next = true;
                    if (!data.StartsWith(tbSerialNum.Text) && tbSerialNum.Text.Length < tbCard.Text.Length)
                    {

                        string WebError = string.Format($"{Resources.GetString(Resource.String.s306)}");
                        Toast.MakeText(this, WebError, ToastLength.Long).Show();
                    }
                    else
                    {

                        foreach (ListViewItem existing in listItems)
                        {
                            if (existing.stKartona == stKartona)
                            {
                                string WebError = string.Format($"{Resources.GetString(Resource.String.s307)}");
                                Toast.MakeText(this, WebError, ToastLength.Long).Show();

                                return;
                            }
                        }
                        try
                        {
                            string error;

                            var cardObj = Services.GetObject("cq", tbSerialNum.Text + "|" + stKartona + "|" + tbIdent.Text, out error);

                            if (cardObj == null)
                            {
                                string WebError = string.Format($"{Resources.GetString(Resource.String.s216)}" + error);
                                Toast.MakeText(this, WebError, ToastLength.Long).Show();
                                return;
                            }

                            var qty = cardObj.GetDouble("Qty");

                            if (qty > 0.0)
                            {
                                if (cardObj.GetInt("IDHead") > 0)
                                {

                                    var ivis = new ListViewItem { stKartona = stKartona, quantity = qty.ToString("###,###,##0.00") };

                                    Move(ivis, qty, totalQty);



                                }
                                else
                                {

                                    var ivi = new ListViewItem { stKartona = stKartona, quantity = qty.ToString("###,###,##0.00") };
                                    listItems.Add(ivi);
                                    lvCardList.Adapter = null;
                                    AdapterListViewItem adapter = new AdapterListViewItem(this, listItems);
                                    lvCardList.Adapter = adapter;
                                    totalQty += qty;


                                    lbTotalQty.Text = $"{Resources.GetString(Resource.String.s304)}: {totalQty.ToString("###,###,##0.00")} / {collectiveAmount.ToString("###,###,##0.00")}";

                                    btConfirm.Enabled = true;
                                }
                            }

                            else
                            {
                                string WebError = string.Format($"{Resources.GetString(Resource.String.s312)}" + data);
                                Toast.MakeText(this, WebError, ToastLength.Long).Show();
                                return;
                            }
                        }
                        finally
                        {
                            tbCard.Text = "";
                        }
                    }
                }
                else
                {
                    Toast.MakeText(this, $"{Resources.GetString(Resource.String.s270)}", ToastLength.Long).Show();
                }
            }
            catch (Exception ex)           
            {
                SentrySdk.CaptureException(ex);
                return; 
            
            }
        }

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetTheme(Resource.Style.AppTheme_NoActionBar);
            if (App.Settings.tablet)
            {
                base.RequestedOrientation = ScreenOrientation.Landscape;
                base.SetContentView(Resource.Layout.ProductionPaletteTablet);
            }
            else
            {
                base.RequestedOrientation = ScreenOrientation.Portrait;
                base.SetContentView(Resource.Layout.ProductionPalette);
            }
            AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
            var _customToolbar = new CustomToolbar(this, toolbar, Resource.Id.navIcon);
            _customToolbar.SetNavigationIcon(App.Settings.RootURL + "/Services/Logo");
            SetSupportActionBar(_customToolbar._toolbar);
            SupportActionBar.SetDisplayShowTitleEnabled(false);
            tbWorkOrder = FindViewById<EditText>(Resource.Id.tbWorkOrder);
            tbIdent = FindViewById<EditText>(Resource.Id.tbIdent);
            tbSSCC = FindViewById<EditText>(Resource.Id.tbSSCC);
            tbSerialNum = FindViewById<EditText>(Resource.Id.tbSerialNum);
            lvCardList = FindViewById<ListView>(Resource.Id.lvCardList);
            tbCard = FindViewById<EditText>(Resource.Id.tbCard);
            btConfirm = FindViewById<Button>(Resource.Id.btConfirm);
            button2 = FindViewById<Button>(Resource.Id.button2);
            lbTotalQty = FindViewById<TextView>(Resource.Id.lbTotalQty);
            tbLegCode = FindViewById<EditText>(Resource.Id.tbLegCode);
            legLayout = FindViewById<LinearLayout>(Resource.Id.legLayout);




            var isPalletCode = await CommonData.GetSettingAsync("Pi.HideLegCode", this);

            if (isPalletCode != null)
            {
                if (isPalletCode != "1")
                {

                }
                else
                {
                    legLayout.Visibility = ViewStates.Invisible;

                }
            }
            else
            {
                legLayout.Visibility = ViewStates.Invisible;
            }




            tbWorkOrder.Text = cardInfo.GetString("WorkOrder").Trim();
            tbIdent.Text = cardInfo.GetString("Ident").Trim();
            tbSSCC.Text = await CommonData.GetNextSSCCAsync(this);

            barcode2D = new Barcode2D(this, this);
            btConfirm.Click += BtConfirm_Click;
            button2.Click += Button2_Click;
            AdapterListViewItem adapter = new AdapterListViewItem(this, listItems);
            lvCardList.Adapter = adapter;
            lvCardList.ItemLongClick += LvCardList_ItemLongClick;
            tbSerialNum.RequestFocus();
            tbSerialNum.KeyPress += TbSerialNum_KeyPress;
            tbCard.KeyPress += TbCard_KeyPress;

            color();


            var ident = cardInfo.GetString("Ident").Trim();


            setUpMaximumQuantity(ident);


            var _broadcastReceiver = new NetworkStatusBroadcastReceiver();
            _broadcastReceiver.ConnectionStatusChanged += OnNetworkStatusChanged;
            Application.Context.RegisterReceiver(_broadcastReceiver,
            new IntentFilter(ConnectivityManager.ConnectivityAction), ReceiverFlags.NotExported);
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
                    SentrySdk.CaptureException(err);
                }
            }
            else
            {
                LoaderManifest.LoaderManifestLoop(this);
            }
        }

        private void setUpMaximumQuantity(string ident)
        {
            string error;
            var identObject = Services.GetObject("id", ident, out error);

            collectiveAmount = identObject.GetDouble("UM1toUM2") * identObject.GetDouble("UM1toUM3");
        }

        private void TbCard_KeyPress(object sender, View.KeyEventArgs e)
        {
            if (e.KeyCode == Keycode.Enter)
            {
                if (tbCard.Text != "")
                {
                    ProcessCard(tbCard.Text);
                    tbCard.RequestFocus();
                }
            }
            else
            {
                e.Handled = false;
            }
        }

        private void TbSerialNum_KeyPress(object sender, View.KeyEventArgs e)
        {
            if (e.KeyCode == Keycode.Enter)
            {
                ProcessSerialNum();
                tbCard.RequestFocus();
            }
            else
            {
                e.Handled = false;
            }
        }



        private void LvCardList_ItemLongClick(object sender, AdapterView.ItemLongClickEventArgs e)
        {

            popupDialog = new Dialog(this);
            popupDialog.SetContentView(Resource.Layout.YesNoGeneric);
            popupDialog.Window.SetSoftInputMode(SoftInput.AdjustResize);
            popupDialog.Show();
            popupDialog.Window.SetLayout(LayoutParams.MatchParent, LayoutParams.WrapContent);
            popupDialog.Window.SetBackgroundDrawable(new ColorDrawable(Color.ParseColor("#081a45")));

            btnYes = popupDialog.FindViewById<Button>(Resource.Id.btnYes);
            btnNo = popupDialog.FindViewById<Button>(Resource.Id.btnNo);
            btnNo.Click += BtnNo_Click;
            btnYes.Click += (e, ev) => { ButtonYes(lvCardList.SelectedItemId); };

        }

        private void ButtonYes(long selectedItemId)
        {
            ListViewItem itemPriorToDelete = listItems.ElementAt((int)selectedItemId);
            totalQty = totalQty - Convert.ToDouble(itemPriorToDelete.quantity);
            listItems.RemoveAt((int)selectedItemId);
            lbTotalQty.Text = $"{Resources.GetString(Resource.String.s304)}: {totalQty.ToString("###,###,##0.00")} / {collectiveAmount.ToString("###,###,##0.00")}";

            lvCardList.Adapter = null;
            AdapterListViewItem adapter = new AdapterListViewItem(this, listItems);
            lvCardList.Adapter = adapter;
            popupDialog.Dismiss();
            popupDialog.Cancel();

        }

        private void BtnNo_Click(object sender, EventArgs e)
        {
            popupDialog.Dismiss();
            popupDialog.Cancel();
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(MainMenu));
            Finish();
        }




        private async Task runOnBothThreads()
        {
            await Task.Run(() =>
            {

                try
                {
             

                    var palInfo = new NameValueObject("PaletteInfo");

                    // UI changes.
                    RunOnUiThread(() =>
                    {
                        palInfo.SetString("WorkOrder", tbWorkOrder.Text);
                        palInfo.SetString("Ident", tbIdent.Text);
                        palInfo.SetInt("Clerk", Services.UserID());
                        palInfo.SetString("SerialNum", tbSerialNum.Text);
                        palInfo.SetString("SSCC", tbSSCC.Text);
                        palInfo.SetString("CardNums", string.Join(",", ScannedCardNumbers().Select(x => x.ToString()).ToArray()));
                        palInfo.SetDouble("TotalQty", totalQty);
                        palInfo.SetString("DeviceID", Services.DeviceUser());
                    });


                    string error;
                    palInfo = Services.SetObject($"cf&&legCode={tbLegCode.Text}", palInfo, out error);
                    if (palInfo == null)
                    {

                        RunOnUiThread(() =>
                        {
         
                            AlertDialog.Builder alert = new AlertDialog.Builder(this);
                            alert.SetTitle($"{Resources.GetString(Resource.String.s265)}");
                            alert.SetMessage($"{Resources.GetString(Resource.String.s216)}" + error);

                            alert.SetPositiveButton("Ok", (senderAlert, args) =>
                            {
                                alert.Dispose();
                                StartActivity(typeof(MainMenu));
                                Finish();
                            });


                            Dialog dialog = alert.Create();
                            dialog.Show();
                        });


                    }
                    else
                    {
                        var result = palInfo.GetString("Result");
                        if (result.StartsWith("OK!"))
                        {
                            RunOnUiThread(() =>
                            {
                                var id = result.Split('+')[1];

                                AlertDialog.Builder alert = new AlertDialog.Builder(this);
                                alert.SetTitle($"{Resources.GetString(Resource.String.s263)}");
                                alert.SetMessage($"{Resources.GetString(Resource.String.s264)}" + id);

                                alert.SetPositiveButton("Ok", (senderAlert, args) =>
                                {
                                    alert.Dispose();
                                    StartActivity(typeof(MainMenu));
                                    Finish();
                                });
                                Dialog dialog = alert.Create();
                                dialog.Show();
                            });


                        }
                        else
                        {
                            RunOnUiThread(() =>
                            {
                                AlertDialog.Builder alert = new AlertDialog.Builder(this);
                                alert.SetTitle($"{Resources.GetString(Resource.String.s265)}");
                                alert.SetMessage($"{Resources.GetString(Resource.String.s216)}" + result);

                                alert.SetPositiveButton("Ok", (senderAlert, args) =>
                                {
                                    alert.Dispose();
                                    StartActivity(typeof(MainMenu));
                                    Finish();
                                });

                                Dialog dialog = alert.Create();
                                dialog.Show();
                            });
                        }
                    }
                }
                catch(Exception ex)
                {
                    SentrySdk.CaptureException(ex);
                }
            });
        }

        private  void BtConfirm_Click(object sender, EventArgs e)
        {
            RunOnUiThread(() =>
            {
                AlertDialog.Builder alert = new AlertDialog.Builder(this);

                alert.SetTitle($"{Resources.GetString(Resource.String.s309)}");
                alert.SetMessage($"{Resources.GetString(Resource.String.s313)}");
                alert.SetPositiveButton("Ok", async (senderAlert, args) =>
                {
                    alert.Dispose();
                    await runOnBothThreads();
                });

                alert.SetNegativeButton($"{Resources.GetString(Resource.String.s311)}", (senderAlert, args) =>
                {
                    alert.Dispose();
                });

                Dialog dialog = alert.Create();
                dialog.Show();
            });
        }
    }
}