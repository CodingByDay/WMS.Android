using Android.Content;

using Com.Barcode;

namespace BarCode2D_Receiver
{
    public class Barcode2D
    {
        private String TAG = "Barcode2D";
        private BarcodeUtility barcodeUtility = null;
        private BarcodeDataReceiver barcodeDataReceiver = null;
        private IBarcodeResult iBarcodeResult = null;

        public Barcode2D()
        {
            barcodeUtility = BarcodeUtility.Instance;//.getInstance();
        }

        public void startScan(Context context)
        {
            if (barcodeUtility != null)
            {
                barcodeUtility.StartScan(context, BarcodeUtility.ModuleType.Barcode2d);
            }
        }

        public void EnableTrigger(Context context, bool enable)
        {
            if (barcodeUtility != null)
            {
                if (enable)
                    barcodeUtility.StartScan(context, BarcodeUtility.ModuleType.Barcode2d);
                else
                {
                    barcodeUtility.StopScan(context, BarcodeUtility.ModuleType.Barcode2d);
                    EnableKeyboardemulator(context, false);
                }
            }
        }

        public void EnableKeyboardemulator(Context context, bool enable)
        {
            if (barcodeUtility != null)
            {
                if (enable)
                    barcodeUtility.OpenKeyboardHelper(context);
                else
                    barcodeUtility.CloseKeyboardHelper(context);
            }
        }

        public void GoodReadNotificationSound(Context context, bool enable)
        {
            if (barcodeUtility != null)
            {
                if (enable)
                    barcodeUtility.EnablePlaySuccessSound(context, true);
                else
                    barcodeUtility.EnablePlaySuccessSound(context, false);
            }
        }

        
        public void stopScan(Context context)
        {
            if (barcodeUtility != null)
            {
                barcodeUtility.StopScan(context, BarcodeUtility.ModuleType.Barcode2d);
            }
        }

        
        public void open(Context context, IBarcodeResult iBarcodeResult)
        {
            if (barcodeUtility != null)
            {
                this.iBarcodeResult = iBarcodeResult;
                barcodeUtility.SetOutputMode(context, 2);
                barcodeUtility.SetScanResultBroadcast(context, "com.scanner.broadcast", "data");
                barcodeUtility.Open(context, BarcodeUtility.ModuleType.Barcode2d);
                barcodeUtility.SetReleaseScan(context, true);
                barcodeUtility.SetScanFailureBroadcast(context, true);
                barcodeUtility.EnableContinuousScan(context, false);
                barcodeUtility.EnablePlayFailureSound(context, false);
                barcodeUtility.EnablePlaySuccessSound(context, false);
                barcodeUtility.EnableEnter(context, false);

                if (barcodeDataReceiver == null)
                {
                    barcodeDataReceiver = new BarcodeDataReceiver(this.iBarcodeResult);
                    IntentFilter intentFilter = new IntentFilter();
                    intentFilter.AddAction("com.scanner.broadcast");
                    context.RegisterReceiver(barcodeDataReceiver, intentFilter);
                }
            }
        }

        
        public void close(Context context)
        {
            if (barcodeUtility != null)
            {
                barcodeUtility.Close(context, BarcodeUtility.ModuleType.Barcode2d);
                if (barcodeDataReceiver != null)
                {
                    context.UnregisterReceiver(barcodeDataReceiver);
                    barcodeDataReceiver = null;
                }
            }
        }
    }

    internal class BarcodeDataReceiver : BroadcastReceiver
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
                if (barCode != null && !barCode.Equals(""))
                {

                }
                else
                {
                    barCode = "Scan fail";
                }
                if (ib != null)
                    ib.GetBarcode(barCode);
            }
        }
    }
}