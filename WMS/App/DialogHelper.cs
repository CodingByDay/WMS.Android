using Android.App;
using Android.Content;
using Microsoft.AppCenter.Crashes;
using System;

public static class DialogHelper
{

    public static void ShowDialogError(Activity activity, Context context, string error)
    {
        try
        {
            activity.RunOnUiThread(() =>
            {
                AlertDialog.Builder alert = new AlertDialog.Builder(context);
                alert.SetTitle("Napaka");
                alert.SetMessage(error);
                alert.SetPositiveButton("Ok", (senderAlert, args) =>
                {
                    alert.Dispose();
                });
                Dialog dialog = alert.Create();
                dialog.Show();
            });
        } catch (Exception e)
        {
            Crashes.TrackError(e);
        }
    }

    public static void ShowDialogSuccess(Activity activity, Context context, string error)
    {
        try
        {
            activity.RunOnUiThread(() =>
            {
                AlertDialog.Builder alert = new AlertDialog.Builder(context);
                alert.SetTitle("Informacija");
                alert.SetMessage(error);
                alert.SetPositiveButton("Ok", (senderAlert, args) =>
                {
                    alert.Dispose();
                });
                Dialog dialog = alert.Create();
                dialog.Show();
            });
        } catch (Exception e)
        {
            Crashes.TrackError(e);
        }
    }



}