using Android.App;
using Android.Content;
using Android.OS;
using Android.Preferences;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TrendNET.WMS.Device.Services;

namespace WMS.Caching
{
    [Service(Exported = true)]
    public class CachingService : Service
    {
        private List<string> idents;

        public override IBinder? OnBind(Intent intent)
        {
            return null;
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            // Perform the API call and caching in the background
            Task.Run(async () => await SerializeListCallAPIPersist());
            return StartCommandResult.NotSticky;
        }

        private async Task SerializeListCallAPIPersist()
        {
            try
            {

                idents = await AsyncServices.AsyncServices.GetObjectAsyncSingularServiceCall("idx", "");

                // Serialize idents to JSON
                string identsJson = JsonConvert.SerializeObject(idents);

                // Use SharedPreferences for caching
                ISharedPreferences sharedPreferences = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
                ISharedPreferencesEditor editor = sharedPreferences.Edit();

                // Store serialized data in SharedPreferences
                editor.PutString("idents", identsJson);
                editor.Apply();

                // Stop the service when done
                StopSelf();
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., log with Sentry)
                SentrySdk.CaptureException(ex);
                // Optionally retry or handle errors gracefully
            }
        }
    }
}
