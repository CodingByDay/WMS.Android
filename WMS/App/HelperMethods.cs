using Android.Content;
using Android.Views;

namespace WMS.App
{
    public static class HelperMethods
    {
        public static View FindNextFocusableView(View currentView)
        {
            if (currentView == null)
                return null;

            ViewGroup parentView = (ViewGroup)currentView.Parent;
            if (parentView == null)
                return null;

            View nextFocusableView = null;
            bool foundCurrent = false;
            int childCount = parentView.ChildCount;

            for (int i = 0; i < childCount; i++)
            {
                View child = parentView.GetChildAt(i);

                if (foundCurrent && IsFocusable(child))
                {
                    nextFocusableView = child;
                    break;
                }

                if (child == currentView)
                {
                    foundCurrent = true;
                }
            }

            // If not found in this parent, recursively search in the parent's parent
            if (nextFocusableView == null && parentView.Parent is ViewGroup)
            {
                nextFocusableView = FindNextFocusableView(parentView);
            }

            return nextFocusableView;
        }

        public static bool IsFocusable(View view)
        {
            return view != null && view.Focusable && view.Visibility == ViewStates.Visible;
        }

        public static bool is2D(string code)
        {
            if (code.Contains("1T") && code.Contains("K") && code.Contains("4Q"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static string lastReturn(string input, int noCharacters)
        {
            if (noCharacters > input.Length)
            {
                return input;
            }
            else
            {
                string lastReturn;
                lastReturn = input.Substring(input.Length - noCharacters);
                return lastReturn;
            }
        }



        public async static Task TabletHaltCorrectly(Context context)
        {

            LoaderManifest.LoaderManifestLoopResources(context);
            string initialText = Base.Store.CurrentAutoCompleteInstance.Text;
            int iterations = 0;
            int maxIterations = 5;
            // Loop to check if text remains unchanged after 1 second intervals
            while (iterations < maxIterations)
            {
                await Task.Delay(1000); // Wait for 1 second

                // Check if the text has changed
                if (Base.Store.CurrentAutoCompleteInstance.Text == initialText)
                {
                    // Text has not changed, show message or perform action
                    LoaderManifest.LoaderManifestLoopStop(context);
                    return; // Exit the method
                }
                else
                {
                    // Update initialText to current text and continue checking
                    initialText = Base.Store.CurrentAutoCompleteInstance.Text;
                }
                iterations++;
            }
            LoaderManifest.LoaderManifestLoopStop(context);
            // If maxIterations is reached without the condition being met;
        }

    }
}