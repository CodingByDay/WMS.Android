using Android.Views;




public class GlobalLayoutListenerSelectAll : Java.Lang.Object, ViewTreeObserver.IOnGlobalLayoutListener
{
    private readonly EditText _editText;
    private readonly int _selectionEnd;

    public GlobalLayoutListenerSelectAll(EditText editText, int selectionEnd)
    {
        _editText = editText;
        _selectionEnd = selectionEnd;
    }

    public void OnGlobalLayout()
    {
        // Ensure to remove the listener once the layout is done to avoid multiple calls
        _editText.ViewTreeObserver.RemoveOnGlobalLayoutListener(this);

        // Now you can safely select all text
        _editText.SetSelection(0, _selectionEnd);
    }
}