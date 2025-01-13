﻿using Android.Content;
using Android.Content.PM;
using Android.Media;
using Android.Net;
using Android.Preferences;
using Android.Renderscripts;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Aspose.Words.Fonts;
using BarCode2D_Receiver;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.ComponentModel;
using TrendNET.WMS.Core.Data;
using TrendNET.WMS.Device.App;
using TrendNET.WMS.Device.Services;
using WMS.App;
using WMS.ExceptionStore;
using static Android.Renderscripts.Sampler;
using AlertDialog = Android.App.AlertDialog;
namespace WMS
{
    [Activity(Label = "WMS")]
    public class TakeOverIdentEntry : CustomBaseActivity, IBarcodeResult

    {
        private NameValueObject moveHead = (NameValueObject)InUseObjects.Get("MoveHead");
        private NameValueObject openIdent = null;
        private int displayedOrder = -1;
        Button btScan;
        public string barcode;
        private CustomAutoCompleteTextView tbIdent;
        private EditText tbNaziv;
        private EditText tbOrder;
        private EditText tbConsignee;
        private EditText tbDeliveryDeadline;
        private EditText tbQty;
        private TextView lbOrderInfo;
        private Button btNext;
        private Button btConfirm;
        private Button btOverview;
        private Button btLogout;
        SoundPool soundPool;
        int soundPoolId;
        private Barcode2D barcode2D;
        private List<string> returnList = new List<string>();
        private List<string> identData = new List<string>();
        private Intent intentClass;
        private List<string> savedIdents;
        private CustomAutoCompleteAdapter<string> tbIdentAdapter;
        private List<OpenOrder> orders = new List<OpenOrder>();
        private ListView listData;
        private UniversalAdapter<TakeOverIdentList> dataAdapter;
        private List<TakeOverIdentList> data = new List<TakeOverIdentList>();

        protected async override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                SetTheme(Resource.Style.AppTheme_NoActionBar);
                if (App.Settings.tablet)
                {
                    base.RequestedOrientation = ScreenOrientation.Landscape;
                    base.SetContentView(Resource.Layout.TakeOverIdentEntryTablet);
                    listData = FindViewById<ListView>(Resource.Id.listData);
                    dataAdapter = UniversalAdapterHelper.GetTakeoverIdentEntry(this, data);
                    listData.ItemClick += ListData_ItemClick;
                    listData.ItemLongClick += ListData_ItemLongClick;
                    listData.Adapter = dataAdapter;
                }
                else
                {
                    base.RequestedOrientation = ScreenOrientation.Portrait;
                    base.SetContentView(Resource.Layout.TakeOverIdentEntry);
                }
                AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
                var _customToolbar = new CustomToolbar(this, toolbar, Resource.Id.navIcon);
                _customToolbar.SetNavigationIcon(App.Settings.RootURL + "/Services/Logo");
                SetSupportActionBar(_customToolbar._toolbar);
                SupportActionBar.SetDisplayShowTitleEnabled(false);
                tbIdent = FindViewById<CustomAutoCompleteTextView>(Resource.Id.tbIdent);
                tbNaziv = FindViewById<EditText>(Resource.Id.tbNaziv);
                tbOrder = FindViewById<EditText>(Resource.Id.tbOrder);
                tbConsignee = FindViewById<EditText>(Resource.Id.tbConsignee);
                tbDeliveryDeadline = FindViewById<EditText>(Resource.Id.tbDeliveryDeadline);
                tbQty = FindViewById<EditText>(Resource.Id.tbQty);
                lbOrderInfo = FindViewById<TextView>(Resource.Id.lbOrderInfo);
                btNext = FindViewById<Button>(Resource.Id.btNext);
                btConfirm = FindViewById<Button>(Resource.Id.btConfirm);
                btOverview = FindViewById<Button>(Resource.Id.btOverview);
                btLogout = FindViewById<Button>(Resource.Id.btLogout);
                color();
                barcode2D = new Barcode2D(this, this);
                if (moveHead == null) { throw new ApplicationException("moveHead not known at this point!?"); }
                displayedOrder = 0;
                FillDisplayedOrderInfo();

                btNext.Click += BtNext_Click;
                btConfirm.Click += BtConfirm_Click;
                btOverview.Click += BtOverview_Click;
                btLogout.Click += BtLogout_Click;
     

