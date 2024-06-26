using Android.App;
using Android.Content;
using Android.OS;
using Java.IO;
using System;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace TrendNET.WMS.Device.Services
{
    public static class UpdateService 
    {
        public static void DownloadApk(string downloadUrl)
        {
            try
            {
                // Create a new DownloadManager.Request
                DownloadManager.Request request = new DownloadManager.Request(Android.Net.Uri.Parse(downloadUrl));

                // Set the MIME type for the request (application/vnd.android.package-archive for APK)
                request.SetMimeType("application/vnd.android.package-archive");

                // Set the title and description for display in the download notification
                request.SetTitle("Downloading Update");
                request.SetDescription("Please wait while the update is downloaded.");

                // Set the destination for the downloaded file (External directory)
                request.SetDestinationInExternalPublicDir(Android.OS.Environment.DirectoryDownloads, "test.apk");

                // Get the download service and enqueue the request
                DownloadManager manager = (DownloadManager)Android.App.Application.Context.GetSystemService(Context.DownloadService);
                long downloadId = manager.Enqueue(request);

                // Optionally, register a BroadcastReceiver to listen for download completion
                // BroadcastReceiver implementation not shown here for simplicity
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
            }
        }


        public static void InstallApk(string filePath)
        {
            try
            {
                Java.IO.File file = new Java.IO.File(filePath);

                // Create the Intent for installing the APK
                Intent intent = new Intent(Intent.ActionView);
                intent.SetDataAndType(Android.Net.Uri.FromFile(file), "application/vnd.android.package-archive");
                intent.SetFlags(ActivityFlags.NewTask);
                intent.AddFlags(ActivityFlags.GrantReadUriPermission);
                Android.App.Application.Context.StartActivity(intent);
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
            }
        }
    }
}
