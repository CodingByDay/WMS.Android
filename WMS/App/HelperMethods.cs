using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Stream = Android.Media.Stream;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using AndroidX.AppCompat.App;using AlertDialog = Android.App.AlertDialog;namespace WMS.App
{
    public static class HelperMethods
    {


        public static bool is2D(string code)
        {
            if(code.Contains("1T")&&code.Contains("K")&&code.Contains("4Q"))
            {
                return true;
            } else
            {
                return false;
            }
        }





        public static bool is1D(string barcode)
        {
            string input = barcode; // this is your input string
            char[] chars = input.ToCharArray();
            List<char> buffer = new List<char>();

            foreach (var c in chars)
            {
                if (char.IsControl(c)||char.IsSeparator(c)||char.IsSymbol(c))
                {
                    return true;
                }
                else
                {
                    buffer.Add(c);
                }
            }

            return false;
        }
    }
}