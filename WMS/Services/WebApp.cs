using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Collections.Generic;
using System.Text;
using System.Threading;
 

using TrendNET.WMS.Device.App;
using Scanner.App;
using Microsoft.AppCenter.Crashes;
using Microsoft.AppCenter.Analytics;
using Android.Net;
using Android.Support.V7.App;

namespace TrendNET.WMS.Device.Services
{
    public class WebApp 
    {
        private const int x16kb = 16 * 1024;
        public static string rootURL = settings.RootURL;
        public static string device = settings.ID;
        private static DateTime skipPingsUntil = DateTime.MinValue;
        private static object pingLock = new object();
        
        public static void WaitForPing()
        {
            lock (pingLock)
            {
                if (DateTime.Now <= skipPingsUntil) { return; }

                var waitFor = TimeSpan.FromMinutes(5);
                var waitForMs = Convert.ToInt32(waitFor.TotalMilliseconds);
                var waitUntil = DateTime.Now.Add(waitFor);

                try
                {
                    var result = "";
                    var waitSec = 2;
                    while (DateTime.Now < waitUntil)
                    {
                        waitSec++;
                        if (waitSec > 5) { waitSec = 2; }

                        {
                            var perc = (waitForMs - Convert.ToInt32((waitUntil - DateTime.Now).TotalMilliseconds)) * 100 / waitForMs;

                            Thread.Sleep(waitSec * 100);
                        }
                        if (Ping(waitSec, out result))
                        {
                            if (result == "OK!")
                            {
                                skipPingsUntil = DateTime.Now.AddSeconds(15);
                                return;
                            }
                        }
                        else
                        {
                       
                            {                                                    
                            }
                        }
                    }                                  
                    throw new ApplicationException("Dlančnik ima težave z vzpostavitvijo povezave do strežnika (" + settings.RootURL + ")! Napaka: " + result);
                }
                catch (Exception err)
                {

                    throw new ApplicationException("Dlančnik ima težave z vzpostavitvijo povezave do strežnika (" + settings.RootURL + ")! Napaka: " + err.Message);


                }
            }
        }

        public static bool Post(string rqURL, string data, out string result)
        {
            return Post(rqURL, data, out result, 120000);
        }

        public static bool Post(string rqURL, string data, out string result, int timeout)
        {
            WaitForPing();

            bool success = false;
            string threadResult = null;
            var t = new Thread(new ThreadStart(() =>
            {
                success = PostX(rqURL, data, out threadResult, timeout);
            }));
            t.IsBackground = true;
            t.Start();
            var cnt = timeout / 1500 + 5;
      
  
                while (--cnt > 0 && !t.Join(1500))
                {
                }
                if (cnt <= 0)
                {
                    threadResult = "Timeout/Aborted!";
                    success = false;
                    t.Abort();
                }
            
            
            result = threadResult;
            return success;
        }

        public static bool PostAzure(string data, out string result, int timeout)
        {
            try
            {
                result = "";
                var url = "http://wmsconfig.azurewebsites.net/api/checkconfig";
                var startedAt = DateTime.Now;
                try
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                    request.Method = "POST";
                    request.Timeout = timeout;
                    var buffer = Encoding.UTF8.GetBytes(data);
                    request.ContentLength = buffer.Length;
                    var rqStream = request.GetRequestStream();
                    rqStream.Write(buffer, 0, buffer.Length);
                    rqStream.Close();
                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    {
                        var ms = new MemoryStream();
                        using (var receiveStream = response.GetResponseStream())
                        {
                            Encoding encode = System.Text.Encoding.GetEncoding("utf-8");
                            using (StreamReader readStream = new StreamReader(receiveStream, encode))
                            {
                                Char[] read = new Char[x16kb];
                                int count = readStream.Read(read, 0, x16kb);
                                while (count > 0)
                                {
                                    String str = new String(read, 0, count);
                                    result += str;
                                    count = readStream.Read(read, 0, x16kb);
                                }
                                return true;
                            }
                        }
                    }
                }
                finally
                {
                    Log.Write(new LogEntry("END REQUEST: [Device/PostAzure] '" + url + "';" + (DateTime.Now - startedAt).TotalMilliseconds.ToString())); /* post to App centar */
                }
            }
            catch (Exception ex)
            {
               
                result = ex.Message;
                Crashes.TrackError(ex); 
                return false;
            }
        }

