using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

using System.Text;

namespace TrendNET.WMS.Device.App {
    /// <summary>
    /// Contains a single entry of the log file.
    /// </summary>
    public class LogEntry {
        private DateTime _timestamp;
        private string _info;
        private double _time;
        private int _elements;
        private int _threadId;
        private int _procId;

        /// <summary>
        /// Creates a log entry with given info, timing and num of elements for which timing was given.
        /// </summary>
        public LogEntry (string info, double time, int elements) {
            Set (info, time, elements);
        }

        /// <summary>
        /// Creates a log entry with given info and timing.
        /// </summary>
        public LogEntry (string info, double time) {
            Set (info, time, -1);
        }

        /// <summary>
        /// Creates a log entry with given info.
        /// </summary>
        public LogEntry (string info) {
            Set (info, -1, -1);
        }

        private void Set (string info, double time, int elements) {
            _timestamp = DateTime.Now;
            _info = info;
            _time = time;
            _elements = elements;
            _threadId = Thread.CurrentThread.ManagedThreadId;
            _procId = Process.GetCurrentProcess ().Id;
        }

        /// <summary>
        /// Return timestamp of log entry creation.
        /// </summary>
        public DateTime TimeStamp { get { return _timestamp; } }

        /// <summary>
        /// Return complete string representation of log entry.
        /// </summary>
        public string Info { 
            get {
                StringBuilder sb = new StringBuilder ();
                sb.Append (_timestamp.ToString ("yyyy-MM-dd HH:mm:ss.ffff"));
                sb.Append ("\t{");
                sb.Append (_procId.ToString ());
                sb.Append ("/");
                sb.Append (_threadId.ToString ());
                sb.Append ("}\t");
                sb.Append (_info);
                if (_time != -1) {
                    sb.Append ("; ");
                    sb.Append (_time.ToString ());
                }
                if (_elements != -1) {
                    sb.Append (';');
                    sb.Append (_elements.ToString ());
                }
                return sb.ToString ();
            }
        }
    }

    /// <summary>
    /// Handles background logging operations.
    /// </summary>
    public class Log {
        private static Object syncLock = new Object();
        private static Queue <LogEntry> entries = new Queue <LogEntry> ();

        /// <summary>
        /// Writes a single log entry into the log queue.
        /// </summary>
        public static void Write(LogEntry data)
        {
            lock (syncLock)
            {
                entries.Enqueue(data);
            }
        }

        /// <summary>
        /// Handles periodic dumping of queued log entries into log file.
        /// </summary>
        public static void Dumper()
        {
            Thread.CurrentThread.IsBackground = true;
            Log.Write(new LogEntry("Log.Dumper started"));
            while (true)
            {
                try
                {
                    Thread.Sleep(20000);
                    DumpEntries();
                }
                catch (Exception e)
                {
                    Log.Write(new LogEntry("Log Dumper failed: " + e.ToString()));

                  
                }
            }
        }

        /// <summary>
        /// Writes pending log entries into log file.
        /// </summary>
        public static void DumpEntries () {
            lock (syncLock)
            {
                if (entries.Count > 0)
                {
                    var procId = Process.GetCurrentProcess().Id;
                    var data = new StringBuilder();
                    while (entries.Count > 0)
                    {
                        LogEntry le = entries.Dequeue();
                        data.AppendLine(le.Info);
                    }
                    Services.Services.ReportData(data.ToString ());
                }
            }
        }
    }
}
