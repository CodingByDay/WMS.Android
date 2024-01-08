using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Views.InputMethods;
using Microsoft.AppCenter.Crashes;
using Keycode = Android.Views.Keycode;

public class CustomAutoCompleteTextView : AutoCompleteTextView
{
    public int Count()
    {
        return Adapter.Count;
    }

    public CustomAutoCompleteTextView(Context context) : base(context)
    {
        Initialize();
    }

    public CustomAutoCompleteTextView(Context context, IAttributeSet attrs) : base(context, attrs)
    {
        Initialize();
    }

    public CustomAutoCompleteTextView(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
    {
        Initialize();
    }

    public CustomAutoCompleteTextView(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
    {
        Initialize();
    }

    private void Initialize()
    {
        this.KeyPress += CustomAutoCompleteTextView_KeyPress;
        // Attach a touch event handler
        Touch += (sender, e) =>
        {
            if (e.Event.Action == Android.Views.MotionEventActions.Up)
            {
                // Show the dropdown
                ShowKeyboard();
                RequestFocus();
                Handler handler = new Handler();
                handler.PostDelayed(() =>
                {
                    RequestFocus();
                    SetSelection(Text.Length); // Set the cursor at the end
                }, 100);
            }
        };
    }

    public void ShowKeyboard()
    {
        InputMethodManager imm = (InputMethodManager)Context.GetSystemService(Context.InputMethodService);
        imm.ShowSoftInput(this, ShowFlags.Implicit);
    }

    private void CustomAutoCompleteTextView_KeyPress(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keycode.Enter && e.Event.Action == KeyEventActions.Down)
        {
            // The Enter key was pressed

            View nextFocus = FocusSearch(FocusSearchDirection.Forward);
            if (nextFocus != null)
            {
                nextFocus.RequestFocus();
            }

            e.Handled = true; // Consume the event
        }
        else
        {
            e.Handled = false; // Allow normal event processing
        }
    }

    protected override void OnFocusChanged(bool gainFocus, [GeneratedEnum] FocusSearchDirection direction, Rect previouslyFocusedRect)
    {
        ShowKeyboard();
        base.OnFocusChanged(gainFocus, direction, previouslyFocusedRect);
        try
        {
            if (gainFocus)
            {
                if (Adapter != null && Adapter.Count > 0)
                {
                    ShowDropDown();
                    ShowKeyboard();
                }
            }
        }
        catch (System.Exception e)
        {
            Crashes.TrackError(e);
        }
    }

    // Function to check if options exist (replace this with your logic)

    public string GetItemAtPosition(int position)
    {
        if (position >= 0 && position < Adapter.Count)
        {
            return Adapter.GetItem(position).ToString();
        }
        return null;
    }

    public void SelectAtPosition(int position)
    {
        if (Adapter != null && position >= 0 && position < Adapter.Count)
        {
            SetText(Adapter.GetItem(position).ToString(), false);
            DismissDropDown();
        }
    }

    public int SetItemByString(string value)
    {
        int index = -1;
        for (int i = 0; i < Adapter.Count; i++)
        {
            if (Adapter.GetItem(i).ToString() == value)
            {
                SetText(Adapter.GetItem(i).ToString(), false);
                index = i;
                break;
            }
        }
        return index;
    }

    public int GetIndexOfElement(string value)
    {
        int index = -1;

        for (int i = 0; i < Adapter.Count; i++)
        {
            if (Adapter.GetItem(i).ToString() == value)
            {
                index = i;
                break;
            }
        }

        return index;
    }
}