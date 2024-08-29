using Android.Content;
using Newtonsoft.Json;
using System.Text;
using TrendNET.WMS.Core.Data;
using TrendNET.WMS.Device.App;
using TrendNET.WMS.Device.Services;
using WMS.App;
using static TrendNET.WMS.Device.Services.Services;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace WMS.AsyncServices
{
    public class AsyncServices
    {
        public static string instanceInfo = Guid.NewGuid().ToString().Split('-')[0];

        // Declare a static HttpClient instance
        private static readonly HttpClient client;

        // Static constructor to initialize HttpClient
        static AsyncServices()
        {
            client = new HttpClient
            {
                Timeout = TimeSpan.FromMilliseconds(120000) // 120 seconds timeout
            };
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            // Other default configuration if needed
        }

        public class GetResult
        {
            public bool Success { get; set; }
            public string Result { get; set; }
        }

        public class PostResult
        {
            public bool Success { get; set; }
            public string Result { get; set; }
        }

        public static async Task<NameValueObjectList?> GetObjectListAsync(string table, string pars, Context context = null)
        {
            try
            {
                GetResult getResult = await GetAsync("mode=list&table=" + table + "&pars=" + pars);

                if (getResult.Success)
                {
                    return CompactSerializer.Deserialize<NameValueObjectList>(getResult.Result);
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }

        private static string RandomizeURL(string url)
        {
            if (url.Contains("?"))
            {
                return url + "&ts=" + TimeStamp() + "&i=" + AsyncServices.instanceInfo;
            }
            else
            {
                return url + "?ts=" + TimeStamp() + "&i=" + AsyncServices.instanceInfo;
            }
        }

        private static string TimeStamp()
        {
            return Environment.TickCount.ToString();
        }

        public static async Task<GetResult> GetAsync(string rqURL)
        {
            try
            {
                string device_updated = App.Settings.ID;
                var url = RandomizeURL(App.Settings.RootURL + "/Services/Device/?" + rqURL + "&device=" + device_updated + "&lang=" + Base.Store.language);

                HttpResponseMessage response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync();
                    return new GetResult { Success = true, Result = result };
                }
                else
                {
                    string result = $"Error calling web server: {response.StatusCode}";
                    return new GetResult { Success = false, Result = result };
                }
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                string result = ex.Message;
                return new GetResult { Success = false, Result = result };
            }
        }

        /*public static async Task<PostResult> PostAsync(string rqURL, string requestBody)
        {
            try
            {
                string device_updated = App.Settings.ID;
                var url = RandomizeURL(App.Settings.RootURL + "/Services/Device/?" + rqURL + "&device=" + device_updated + "&lang=" + Base.Store.language);

                StringContent content = new StringContent(requestBody, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync();
                    return new PostResult { Success = true, Result = result };
                }
                else
                {
                    string result = $"Error calling web server: {response.StatusCode}";
                    return new PostResult { Success = false, Result = result };
                }
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                string result = ex.Message;
                return new PostResult { Success = false, Result = result };
            }
        }*/

        public static async Task<ApiResultSet?> GetObjectListBySqlAsync(string sql, List<Parameter>? sqlParameters = null, Context? context = null)
        {
            try
            {
                // Create the request object
                SqlQueryRequest requestObject = sqlParameters != null
                    ? new SqlQueryRequest { SQL = sql, Parameters = sqlParameters }
                    : new SqlQueryRequest { SQL = sql };

                // Serialize the request object to JSON
                string requestBody = JsonConvert.SerializeObject(requestObject);

                // Build the request URL
                string url = RandomizeURL(App.Settings.RootURL + "/Services/Device/?mode=sql&type=sel" + "&device=" + App.Settings.ID + "&lang=" + Base.Store.language);

                // Create the HTTP content for the request
                StringContent content = new StringContent(requestBody, Encoding.UTF8, "application/json");

                // Send the POST request
                HttpResponseMessage response = await client.PostAsync(url, content);

                // Ensure the request was successful
                response.EnsureSuccessStatusCode();

                // Read the response content as a string
                string result = await response.Content.ReadAsStringAsync();

                // Deserialize the response content to ApiResultSet
                ApiResultSet resultSet = JsonConvert.DeserializeObject<ApiResultSet>(result);

                return resultSet;
            }
            catch (Exception ex)
            {
                // Capture the exception and handle it
                SentrySdk.CaptureException(ex);
                return new ApiResultSet
                {
                    Error = ex.Message,
                    Success = false,
                    Results = 0,
                    Rows = new List<Row>()
                };
            }
        }

        public static async Task<(NameValueObject? nvo, string? error)> GetObjectAsync(string? table, string? id, Context? context)
        {
            var (success, result) = await WebApp.GetAsync("mode=getObj&table=" + table + "&id=" + id, context);

            if (success)
            {
                try
                {
                    var nvo = CompactSerializer.Deserialize<NameValueObject>(result);
                    string error = nvo == null ? "Does not exist (" + table + "; " + id + ")!" : "";
                    return (nvo, error);
                }
                catch (Exception ex)
                {
                    string error = ex.Message;
                    return (null, error);
                }
            }
            else
            {
                string error = result;
                return (null, error);
            }
        }

        public static async Task<List<string>> GetObjectAsyncSingularServiceCall(string? table, string? pars)
        {
            var (success, result) = await WebApp.GetAsync("mode=list&table=" + table + "&pars=" + pars);

            if (success)
            {
                try
                {
                    var nvol = CompactSerializer.Deserialize<List<string>>(result);
                    return nvol;
                }
                catch (Exception ex)
                {
                    string error = ex.Message;
                    return new List<string>();
                }
            }
            else
            {
                string errorElse = result;
                return new List<string>();
            }
        }
    }
}