        private static bool PostX(string rqURL, string data, out string result, int timeout)
        {
            try
            {
                result = "";
              
            
                var url = RandomizeURL (settings.RootURL + "/Services/Device/?" + rqURL + "&device=" + device);
                var startedAt = DateTime.Now;
                try
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                    request.Method = "POST";
                    request.Timeout = timeout;
                    var buffer = Encoding.UTF8.GetBytes(data);
                    request.ContentLength = buffer.Length;
                    var rqStream = request.GetRequestStream();
                    rqStream.Write(buffer, 0, buffer.Length);
                    rqStream.Close();
                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    {
                        var ms = new MemoryStream();
                        using (var receiveStream = response.GetResponseStream())
                        {
                            Encoding encode = System.Text.Encoding.GetEncoding("utf-8");
                            using (StreamReader readStream = new StreamReader(receiveStream, encode))
                            {
                                Char[] read = new Char[x16kb];
                                int count = readStream.Read(read, 0, x16kb);
                                while (count > 0)
                                {
                                    String str = new String(read, 0, count);
                                    result += str;
                                    count = readStream.Read(read, 0, x16kb);
                                }
                                return true;
                            }
                        }
                    }
                }
                finally
                {
                    Log.Write(new LogEntry("END REQUEST: [Device/Post] '" + url + "';" + (DateTime.Now - startedAt).TotalMilliseconds.ToString()));
                }
            }
            catch (Exception ex)
            {
              
                result = ex.Message;      
                return false;

            }
        }


     

        public static bool Get(string rqURL, out string result)
        {
            return Get(rqURL, out result, 120000);
        }

        public static bool Get(string rqURL, out string result, int timeout)
        {

            bool success = false;
            string threadResult = null;
            var t = new Thread (new ThreadStart (() => {
                success = GetX (rqURL, out threadResult, timeout);
            }));
            t.IsBackground = true;
            t.Start();

  
                while (!t.Join(1500))
                {

                }
        
            result = threadResult;
            return success;
        }

        private static bool GetX(string rqURL, out string result, int timeout)
        {
            try
            {
                result = "";
                string device_updated = settings.ID;
                var url = RandomizeURL(settings.RootURL + "/Services/Device/?" + rqURL + "&device=" + device_updated);
                var startedAt = DateTime.Now;
                try
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                    request.Method = "GET";
                    request.Timeout = timeout;
                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    {
                        var ms = new MemoryStream();
                        using (var receiveStream = response.GetResponseStream())
                        {
                            Encoding encode = System.Text.Encoding.GetEncoding("utf-8");
                            using (StreamReader readStream = new StreamReader(receiveStream, encode))
                            {
                                Char[] read = new Char[x16kb];
                                int count = readStream.Read(read, 0, x16kb);
                                while (count > 0)
                                {
                                    String str = new String(read, 0, count);
                                    result += str;
                                    count = readStream.Read(read, 0, x16kb);
                                }
                                return true;
                            }
                        }
                    }
                } catch
                {
                    Analytics.TrackEvent("END REQUEST: [Device/Get] '" + url + "';" + (DateTime.Now - startedAt).TotalMilliseconds.ToString());
                    return false;
                }
                finally
                {
                    Analytics.TrackEvent("END REQUEST: [Device/Get] '" + url + "';" + (DateTime.Now - startedAt).TotalMilliseconds.ToString());
                }
            }
            catch (Exception ex)
            {
                Analytics.TrackEvent("Interent went down.");

                result = ex.Message;
                Crashes.TrackError(ex);
                return false;
            }
        }

        private static bool Ping (int waitSec, out string result) {
            try
            {
                result = "";                         
                var url = RandomizeURL (settings.RootURL + "/Services/Device/?mode=ping&device=" + device);
                var startedAt = DateTime.Now;
                try
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                    request.Method = "GET";
                    request.Timeout = waitSec * 5000;
                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    {
                        var ms = new MemoryStream();
                        using (var receiveStream = response.GetResponseStream())
                        {
                            Encoding encode = System.Text.Encoding.GetEncoding("utf-8");
                            using (StreamReader readStream = new StreamReader(receiveStream, encode))
                            {
                                Char[] read = new Char[x16kb];
                                int count = readStream.Read(read, 0, x16kb);
                                while (count > 0)
                                {
                                    String str = new String(read, 0, count);
                                    result += str;
                                    count = readStream.Read(read, 0, x16kb);
                                }
                                return true;
                            }
                        }
                    }
                }
                finally
                {
                    Log.Write(new LogEntry("END REQUEST: [Device/Ping] '" + url + "';" + (DateTime.Now - startedAt).TotalMilliseconds.ToString()));
                    
                }
            }
            catch (Exception ex)
            {
            
                result = ex.Message;
                Crashes.TrackError(ex);
                return false;
            }
        }

        public static bool GetBin(string url, string fileName, out string result)
        {
            WaitForPing();

            bool success = false;
            string threadResult = null;
            var t = new Thread(new ThreadStart(() =>
            {
                success = GetBinX(url, fileName, out threadResult);
            }));
            t.IsBackground = true;
            t.Start();

      
                while (!t.Join(1500))
                {
          
                }
           
            result = threadResult;
            return success;
        }

        private static bool GetBinX(string url, string fileName, out string result)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(RandomizeURL (url));
                request.Method = "GET";
                request.Timeout = 300000;
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    var ms = new MemoryStream();
                    using (Stream receiveStream = response.GetResponseStream())
                    {
                        using (FileStream fs = new FileStream(fileName + ".tmp", FileMode.Create))
                        {
                            Byte[] read = new Byte[x16kb];
                            int count = receiveStream.Read(read, 0, x16kb);
                            result = "";
                            while (count > 0)
                            {
                                fs.Write(read, 0, count);
                                count = receiveStream.Read(read, 0, x16kb);
                            }
                        }

                        File.Delete(fileName);
                        File.Move(fileName + ".tmp", fileName);

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
               
                result = ex.Message;
                Crashes.TrackError(ex);
                return false;
            }
        }

        private static string RandomizeURL(string url)
        {
            if (url.Contains("?"))
            {
                return url + "&ts=" + TimeStamp() + "&i=" + Services.instanceInfo;
            }
            else
            {
                return url + "?ts=" + TimeStamp() + "&i=" + Services.instanceInfo;
            }
        }

        private static string TimeStamp()
        {
            return Environment.TickCount.ToString ();
        }
    }
}
