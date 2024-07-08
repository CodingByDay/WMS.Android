using Android.Views;

namespace WMS.App
{
    public static class HelperMethods
    {
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




    }
}