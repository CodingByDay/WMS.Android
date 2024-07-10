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



        public async static Task<bool> TabletHaltCorrectly(Context context)
        {
            LoaderManifest.LoaderManifestLoopResources(context);
            int iterations = 0;
            // Loop to check if text remains unchanged after 1 second intervals
            while (!Base.Store.OnlyOneSuggestion)
            { 
                if(iterations == 5)
                {
                    Base.Store.OnlyOneSuggestion = false;
                    return false;
                }
                await Task.Delay(1000); // Wait for 1 second
                iterations++;
            }
            LoaderManifest.LoaderManifestLoopStop(context);
            // If maxIterations is reached without the condition being met;
            Base.Store.OnlyOneSuggestion = true;
            return true;
        }

    }
}