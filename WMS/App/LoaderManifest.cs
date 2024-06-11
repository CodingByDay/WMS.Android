using Android.Content;
using Android.Net;

namespace WMS.App
{
    public static class LoaderManifest
    {
        private static ProgressDialogClass progress;
        private static Context current;

        public static void LoaderManifestLoop(Context context)
        {
            try
            {
                Activity activity = context as Activity;
                activity.RunOnUiThread(() =>
                {
                    if (progress != null)
                    {
                        progress.StopDialogSync();
                    }
                    progress = new ProgressDialogClass();
                    progress.ShowDialogSync(context, "Waiting for connection...");
                });
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
            }
        }

        public static void LoaderManifestLoopResources(Context context)
        {
            try
            {
                Activity activity = context as Activity;
                activity.RunOnUiThread(() =>
                {
                    if (progress != null)
                    {
                        progress.StopDialogSync();
                    }
                    progress = new ProgressDialogClass();
                    progress.ShowDialogSync(context, "Waiting...");
                });
            } catch (Exception ex) 
            {
                SentrySdk.CaptureException(ex);
            }
        }

 

        public static void LoaderManifestLoopStop(Context context)
        {
            try
            {
                Activity activity = context as Activity;
                activity.RunOnUiThread(() =>
                {
                    if (progress != null)
                    {
                        progress.StopDialogSync();
                    }

                });
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
            }
        }

        public static NetworkInfo GetNetworkInfo(Context context)
        {
            ConnectivityManager cm = (ConnectivityManager)context.GetSystemService(Context.ConnectivityService);
            return cm.ActiveNetworkInfo;
        }

        /**
	     * Check if there is any connectivity
	     * @param context
	     * @return
	     */

        public static bool IsConnected(Context context)
        {
            NetworkInfo info = GetNetworkInfo(context);
            return (info != null && info.IsConnected);
        }
    }
}