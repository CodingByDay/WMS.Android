using Android.Content;
using Android.OS;

namespace WMS.App
{
    public static class HelpfulMethods
    {

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


        public static object RunSafelyOnUIThreadReturnResult(Context context, Func<object> action)
        {
            object result = null;

            var uiHandler = new Handler(context.MainLooper);
            var uiThread = new Java.Lang.Thread(() =>
            {
                uiHandler.Post(() =>
                {
                    result = action();
                });
            });
            uiThread.Start();
            uiThread.Join(); // Wait for the UI thread to finish

            return result;
        }

    }
}