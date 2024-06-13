using Android.Content;
using API;


using Newtonsoft.Json;
using System.Net;
using System.Text;
using TrendNET.WMS.Core.Data;
using TrendNET.WMS.Device.App;
using WMS.App;

namespace TrendNET.WMS.Device.Services
{
    public class Services
    {
        public static List<NameValue> UserInfo = new List<NameValue>();

        public static void ClearUserInfo()
        {
            UserInfo.Clear();
        }

        public static string defaultIssueWarehouse()
        {
            return string.Empty;
        }

        public static string defaultInterwarehouse()
        {
            return string.Empty;
        }

        public static string defaultTakeoverWarehouse()
        {
            return string.Empty;
        }

        /// <summary>
        /// Method for downloading an image for a specific warehouse.
        /// </summary>
        /// <returns>Android.Graphics.Bitmap image</returns>
        public static Android.Graphics.Bitmap GetImageFromServer(string warehouse)
        {
            using (WebClient wc = new WebClient())
            {
                var webApp = Settings.RootURL;
                try
                {
                    using (WebClient webClient = new WebClient())
                    {
                        image = wc.DownloadData(webApp + "/Services/Image/?wh=" + warehouse);

                        Android.Graphics.Bitmap bitmapImage = Android.Graphics.BitmapFactory.DecodeByteArray(image, 0, image.Length, null);
                        return bitmapImage;
                    }
                }
                catch (System.Net.WebException)
                {
                    return null;
                }
            }
        }

        public static Android.Graphics.Bitmap GetImageFromServerIdent(string warehouse, string ident)
        {
            using (WebClient wc = new WebClient())
            {
                var webApp = Settings.RootURL;
                try
                {
                    using (WebClient webClient = new WebClient())
                    {
                        image = wc.DownloadData(webApp + "/Services/Image/?wh=" + warehouse + "&ident=" + ident);

                        Android.Graphics.Bitmap bitmapImage = Android.Graphics.BitmapFactory.DecodeByteArray(image, 0, image.Length, null);
                        return bitmapImage;
                    }
                }
                catch (System.Net.WebException)
                {
                    return null;
                }
            }
        }

