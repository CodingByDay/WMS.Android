using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.Content;
using Java.IO;
using System;
using System.Threading.Tasks;
using Xamarin.Essentials;
using AndroidX.ConstraintLayout.Core.Motion.Utils;
using WMS.App;

namespace TrendNET.WMS.Device.Services
{
    public static class UpdateService
    {
        public static void DownloadAndInstallAPK(string url, Context context)
        {
            try
            {
                LoaderManifest.LoaderManifestLoopUpdate(context);

                string apkFileName = "downloadedApp.apk";
                string apkFilePath = Path.Combine(Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads).AbsolutePath, apkFileName);

                DownloadManager.Request request = new DownloadManager.Request(Android.Net.Uri.Parse(url));
                request.SetAllowedNetworkTypes(DownloadNetwork.Wifi | DownloadNetwork.Mobile);
                request.SetAllowedOverRoaming(false);
                request.SetTitle("Downloading APK");
                request.SetDestinationUri(Android.Net.Uri.Parse("file://" + apkFilePath)); // Set the destination directly

                DownloadManager manager = (DownloadManager)context.GetSystemService(Context.DownloadService);
                long downloadId = manager.Enqueue(request);

                // Register a BroadcastReceiver to listen for download completion
                BroadcastReceiver receiver = new DownloadCompleteReceiver(apkFilePath, context, downloadId);
                context.RegisterReceiver(receiver, new IntentFilter(DownloadManager.ActionDownloadComplete));
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                // Handle exceptions, log them, etc. 27.06.2024 Janko Jovčić
            }
            finally
            {
                LoaderManifest.LoaderManifestLoopStop(context);
            }
        }

        private class DownloadCompleteReceiver : BroadcastReceiver
        {
            private string apkFilePath;
            private Context context;
            private long downloadId;

            public DownloadCompleteReceiver(string apkFilePath, Context context, long downloadId)
            {
                this.apkFilePath = apkFilePath;
                this.context = context;
                this.downloadId = downloadId;
            }

            public override async void OnReceive(Context context, Intent intent)
            {
                long receivedDownloadId = intent.GetLongExtra(DownloadManager.ExtraDownloadId, -1);
                if (receivedDownloadId == downloadId)
                {
                    // Unregister the BroadcastReceiver
                    context.UnregisterReceiver(this);

                    // Install the APK
                    await InstallAPK(apkFilePath);
                }
            }
        }

        private static async Task InstallAPK(string apkFilePath)
        {
            try
            {
                Java.IO.File apkFile = new Java.IO.File(apkFilePath);

                if (apkFile.Exists())
                {
                    Intent intent = new Intent(Intent.ActionView);

                    // Check Android version to determine how to handle URI
                    if (Build.VERSION.SdkInt >= BuildVersionCodes.N)
                    {
                        Android.Net.Uri apkURI = Xamarin.Essentials.FileProvider.GetUriForFile(Android.App.Application.Context, "si.in_sist.wms.provider", apkFile);
                        intent.SetDataAndType(apkURI, "application/vnd.android.package-archive");
                        intent.AddFlags(ActivityFlags.NewTask);
                        intent.AddFlags(ActivityFlags.GrantReadUriPermission);
                        intent.AddFlags(ActivityFlags.ClearTop);
                        intent.PutExtra(Intent.ExtraNotUnknownSource, true);
                    }
                    else
                    {
                        intent.SetDataAndType(Android.Net.Uri.FromFile(apkFile), "application/vnd.android.package-archive");
                        intent.SetFlags(ActivityFlags.NewTask);
                    }

                    // Start the installation process
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        Application.Context.StartActivity(intent);
                    });

                    // Delay to ensure the installation starts
                    await Task.Delay(5000);

                    // Handle installation success event
                }

            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex); 
            }
        }

     
    }
}
