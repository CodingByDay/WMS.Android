using Android.App;
using Android.Content;
using Android.OS;
using Android.Preferences;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using WMS.App;
using Stream = Android.Media.Stream;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrendNET.WMS.Device.Services;

namespace WMS.Caching
{
    [Service(Exported = true)]
    public class CachingService : Service
    {
        private List<string> idents;
        public override IBinder OnBind(Intent intent)
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
                string error;
                idents = Services.GetObjectSingularList("idx", out error, "");
                // Shared preference can be used instead of the library for setting, use this in the future migrate from settings. 6.10.2023.
                ISharedPreferences sharedPreferences = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
                ISharedPreferencesEditor editor = sharedPreferences.Edit();
                string identsJson = JsonConvert.SerializeObject(idents);
                editor.PutString("idents", identsJson);
                editor.Apply();

                StopSelf(); 
            }
            catch
            {

            }
        }
    }
}