                var _broadcastReceiver = new NetworkStatusBroadcastReceiver();
                _broadcastReceiver.ConnectionStatusChanged += OnNetworkStatusChanged;
                Application.Context.RegisterReceiver(_broadcastReceiver,
                new IntentFilter(ConnectivityManager.ConnectivityAction), ReceiverFlags.NotExported);

                // UpdateSuggestions(string.Empty);
                InputMethodManager imm = (InputMethodManager)GetSystemService(Context.InputMethodService);
                imm.ShowSoftInput(tbIdent, ShowFlags.Forced);

                tbIdent.RequestFocus();
                tbIdent.TextChanged += TbIdent_TextChanged;
                tbIdent.ItemClick += TbIdent_ItemClick;
                // These are read only. 6.6.2024 JJ
                tbOrder.Enabled = false;
                tbConsignee.Enabled = false;
                tbDeliveryDeadline.Enabled = false;
                tbQty.Enabled = false;
                tbNaziv.Enabled = false;

                LoadIdentDataAsync();
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private async void TbIdent_ItemClick(object? sender, AdapterView.ItemClickEventArgs e)
        {
            await ProcessIdent(false);
        }

        private async void LoadIdentDataAsync()
        {
            try
            {
                await Task.Run(() => LoadData());

                // After loading the data, update the UI on the main thread
                RunOnUiThread(() =>
                {
                    if (savedIdents != null)
                    {
                        tbIdentAdapter = new CustomAutoCompleteAdapter<string>(this, Android.Resource.Layout.SimpleDropDownItem1Line, savedIdents);
                        tbIdent.Adapter = tbIdentAdapter;
                        tbIdentAdapter.SingleItemEvent += TbIdentAdapter_SingleItemEvent;
                    }
                });
            } catch(Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private void LoadData()
        {
            try
            {
                ISharedPreferences sharedPreferences = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
                string savedIdentsJson = sharedPreferences.GetString("idents", "");

                if (!string.IsNullOrEmpty(savedIdentsJson))
                {
                    LoaderManifest.LoaderManifestLoopResources(this);
                    savedIdents = JsonConvert.DeserializeObject<List<string>>(savedIdentsJson);
                    LoaderManifest.LoaderManifestLoopStop(this);
                }
            } catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }





        private async void TbIdent_TextChanged(object? sender, Android.Text.TextChangedEventArgs e)
        {
            try
            {
                if (e.Text.ToString() == string.Empty)
                {
                    await ProcessIdent(true);
                }
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private async void TbIdentAdapter_SingleItemEvent(string barcode)
        {
            try
            {
                var item = tbIdentAdapter.GetItem(0);
                tbIdent.SetText(item.ToString(), false);
                await ProcessIdent(false);
                tbIdent.SelectAll();
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }
   
        private void ListData_ItemLongClick(object? sender, AdapterView.ItemLongClickEventArgs e)
        {
            try
            {
                selected = e.Position;
                Select(selected);
                selectedItem = selected;
                btConfirm.PerformClick();
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private void ListData_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            try
            {
                selected = e.Position;
                Select(selected);
                selectedItem = selected;
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }


        private void fillList(string ident)
        {
            try
            {
                data.Clear();
                if (orders != null)
                {
                    orders.ForEach(x =>
                    {
                        data.Add(new TakeOverIdentList
                        {
                            Ident = x.Ident,
                            Subject = x.Client,
                            Order = x.Order,
                            Position = x.Position,
                            Quantity = x.Quantity

                        });
                    });

                    // UI changes.
                    RunOnUiThread(() =>
                    {
                        dataAdapter.NotifyDataSetChanged();
                        UniversalAdapterHelper.SelectPositionProgramaticaly(listData, 0);
                    });

                }
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }
        private void Select(int postionOfTheItemInTheList)
        {
            try
            {
                displayedOrder = postionOfTheItemInTheList;

                FillDisplayedOrderInfo();
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }
 

   



        public bool IsOnline()
        {
            try
            {
                var cm = (ConnectivityManager)GetSystemService(ConnectivityService);
                return cm.ActiveNetworkInfo == null ? false : cm.ActiveNetworkInfo.IsConnected;
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
                return false;
            }
        }


        private async void UpdateSuggestions(string userInput)
        {
            try
            {
                suggestions.Clear();
                // Provide custom suggestions based on user input
                suggestions = GetCustomSuggestions(userInput);

                // Clear the existing suggestions and add the new ones

                tbIdentAdapter.Clear();
                tbIdentAdapter.AddAll(suggestions);
                tbIdentAdapter.NotifyDataSetChanged();

 

            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private List<string> GetCustomSuggestions(string userInput)
        {
            try
            {
                if (savedIdents != null)
                {
                    // In order to improve performance try to implement paralel processing. 23.05.2024 Janko Jovičić

                    var lowerUserInput = userInput.ToLower();
                    var result = new ConcurrentBag<string>();

                    Parallel.ForEach(savedIdents, suggestion =>
                    {
                        if (suggestion.ToLower().Contains(lowerUserInput))
                        {
                            result.Add(suggestion);
                        }
                    });

                    return result.Take(100).ToList();
                }

                // Service not yet loaded. 6.6.2024 J.J
                return new List<string>();
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
                return new List<string>();
            }
        }
        private void OnNetworkStatusChanged(object sender, EventArgs e)
        {
            try
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
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }
  



        private void BtLogout_Click(object sender, EventArgs e)
        {
            try
            {
                StartActivity(typeof(MainMenu));
                Finish();
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private void BtOverview_Click(object sender, EventArgs e)
        {
            try
            {
                StartActivity(typeof(TakeOverEnteredPositionsView));
                Finish();
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private void BtConfirm_Click(object sender, EventArgs e)
        {
            try
            {
                if (SaveMoveHead())
                {
                    var intent = new Intent(this, typeof(TakeOverSerialOrSSCCEntry));
                    StartActivity(intent);
                    Finish();
                }
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private void BtNext_Click(object sender, EventArgs e)
        {
            try
            {
                displayedOrder++;

                if (displayedOrder >= orders.Count)
                {
                    displayedOrder = 0;
                }

                FillDisplayedOrderInfo();

                if (App.Settings.tablet)
                {
                    UniversalAdapterHelper.SelectPositionProgramaticaly(listData, displayedOrder);
                }
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }


        private bool SaveMoveHead()
        {
            try
            {
                var order = Base.Store.OpenOrder;
                string key = string.Empty;
                string client = string.Empty;
                int no = 0;



                if (!moveHead.GetBool("Saved"))
                {
                    try
                    {

                        moveHead.SetInt("Clerk", Services.UserID());
                        moveHead.SetString("Type", "I");

                        if (order != null)
                        {
                            key = order.Order;
                            client = order.Client;
                            no = order.Position ?? 0;
                        }

                        moveHead.SetString("LinkKey", key);
                        moveHead.SetString("LinkNo", no.ToString());

                        if (moveHead.GetBool("ByOrder"))
                        {
                            moveHead.SetString("Receiver", client);
                        }

                        string error;
                        var savedMoveHead = Services.SetObject("mh", moveHead, out error);
                        if (savedMoveHead == null)
                        {
                            // UI changes.
                            RunOnUiThread(() =>
                            {
                                DialogHelper.ShowDialogError(this, this, $"{Resources.GetString(Resource.String.s213)}");
                            });

                            return false;
                        }
                        else
                        {
                            moveHead.SetInt("HeadID", savedMoveHead.GetInt("HeadID"));
                            moveHead.SetBool("Saved", true);
                            return true;
                        }
                    }
                    catch (Exception error)
                    {
                        SentrySdk.CaptureException(error);
                        return false;
                    }
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
                return false;
            }
        }


        private bool SaveMoveHead2D(Row data)
        {
            try
            {
                if (!moveHead.GetBool("Saved"))
                {
                    try
                    {
                        moveHead.SetInt("Clerk", Services.UserID());
                        moveHead.SetString("Type", "I");
                        moveHead.SetString("LinkKey", data.StringValue("acKey"));
                        moveHead.SetString("LinkNo", data.IntValue("anNo").ToString());
                        if (moveHead.GetBool("ByOrder"))
                        {
                            moveHead.SetString("Receiver", data.StringValue("acSubject"));
                        }
                        string error;
                        var savedMoveHead = Services.SetObject("mh", moveHead, out error);
                        if (savedMoveHead == null)
                        {
                            // UI changes.
                            RunOnUiThread(() =>
                            {
                                DialogHelper.ShowDialogError(this, this, $"{Resources.GetString(Resource.String.s213)}");
                            });
                            return false;
                        }
                        else
                        {
                            moveHead.SetInt("HeadID", savedMoveHead.GetInt("HeadID"));
                            moveHead.SetBool("Saved", true);
                            return true;
                        }
                    }
                    catch (Exception error)
                    {
                        SentrySdk.CaptureException(error);
                        return false;
                    }
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
                return false;
            }
        }

        private void SaveIdent2DCode()
        {
            try
            {
                // UI changes.
                RunOnUiThread(() =>
                {
                    var ident = tbIdent.Text.Trim();
                    string error;
                    openIdent = Services.GetObject("id", ident, out error);
                    if (openIdent == null)
                    {
                        InUseObjects.Set("OpenIdent", new NameValueObject());
                    }
                    else
                    {
                        InUseObjects.Set("OpenIdent", openIdent);
                    }
                });
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private async Task ProcessIdent(bool cleanUp)
        {
            try
            {
                var ident = tbIdent.Text;

                if(cleanUp)
                {
                    ident = string.Empty;
                }

                if (string.IsNullOrEmpty(ident)) {
                    orders.Clear();
                    FillDisplayedOrderInfo();
                    if (App.Settings.tablet)
                    {
                        data.Clear();
                        dataAdapter.NotifyDataSetChanged();
                    }
                }
                try
                {
                    LoaderManifest.LoaderManifestLoopResources(this);
                    string error;
                    openIdent = Services.GetObject("id", ident, out error);
                    if (openIdent == null)
                    {
                        string SuccessMessage = string.Format($"{Resources.GetString(Resource.String.s229)}" + error);
                        Toast.MakeText(this, SuccessMessage, ToastLength.Long).Show();
                        tbIdent.Text = "";
                        tbIdent.RequestFocus();
                        tbNaziv.Text = "";
                    }
                    else
                    {
                        ident = openIdent.GetString("Code");

                        if (ident != tbIdent.Text)
                        {
                            // Needed because of the bimex process. 11. jul. 2024 Janko Jovičić
                            tbIdent.Text = ident;
                        }

                        InUseObjects.Set("OpenIdent", openIdent);
                        var isPackaging = openIdent.GetBool("IsPackaging");
                        if (!moveHead.GetBool("ByOrder") || isPackaging)
                        {
                            if (SaveMoveHead())
                            {
                                var intent = new Intent(this, typeof(TakeOverSerialOrSSCCEntry));
                                StartActivity(intent);
                                Finish();
                            }
                            return;
                        }
                        else
                        {
                            tbNaziv.Text = openIdent.GetString("Name");

                            var parameters = new List<Services.Parameter>();

                            string sql = $"SELECT acSubject, acKey, anNo, anQty, adDeliveryDeadline, acIdent, anPackQty FROM uWMSOrderItemByWarehouseTypeIn WHERE acIdent = @acIdent AND acDocType = @acDocType AND acWarehouse = @acWarehouse";

                            if (moveHead != null)
                            {
                                string? subject = moveHead.GetString("Receiver");
                                if (!string.IsNullOrEmpty(subject))
                                {
                                    sql += " AND acSubject = @acSubject;";
                                    parameters.Add(new Services.Parameter { Name = "acSubject", Type = "String", Value = subject });
                                }
                                else
                                {
                                    sql += ";";
                                }

                            }
                            else
                            {
                                StartActivity(typeof(MainMenu));
                                Finish();
                            }


                            parameters.Add(new Services.Parameter { Name = "acIdent", Type = "String", Value = ident });
                            parameters.Add(new Services.Parameter { Name = "acDocType", Type = "String", Value = moveHead.GetString("DocumentType") });
                            parameters.Add(new Services.Parameter { Name = "acWarehouse", Type = "String", Value = moveHead.GetString("Wharehouse") });

                            var subjects = await AsyncServices.AsyncServices.GetObjectListBySqlAsync(sql, parameters);
                            orders.Clear();

                            if (!subjects.Success)
                            {
                                RunOnUiThread(() =>
                                {
                                    AlertDialog.Builder alert = new AlertDialog.Builder(this);
                                    alert.SetTitle($"{Resources.GetString(Resource.String.s265)}");
                                    alert.SetMessage($"{subjects.Error}");
                                    alert.SetPositiveButton("Ok", (senderAlert, args) =>
                                    {
                                        alert.Dispose();
                                    });
                                    Dialog dialog = alert.Create();
                                    dialog.Show();

                                    SentrySdk.CaptureMessage(subjects.Error);
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
                                        orders.Add(new OpenOrder
                                        {
                                            Client = row.StringValue("acSubject"),
                                            Order = row.StringValue("acKey"),
                                            Position = (int?)row.IntValue("anNo"),
                                            Quantity = row.DoubleValue("anQty"),
                                            Date = row.DateTimeValue("adDeliveryDeadline"),
                                            Ident = row.StringValue("acIdent"),
                                            Packaging = row.DoubleValue("anPackQty")
                                        });

                                    }
                                    displayedOrder = 0;
                                }
                            }
                        }
                    }

                    FillDisplayedOrderInfo();

                    if (App.Settings.tablet)
                    {
                        fillList(ident);
                    }

                }
                catch (Exception err)
                {
                    SentrySdk.CaptureException(err);
                    return;
                }
                finally
                {
                    LoaderManifest.LoaderManifestLoopStop(this);
                }
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }


        private void FillDisplayedOrderInfo()
        {
            try
            {
                if ((openIdent != null) && (orders != null) && (orders.Count > 0))
                {
                    // UI changes.
                    RunOnUiThread(() =>
                    {
                        lbOrderInfo.Text = $"{Resources.GetString(Resource.String.s36)} (" + (displayedOrder + 1).ToString() + "/" + orders.Count.ToString() + ")";
                        var order = orders.ElementAt(displayedOrder);
                        Base.Store.OpenOrder = order;
                        tbOrder.Text = order.Order + " / " + order.Position;
                        tbConsignee.Text = order.Client;
                        tbQty.Text = order.Quantity.ToString();
                        var deadLine = order.Date;
                        tbDeliveryDeadline.Text = deadLine == null ? "" : ((DateTime)deadLine).ToString("dd.MM.yyyy");
                        btNext.Enabled = true;
                        btConfirm.Enabled = true;
                        btNext.Enabled = true;
                        btConfirm.Enabled = true;
                        tbOrder.Enabled = false;
                        tbConsignee.Enabled = false;
                        tbQty.Enabled = false;
                        tbDeliveryDeadline.Enabled = false;
                    });

                }
                else
                {
                    // UI changes.
                    RunOnUiThread(() =>
                    {
                        lbOrderInfo.Text = $"{Resources.GetString(Resource.String.s289)}";
                        tbOrder.Text = "";
                        tbConsignee.Text = "";
                        tbQty.Text = "";
                        tbDeliveryDeadline.Text = "";
                        btNext.Enabled = false;
                        btConfirm.Enabled = false;
                    });


                }
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }


        private bool preventDuplicate = false;
        private int selected;
        private int selectedItem;
        private List<string> suggestions = new List<string>();
        private bool singleItem = false;

        public async void GetBarcode(string barcode)
        {
            try
            {
                if (barcode != "Scan fail" && barcode != "")
                {
                    if (HelperMethods.is2D(barcode) && tbIdent.HasFocus && preventDuplicate == false)
                    {
                        Parser2DCode parser2DCode = new Parser2DCode(barcode.Trim());
                        await jumpAhead(parser2DCode);
                        preventDuplicate = true;
                    }
                    else if (HelperMethods.is2D(barcode) && tbIdent.HasFocus && preventDuplicate == true)
                    {
                        return;
                    }
                    else if (!CheckIdent(barcode) && barcode.Length > 17 && barcode.Contains("400") && tbIdent.HasFocus)
                    {
                        var ident = barcode.Substring(0, barcode.Length - 16);
                        tbIdent.Text = ident;
                        await ProcessIdent(false);
                    }
                    else
                    {
                        if (tbIdent.HasFocus)
                        {
                            tbIdent.Text = barcode;
                            await ProcessIdent(false);
                        }
                    }
                }
                else
                {
                    tbIdent.Text = string.Empty;
                }
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }
        private bool CheckIdent(string barcode)
        {
            try
            {
                if (string.IsNullOrEmpty(barcode)) { return false; }
                try
                {
                    string error;
                    openIdent = Services.GetObject("id", barcode, out error);
                    if (openIdent != null)
                    {
                        return true;
                    }
                }
                catch (Exception)
                {
                    return false;
                }
                return false;
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
                return false;
            }
        }

        private async Task jumpAhead(Parser2DCode parser2DCode)
        {
            try
            {
                String order = parser2DCode.clientOrder.ToString();
                string key = order;
                int addZeros = 13 - (key.Length - 2);
                int lastDash = key.LastIndexOf("-");
                string newKey = key.Insert(lastDash, "".PadLeft(addZeros, '0'));
                newKey = newKey.Replace("-", "");
                String qty = parser2DCode.netoWeight.ToString();
                String ident = parser2DCode.ident.ToString();
                string errors;
                openIdent = Services.GetObject("id", ident, out errors);
                if (openIdent != null)
                {
                    var convertedIdent = openIdent.GetString("Code");
                    ident = convertedIdent;
                }
                else
                {
                    return;
                }
                // Get all the order that are from that ident and have the right order number
                if (!String.IsNullOrEmpty(ident))
                {
                    var parameters = new List<Services.Parameter>();
                    parameters.Add(new Services.Parameter { Name = "acKey", Type = "String", Value = newKey });
                    parameters.Add(new Services.Parameter { Name = "acIdent", Type = "String", Value = ident });
                    string query = $"SELECT acKey, acSubject, anQty, adDeliveryDeadline, anNo FROM uWMSOrderItemByWarehouseTypeIn WHERE acKey = @acKey AND acIdent = @acIdent";
                    var resultQuery = await AsyncServices.AsyncServices.GetObjectListBySqlAsync(query, parameters);

                    if (!resultQuery.Success)
                    {
                        AlertDialog.Builder alert = new AlertDialog.Builder(this);
                        alert.SetTitle($"{Resources.GetString(Resource.String.s265)}");
                        alert.SetMessage($"{resultQuery.Error}");
                        alert.SetPositiveButton("Ok", (senderAlert, args) =>
                        {
                            alert.Dispose();
                        });
                        Dialog dialog = alert.Create();
                        dialog.Show();
                    }

                    if (resultQuery.Success && resultQuery.Rows.Count > 0)
                    {
                        var row = resultQuery.Rows[0];
                        tbIdent.Text = ident;
                        SaveIdent2DCode();
                        tbOrder.Text = row.StringValue("acKey");
                        tbConsignee.Text = row.StringValue("acSubject");
                        tbQty.Text = row.DoubleValue("anQty").ToString();
                        var deadLine = row.DateTimeValue("adDeliveryDeadline");
                        tbDeliveryDeadline.Text = deadLine == null ? "" : ((DateTime)deadLine).ToString("dd.MM.yyyy");

                        parser2DCode.__helper__convertedOrder = newKey;
                        parser2DCode.__helper__position = (int)(row.IntValue("anNo") ?? 0);
                        parser2DCode.ident = ident;
                        Base.Store.code2D = parser2DCode;

                        if (SaveMoveHead2D(row))
                        {
                            var intent = new Intent(this, typeof(TakeOverSerialOrSSCCEntry));
                            StartActivity(intent);
                            Finish();
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        private void color()
        {
            try
            {
                tbIdent.SetBackgroundColor(Android.Graphics.Color.Aqua);
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }

        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            try
            {
                switch (keyCode)
                {
                    case Keycode.F2:
                        if (btNext.Enabled == true)
                        {
                            BtNext_Click(this, null);
                        }
                        break;
                    case Keycode.F3:
                        if (btConfirm.Enabled == true)
                        {
                            BtConfirm_Click(this, null);

                        }
                        break;
                    case Keycode.F4:
                        if (btOverview.Enabled == true)
                        {
                            BtOverview_Click(this, null);

                        }
                        break;
                    case Keycode.F8:
                        if (btLogout.Enabled == true)
                        {
                            BtLogout_Click(this, null);
                        }
                        break;
                }
                return base.OnKeyDown(keyCode, e);
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
                return false;
            }
        }

    }
}