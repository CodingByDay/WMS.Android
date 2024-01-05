using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Stream = Android.Media.Stream;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Android.App.ActionBar;

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
            } catch
            {
            }
        }


        

        public void StopDialogSync()
        {
            try
            {
                dialogSync.Dismiss();
            } catch {

                return;
            }
        }
    }
}