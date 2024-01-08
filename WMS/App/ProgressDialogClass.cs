using Android.Content;

namespace WMS.App
{
    public class ProgressDialogClass
    {
        private Dialog popupDialog;
        private ProgressDialog dialogSync;

        public ProgressDialogClass()
        {
        }

        public void ShowDialogSync(Context target, string message)
        {
            try
            {
                dialogSync = new Android.App.ProgressDialog(target);
                dialogSync.Indeterminate = true;
                dialogSync.SetProgressStyle(Android.App.ProgressDialogStyle.Spinner);
                dialogSync.SetMessage(message);
                dialogSync.SetCancelable(false);
                dialogSync.Show();
            }
            catch
            {
            }
        }

        public void StopDialogSync()
        {
            try
            {
                dialogSync.Dismiss();
            }
            catch
            {
                return;
            }
        }
    }
}