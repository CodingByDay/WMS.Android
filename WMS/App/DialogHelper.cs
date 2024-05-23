using Android.Content;
using Microsoft.AppCenter.Crashes;

public static class DialogHelper
{
    private static int GetResourceIdString(Context context, string resourceName)
    {
        int id = context.Resources.GetIdentifier(resourceName, "string", context.PackageName);
        // This method gets the corresponding id only for strings.
        return id;
    }

    public static void ShowDialogError(Activity activity, Context context, string error)
    {
        try
        {
            activity.RunOnUiThread(() =>
            {

                string localizedTitle = context.GetString(GetResourceIdString(context, "s265"));

                AlertDialog.Builder alert = new AlertDialog.Builder(context);
                alert.SetTitle(localizedTitle);
                alert.SetMessage(error);
                alert.SetPositiveButton("Ok", (senderAlert, args) =>
                {
                    alert.Dispose();
                });
                Dialog dialog = alert.Create();
                dialog.Show();
            });
        }
        catch (Exception e)
        {
            SentrySdk.CaptureException(e);
        }
    }

    public static void ShowDialogSuccess(Activity activity, Context context, string error)
    {
        try
        {
            activity.RunOnUiThread(() =>
            {
                string localizedTitle = context.GetString(GetResourceIdString(context, "s268"));

                AlertDialog.Builder alert = new AlertDialog.Builder(context);
                alert.SetTitle(localizedTitle);
                alert.SetMessage(error);
                alert.SetPositiveButton("Ok", (senderAlert, args) =>
                {
                    alert.Dispose();
                });
                Dialog dialog = alert.Create();
                dialog.Show();
            });
        }
        catch (Exception e)
        {
            SentrySdk.CaptureException(e);
        }
    }
}