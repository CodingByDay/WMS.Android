using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrendNET.WMS.Core.Data;
using API;


using Newtonsoft.Json;
using System.Net;
using TrendNET.WMS.Device.App;
using WMS.App;
using Android.Test;
using static TrendNET.WMS.Device.Services.Services;




namespace WMS.AsyncServices
{
    public class AsyncServices
    {

        public static string instanceInfo = Guid.NewGuid().ToString().Split('-')[0];

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

        public static async Task<NameValueObjectList?> GetObjectListAsync(string table, string pars)
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
                int timeout = 120000;
                string device_updated = settings.ID;
                var url = RandomizeURL(settings.RootURL + "/Services/Device/?" + rqURL + "&device=" + device_updated + "&lang=" + Base.Store.language);
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromMilliseconds(timeout);

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
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                string result = ex.Message;
                return new GetResult { Success = false, Result = result };
            }
        }

        public static async Task<PostResult> PostAsync(string rqURL, string requestBody)
        {
            try
            {
                int timeout = 120000;
                string device_updated = settings.ID;
                var url = RandomizeURL(settings.RootURL + "/Services/Device/?" + rqURL + "&device=" + device_updated + "&lang=" + Base.Store.language);

                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromMilliseconds(timeout);

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
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                string result = ex.Message;
                return new PostResult { Success = false, Result = result };
            }
        }

        public static async Task<ApiResultSet?> GetObjectListBySqlAsync(string sql, List<Parameter> sqlParameters = null)
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
            try
            {
                PostResult getResult = await PostAsync("mode=sql&type=sel", requestBody);
                return JsonConvert.DeserializeObject<ApiResultSet>(getResult.Result); 
            }
            catch (Exception err)
            {
                SentrySdk.CaptureMessage(err.Message);
                return new ApiResultSet { Error = err.Message, Success = false, Results = 0, Rows = new List<Row>()};
            }
        }

    }
}
