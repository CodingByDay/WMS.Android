using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Collections.Generic;
using System.Text;
using Microsoft.AppCenter.Crashes;
using Scanner.App;
namespace TrendNET.WMS.Device.App
{
    public class WebApp
    {
        /// <summary>
        /// device config
        /// </summary>

     public static string rootURL = settings.RootURL;
        private const int x64kb = 64 * 1024;
     // var rootURL = "http://wms.in-sist.si";
        
        public static bool Get (string rqURL, out string result) {
            try {
                // var rootURL = WMSDeviceConfig.GetString("WebApp", "http://localhost");
            
                var url = rootURL + "/Services/Device/Echo.asdp"; /* Settings library */
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                var ms = new MemoryStream();
                var stream = response.GetResponseStream();
                var buffer = new byte[x64kb];
                int read;
                do {
                    read = stream.Read(buffer, 0, x64kb);
                    ms.Write(buffer, 0, read);
                } while (read == x64kb);
                result = Encoding.UTF8.GetString(ms.ToArray (), 0, (int) ms.Length);
                return true;
            } catch (Exception ex) {
               

                result = ex.Message;
                Crashes.TrackError(ex);
                return false;
            }
        }
    }
}
