namespace WMS.App
{
    public static class HelpfulMethods
    {
        private static int count = 0;

        public static bool preventDupUse()
        {
            var check = count;
            count += 1;

            if (count > 1)
            {
                count = 0;
                return false;
            }
            else

            {
                return true;
            }
        }

        public static void releaseLock()
        {
            count = 0;
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

        public static void clearTheStack(Activity activity)
        {
            activity.Finish();
        }
    }
}