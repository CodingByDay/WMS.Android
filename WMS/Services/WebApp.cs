

using Android.Content;
using System.Net;
using System.Text;
using TrendNET.WMS.Device.App;
using WMS;
using WMS.App;

namespace TrendNET.WMS.Device.Services
{
    public class WebApp
    {
        private const int x16kb = 16 * 1024;
        public static string rootURL = global::WMS.App.Settings.RootURL;
        public static string device = global::WMS.App.Settings.ID;
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
                    throw new ApplicationException("Dlančnik ima težave z vzpostavitvijo povezave do strežnika (" + global::WMS.App.Settings.RootURL + ")! Napaka: " + result);
                }
                catch (Exception err)
                {
                    throw new ApplicationException("Dlančnik ima težave z vzpostavitvijo povezave do strežnika (" + global::WMS.App.Settings.RootURL + ")! Napaka: " + err.Message);
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
                }
            }
            catch (Exception ex)
            {
                result = ex.Message;
                SentrySdk.CaptureException(ex);
                return false;
            }
        }

        private static bool PostX(string rqURL, string data, out string result, int timeout)
        {
            try
            {
                result = "";

                var url = RandomizeURL(global::WMS.App.Settings.RootURL + "/Services/Device/?" + rqURL + "&device=" + device + "&lang=" + Base.Store.language);
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
                catch (Exception ex)
                {
                    result = ex.Message;
                    return false;
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
            return Get(rqURL, out result, 240000);
        }



        // Creating an endpoint for long operations



        public static bool Get(string rqURL, out string result, int timeout)
        {
            bool success = false;
            string threadResult = null;

            var t = new Thread(new ThreadStart(() =>
            {
                success = GetX(rqURL, out threadResult, timeout);
            }));

            t.IsBackground = true;
            t.Start();

            while (!t.Join(1500))
            {

            }

            result = threadResult;
            return success;
        }



        /* Adding a trully asyncronous method for application-wide async functioning. 10.06.2024 Janko Jovičić */
        public static async Task<(bool success, string result)> GetAsync(string rqURL, Context context = null)
        {
      
            int timeout = 120000;
            string result = string.Empty;
            bool success = false;

            try
            {
                string device_updated = global::WMS.App.Settings.ID;
                var url = RandomizeURL(global::WMS.App.Settings.RootURL + "/Services/Device/?" + rqURL + "&device=" + device_updated + "&lang=" + Base.Store.language);

                using (var httpClient = new HttpClient())
                {
                    httpClient.Timeout = TimeSpan.FromMilliseconds(timeout);
                    var response = await httpClient.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        result = await response.Content.ReadAsStringAsync();
                        success = true;
                    }
                    else
                    {
                        result = $"Error: {response.StatusCode}";
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                result = ex.Message;
                SentrySdk.CaptureException(ex);
            }

            catch (TaskCanceledException ex)
            {
                result = "Request timed out.";
                SentrySdk.CaptureException(ex);
            }

            catch (Exception ex)
            {
                result = ex.Message;
                SentrySdk.CaptureException(ex);
            }



            return (success, result);
        }


        private static bool GetX(string rqURL, out string result, int timeout)
        {
            try
            {
                result = "";
                string device_updated = global::WMS.App.Settings.ID;
                var url = RandomizeURL(global::WMS.App.Settings.RootURL + "/Services/Device/?" + rqURL + "&device=" + device_updated + "&lang=" + Base.Store.language);
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
                }
                catch (Exception ex)
                {

                    result = ex.Message;
                    SentrySdk.CaptureException(ex);
                    return false;
                }

            }
            catch (Exception ex)
            {

                result = ex.Message;
                SentrySdk.CaptureException(ex);
                return false;
            }
        }

        private static bool Ping(int waitSec, out string result)
        {
            try
            {
                result = "";
                var url = RandomizeURL(global::WMS.App.Settings.RootURL + "/Services/Device/?mode=ping&device=" + device);
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
                }
            }
            catch (Exception ex)
            {
                result = ex.Message;
                SentrySdk.CaptureException(ex);
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
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(RandomizeURL(url));
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
                SentrySdk.CaptureException(ex);
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
            return Environment.TickCount.ToString();
        }
    }
}