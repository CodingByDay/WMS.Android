using Android.App;
using Android.Content;
using System;
using System.Threading.Tasks;

public class DialogAsync : Java.Lang.Object, IDialogInterfaceOnClickListener, IDialogInterfaceOnCancelListener
{
    readonly TaskCompletionSource<bool?> taskCompletionSource = new TaskCompletionSource<bool?>();

    public DialogAsync(IntPtr handle, Android.Runtime.JniHandleOwnership transfer) : base(handle, transfer) { }
    public DialogAsync() { }

    public void OnClick(IDialogInterface dialog, int which)
    {
        switch (which)
        {
            case -1:
                SetResult(true);
                dialog.Dismiss();
                dialog.Cancel();
                
                break;
            default:
                SetResult(false);
                break;

        }
    }
  
    public void OnCancel(IDialogInterface dialog)
    {
        SetResult(false);
    }

    void SetResult(bool? selection)
    {
        taskCompletionSource.SetResult(selection);
    }

    public async static Task<bool?> Show(Activity context, string title, string message)
    {
        using (var listener = new DialogAsync())
        using (var dialog = new AlertDialog.Builder(context)
                                                            .SetPositiveButton("Da", listener)
                                                            .SetNegativeButton("Ne", listener)
                                                            .SetOnCancelListener(listener)
                                                            .SetTitle(title)
                                                            .SetMessage(message))
        {
            dialog.Show();
            return await listener.taskCompletionSource.Task;
        }
    }
}