        public static bool isTablet(string target)
        {
            if (target == "Tablica" || target == "Tablet")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static async Task<bool> HasPermission(string perm, string minLevel, Context? context = null)
        {
            var usePerm = await CommonData.GetSettingAsync("UsePermissions");
            if (string.IsNullOrEmpty(usePerm) || usePerm == "1")
            {
                var item = UserInfo.FirstOrDefault(x => x.Name == "Perm" + perm);
                if (item == null)
                {
                    return false;
                }
                else
                {
                    if (string.IsNullOrEmpty(item.StringValue) || (item.StringValue == "0"))
                    {
                        return false;
                    }
                    if ((minLevel == "R") && (item.StringValue == "R" || item.StringValue == "W" || item.StringValue == "D")) { return true; }
                    if ((minLevel == "W") && (item.StringValue == "W" || item.StringValue == "D")) { return true; }
                    if ((minLevel == "D") && (item.StringValue == "D")) { return true; }
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        public static int UserID()
        { return (int)UserInfo.First(x => x.Name == "UserID").IntValue; }

        public static string UserName()
        { return (string)UserInfo.First(x => x.Name == "FullName").StringValue; }

        public static string DeviceUser()
        { return Settings.ID + "|" + UserID().ToString(); }

        private static List<string> obtainedLocks = new List<string>();

        public static void ReleaseObtainedLocks()
        {
            if (obtainedLocks.Count > 0)
            {
                try
                {
                    string error;
                    obtainedLocks.ForEach(l => WebApp.Get("mode=releaseLock&lockID=" + l, out error));
                    obtainedLocks.Clear();
                }
                catch (Exception err)
                {
                    SentrySdk.CaptureException(err);
                    return;
                }
            }
        }

        public static bool TryLock(string lockID, out string error)
        {
            if (obtainedLocks.FirstOrDefault(x => x == lockID) != null) { error = "OK!"; return true; }

            var obj = new NameValueObject("Lock");
            obj.SetString("LockID", lockID);
            obj.SetString("LockInfo", UserName());
            obj.SetInt("Locker", UserID());
            var serObj = CompactSerializer.Serialize<NameValueObject>(obj);
            if (WebApp.Post("mode=tryLock", serObj, out error))
            {
                if (error == "OK!")
                {
                    obtainedLocks.Add(lockID);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public static bool IsValidUser(string password, out string error)
        {
            string result;
            if (WebApp.Get("mode=loginUser&password=" + password, out result))
            {
                try
                {
                    var nvl = CompactSerializer.Deserialize<NameValueList>(result);
                    if (nvl.Get("Success").BoolValue == true)
                    {
                        nvl.Items.ForEach(nv => UserInfo.Add(nv));
                        error = "";
                        return true;
                    }
                    else
                    {
                        error = nvl.Get("Error").StringValue;
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    error = "Napaka pri tolmačenju odziva web strežnika: " + ex.Message;
                    return false;
                }
            }
            else
            {
                error = "Napaka pri klicu web strežnika: " + result;
                return false;
            }
        }



        public async static Task<bool> IsValidUserAsync(string password)
        {
            var (success, content) = await WebApp.GetAsync("mode=loginUser&password=" + password);
            string error = string.Empty;
            if (success)
            {
                try
                {
                    var nvl = CompactSerializer.Deserialize<NameValueList>(content);
                    if (nvl.Get("Success").BoolValue == true)
                    {
                        nvl.Items.ForEach(nv => UserInfo.Add(nv));
                        error = "";
                        return true;
                    }
                    else
                    {
                        error = nvl.Get("Error").StringValue;
                        return false;
                    }
                }
                catch (Exception)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public class SqlQueryRequest
        {
            public string SQL { get; set; }
            public List<Parameter> Parameters { get; set; }

        }
        /// <summary>
        /// Type pri parametrih je lahko: 'String', 'Int16', 'Int32', 'Int64', 'DateTime', 'Decimal', 'Float'
        /// </summary>
        public class Parameter
        {
            public string Name { get; set; }
            public string Type { get; set; }
            public object Value { get; set; }
        }


        /*
          var parameterId = {
          Name: props.selectedTable.id,
          Type: idType,
          Value: props.id, 
        */
        public static ApiResultSet GetObjectListBySql(string sql, List<Parameter> sqlParameters = null)
        {
            string result;
            SqlQueryRequest requestObject;

            if (sqlParameters != null)
            {
                // Create a JSON object containing the SQL query
                requestObject = new SqlQueryRequest { SQL = sql, Parameters = sqlParameters };
            }
            else
            {
                requestObject = new SqlQueryRequest { SQL = sql };
            }
            string requestBody = JsonConvert.SerializeObject(requestObject);

            if (WebApp.Post("mode=sql&type=sel", requestBody, out result))
            {
                try
                {
                    var startedAt = DateTime.Now;
                    var response = JsonConvert.DeserializeObject<ApiResultSet>(result);
                    return response;
                }
                catch (Exception ex)
                {
                    // Handle deserialization or other exceptions
                    return new ApiResultSet
                    {
                        Success = false,
                        Error = "Napaka pri tolmačenju odziva web strežnika: " + ex.Message,
                    };
                }
            }
            else
            {
                return new ApiResultSet
                {
                    Success = false,
                    Error = "Napaka pri klicu web strežnika: " + result,
                };
            }
        }



        public static ApiResultSetUpdate Update(string sql, List<Parameter> sqlParameters = null)
        {
            string result;
            SqlQueryRequest requestObject;

            if (sqlParameters != null)
            {
                // Create a JSON object containing the SQL query
                requestObject = new SqlQueryRequest { SQL = sql, Parameters = sqlParameters };
            }
            else
            {
                requestObject = new SqlQueryRequest { SQL = sql };
            }
            string requestBody = JsonConvert.SerializeObject(requestObject);

            if (WebApp.Post("mode=sql&type=upd", requestBody, out result))
            {
                try
                {
                    var startedAt = DateTime.Now;
                    var response = JsonConvert.DeserializeObject<ApiResultSetUpdate>(result);
                    return response;
                }
                catch (Exception ex)
                {
                    // Handle deserialization or other exceptions
                    return new ApiResultSetUpdate
                    {
                        Success = false,
                        Error = "Napaka pri tolmačenju odziva web strežnika: " + ex.Message,
                    };
                }
            }
            else
            {
                return new ApiResultSetUpdate
                {
                    Success = false,
                    Error = "Napaka pri klicu web strežnika: " + result,
                };
            }
        }

        public static NameValueObjectList GetObjectList(string table, out string error, string pars)
        {
            if (table == "str")
            {
                var stop = true;
            }

            string result;
            if (WebApp.Get("mode=list&table=" + table + "&pars=" + pars, out result))
            {
                try
                {
                    var startedAt = DateTime.Now;
                    var nvol = CompactSerializer.Deserialize<NameValueObjectList>(result);
                    error = "";
                    return nvol;
                }
                catch (Exception ex)
                {
                    error = "Napaka pri tolmačenju odziva web strežnika: " + ex.Message;

                    return null;
                }
            }
            else
            {
                error = "Napaka pri klicu web strežnika: " + result;
                return null;
            }
        }

        public static List<string> GetObjectSingularList(string table, out string error, string pars)
        {
            string result;
            if (WebApp.Get("mode=list&table=" + table + "&pars=" + pars, out result))
            {
                try
                {
                    var startedAt = DateTime.Now;
                    var nvol = CompactSerializer.Deserialize<List<string>>(result);
                    error = "";
                    return nvol;
                }
                catch (Exception ex)
                {
                    error = "Napaka pri tolmačenju odziva web strežnika: " + ex.Message;
                    return null;
                }
            }
            else
            {
                error = "Napaka pri klicu web strežnika: " + result;
                return null;
            }
        }

        public static NameValueObject GetObject(string table, string id, out string error)
        {
            string result;
            if (WebApp.Get("mode=getObj&table=" + table + "&id=" + id, out result))
            {
                try
                {
                    var startedAt = DateTime.Now;
                    var nvo = CompactSerializer.Deserialize<NameValueObject>(result);
                    error = nvo == null ? "Ne obstaja (" + table + "; " + id + ")!" : "";
                    return nvo;
                }
                catch (Exception ex)
                {
                    error = "Napaka pri tolmačenju odziva web strežnika: " + ex.Message;

                    return null;
                }
            }
            else
            {
                error = "Napaka pri klicu web strežnika: " + result;
                return null;
            }
        }

        public static NameValueObject SetObject(string table, NameValueObject data, out string error)
        {
            string result;
            var startedAt = DateTime.Now;
            var serData = CompactSerializer.Serialize<NameValueObject>(data);
            if (WebApp.Post("mode=setObj&table=" + table, serData, out result))
            {
                try
                {
                    startedAt = DateTime.Now;
                    var nvo = CompactSerializer.Deserialize<NameValueObject>(result);
                    error = nvo == null ? "Zapis objekta ni uspel!" : "";

                    return nvo;
                }
                catch (Exception ex)
                {
                    error = "Napaka pri tolmačenju odziva web strežnika: " + ex.Message;

                    return null;
                }
            }
            else
            {
                error = "Napaka pri klicu web strežnika: " + result;
                return null;
            }
        }


        public static object reportLock = new object();
        public static string instanceInfo = Guid.NewGuid().ToString().Split('-')[0];

    

        private static bool pdRunning = false;
        private static DateTime lastCall = DateTime.MinValue;
        private static string lastEventName = null;
        private static byte[] image;

       
    }
}