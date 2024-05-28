using Android.Content;

using Com.Barcode;
using WMS;
using WMS.App;

namespace BarCode2D_Receiver
{
    public class Barcode2D
    {
        private BarcodeUtility barcodeUtility = BarcodeUtility.Instance;
        private IBarcodeResult? iBarcodeResult = null;


        public Barcode2D(Context context, IBarcodeResult iBarcodeResult)
        {
            ChangeActivity(context, iBarcodeResult);
        }


        public void ChangeActivity(Context context, IBarcodeResult iBarcodeResult)
        {
            try
            {
                // Closing the last open scanner connection so to avoid memory issues.
                if (Base.Store.lastScanningContext != null)
                {
                    barcodeUtility.Close(Base.Store.lastScanningContext, BarcodeUtility.ModuleType.Barcode2d);
                    if (Base.Store.lastBarcodeDataReceiver != null)
                    {
                        Base.Store.lastScanningContext.UnregisterReceiver(Base.Store.lastBarcodeDataReceiver);
                    }
                }

                // Changing the activity

                barcodeUtility.SetOutputMode(context, 2);
                barcodeUtility.SetScanResultBroadcast(context, "com.scanner.broadcast", "data");
                barcodeUtility.Open(context, BarcodeUtility.ModuleType.Barcode2d);
                barcodeUtility.SetReleaseScan(context, true);
                barcodeUtility.SetScanFailureBroadcast(context, true);
                barcodeUtility.EnableContinuousScan(context, false);
                barcodeUtility.EnablePlayFailureSound(context, true);
                barcodeUtility.EnablePlaySuccessSound(context, true);
                barcodeUtility.EnableEnter(context, false);

                // Add the broadcast receiver
                this.iBarcodeResult = iBarcodeResult;
                var barcodeDataReceiver = new BarcodeDataReceiver(this.iBarcodeResult);
                IntentFilter intentFilter = new IntentFilter();
                intentFilter.AddAction("com.scanner.broadcast");
                context.RegisterReceiver(barcodeDataReceiver, intentFilter);
                
                Base.Store.lastBarcodeDataReceiver = barcodeDataReceiver;
                Base.Store.lastScanningContext = context;

            } catch(Exception error) {
                SentrySdk.CaptureException(error);
            }          

        }







    }

    public class BarcodeDataReceiver : BroadcastReceiver
    {
        private IBarcodeResult ib;
        public BarcodeDataReceiver(IBarcodeResult IB)
        {
            ib = IB;
        }
        public override void OnReceive(Context context, Intent intent)
        {
            String barCode = intent.GetStringExtra("data");
            String status = intent.GetStringExtra("SCAN_STATE");
            if (status != null && (status.Equals("cancel")))
            {
                return;
            }
            else
            {
                if (barCode == null || barCode.Equals(""))
                {
                    barCode = "Scan fail";
                }

                ib?.GetBarcode(barCode);
            }
        }
    }
}