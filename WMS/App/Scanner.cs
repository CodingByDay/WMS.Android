using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;


namespace TrendNET.WMS.Device.App
{

    /*
    public class Scanner
    {
        private static Barcode.Barcode barcode = new Barcode.Barcode();
        
        private Control control;

        public static void SetEvent(Barcode.Barcode.ScannerReadEventHandler readEvent)
        {
            barcode.OnRead += readEvent;
        }

        public static void ClearEvent(Barcode.Barcode.ScannerReadEventHandler readEvent)
        {
            barcode.OnRead -= readEvent;
        }

        public Scanner(Control c)
        {
            control = c;
            control.BackColor = Color.Aqua;

            var p = (control.Parent as WMSForm);
            p.EnableScanner();
        }

        private static Object syncLock = new Object();

        public static void EnableScanner()
        {
            Log.Write(new LogEntry("EnableScanner start"));
            lock (syncLock)
            {
                Log.Write(new LogEntry("  scanner status: " + barcode.EnableScanner));
                if (barcode.EnableScanner == false)
                {
                    Log.Write(new LogEntry("  enabling scanner start"));
                    barcode.DecoderParameters.CODABAR = Barcode.DisabledEnabled.Enabled;
                    barcode.DecoderParameters.CODE128 = Barcode.DisabledEnabled.Enabled;
                    barcode.DecoderParameters.CODE39 = Barcode.DisabledEnabled.Enabled;
                    barcode.DecoderParameters.CODE39Params.Code32Prefix = false;
                    barcode.DecoderParameters.CODE39Params.Concatenation = false;
                    barcode.DecoderParameters.CODE39Params.ConvertToCode32 = false;
                    barcode.DecoderParameters.CODE39Params.FullAscii = false;
                    barcode.DecoderParameters.CODE39Params.Redundancy = false;
                    barcode.DecoderParameters.CODE39Params.ReportCheckDigit = false;
                    barcode.DecoderParameters.CODE39Params.VerifyCheckDigit = false;
                    barcode.DecoderParameters.D2OF5 = Barcode.DisabledEnabled.Disabled;
                    barcode.DecoderParameters.EAN13 = Barcode.DisabledEnabled.Enabled;
                    barcode.DecoderParameters.EAN8 = Barcode.DisabledEnabled.Enabled;
                    barcode.DecoderParameters.I2OF5 = Barcode.DisabledEnabled.Enabled;
                    barcode.DecoderParameters.MSI = Barcode.DisabledEnabled.Enabled;
                    barcode.DecoderParameters.UPCA = Barcode.DisabledEnabled.Enabled;
                    barcode.DecoderParameters.UPCE0 = Barcode.DisabledEnabled.Enabled;
                    barcode.ScanParameters.BeepFrequency = 2670;
                    barcode.ScanParameters.BeepTime = 200;
                    barcode.ScanParameters.CodeIdType = Barcode.CodeIdTypes.None;
                    barcode.ScanParameters.LedTime = 3000;
                    barcode.ScanParameters.ScanType = Barcode.ScanTypes.Foreground;
                    barcode.ScanParameters.WaveFile = "";

                    barcode.EnableScanner = true;
                    Log.Write(new LogEntry("  enabling scanner end"));
                }
            }
            Log.Write(new LogEntry("EnableScanner end"));
        }

        public static void DisableScanner()
        {
            Log.Write(new LogEntry("DisableScanner start"));
            lock (syncLock)
            {
                Log.Write(new LogEntry("  scanner status: " + barcode.EnableScanner));
                if (barcode.EnableScanner == true)
                {
                    Log.Write(new LogEntry("  disabling scanner"));
                    barcode.EnableScanner = false;

                    Log.Write(new LogEntry("  disabling scanner end"));
                }
            }
            
            Log.Write(new LogEntry("DisableScanner end"));
        }
    }
     */
}
