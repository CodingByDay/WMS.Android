using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Java.IO;
using System;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Resource = Xamarin.Essentials.Resource;

namespace TrendNET.WMS.Device.Services
{
    public static class UpdateService
    {
        public static void DownloadAndInstallAPK(string url, Context context, string fileName)
        {
            try
            {
                // Create an AlertDialog programmatically
                AlertDialog.Builder builder = new AlertDialog.Builder(context);
                builder.SetCancelable(false); // Set dialog to non-cancelable
                builder.SetTitle("Updating WMS.");

                // Create a LinearLayout to hold the progress components
                LinearLayout layout = new LinearLayout(context);
                layout.Orientation = Orientation.Vertical;
                layout.SetPadding(50, 50, 50, 50); // Example padding

                // Create a ProgressBar
                ProgressBar progressBar = new ProgressBar(context, null, Android.Resource.Attribute.ProgressBarStyleHorizontal);
                progressBar.LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
                progressBar.Indeterminate = false;
                progressBar.Max = 100;
                layout.AddView(progressBar);

                // Create a TextView for progress text
                TextView textProgress = new TextView(context);
                textProgress.LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
                textProgress.Gravity = GravityFlags.Center;
                textProgress.Text = "Downloading...";
                layout.AddView(textProgress);

                builder.SetView(layout);

                // Create and show the AlertDialog
                AlertDialog dialog = builder.Create();
                dialog.Show();

                string apkFileName = fileName;
                string apkFilePath = Path.Combine(Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads).AbsolutePath, apkFileName);

                DownloadManager.Request request = new DownloadManager.Request(Android.Net.Uri.Parse(url));
                request.SetAllowedNetworkTypes(DownloadNetwork.Wifi | DownloadNetwork.Mobile);
                request.SetAllowedOverRoaming(false);
                request.SetDestinationUri(Android.Net.Uri.Parse("file://" + apkFilePath));
                request.SetNotificationVisibility(DownloadVisibility.Visible);

                DownloadManager manager = (DownloadManager)context.GetSystemService(Context.DownloadService);
                long downloadId = manager.Enqueue(request);

                // Register a BroadcastReceiver to listen for download completion
                BroadcastReceiver receiver = new DownloadCompleteReceiver(apkFilePath, context, downloadId, dialog, progressBar, textProgress);
                context.RegisterReceiver(receiver, new IntentFilter(DownloadManager.ActionDownloadComplete));

                // Start the progress update task
                Task.Run(() => UpdateProgress(manager, downloadId, progressBar, textProgress, context));
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
            }
        }


        private class DownloadCompleteReceiver : BroadcastReceiver
        {
            private string apkFilePath;
            private Context context;
            private long downloadId;
            private AlertDialog dialog;
            private ProgressBar progressBar;
            private TextView textProgress;

            public DownloadCompleteReceiver(string apkFilePath, Context context, long downloadId, AlertDialog dialog, ProgressBar progressBar, TextView textProgress)
            {
                this.apkFilePath = apkFilePath;
                this.context = context;
                this.downloadId = downloadId;
                this.dialog = dialog;
                this.progressBar = progressBar;
                this.textProgress = textProgress;
            }

            public override async void OnReceive(Context context, Intent intent)
            {
                long receivedDownloadId = intent.GetLongExtra(DownloadManager.ExtraDownloadId, -1);
                if (receivedDownloadId == downloadId)
                {
                    // Unregister the BroadcastReceiver
                    context.UnregisterReceiver(this);

                    // Dismiss the dialog
                    dialog.Dismiss();

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

                    if (Build.VERSION.SdkInt >= BuildVersionCodes.N)
                    {
                        Android.Net.Uri apkURI = Xamarin.Essentials.FileProvider.GetUriForFile(Application.Context, "si.in_sist.wms.provider", apkFile);
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

                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        try
                        {
                            Application.Context.StartActivity(intent);
                        }
                        catch (Exception ex)
                        {
                            // Log the exception
                            SentrySdk.CaptureException(ex);
                            // Optionally, show a toast or dialog indicating installation failure
                        }
                    });

                    await Task.Delay(5000);
                }
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
            }
        }

        private static async Task UpdateProgress(DownloadManager manager, long downloadId, ProgressBar progressBar, TextView textProgress, Context context)
        {
            bool downloading = true;

            DownloadManager.Query query = new DownloadManager.Query();
            query.SetFilterById(downloadId);

            while (downloading)
            {
                await Task.Delay(1000);

                using (var cursor = manager.InvokeQuery(query))
                {
                    if (cursor != null && cursor.MoveToFirst())
                    {
                        int bytesDownloaded = cursor.GetInt(cursor.GetColumnIndex(DownloadManager.ColumnBytesDownloadedSoFar));
                        int bytesTotal = cursor.GetInt(cursor.GetColumnIndex(DownloadManager.ColumnTotalSizeBytes));

                        if (cursor.GetInt(cursor.GetColumnIndex(DownloadManager.ColumnStatus)) == (int)DownloadStatus.Successful)
                        {
                            downloading = false;
                        }

                        if (bytesTotal > 0)
                        {
                            int progress = (int)((bytesDownloaded * 100L) / bytesTotal);

                            // Update the ProgressBar and text on the UI thread
                            MainThread.BeginInvokeOnMainThread(() =>
                            {
                                progressBar.Progress = progress;
                                textProgress.Text = $"{progress}%";
                            });
                        }
                    }
                }
            }
        }
    }
}
