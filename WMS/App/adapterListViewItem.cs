using Stream = Android.Media.Stream;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.Lang;
using Exception = Java.Lang.Exception;

using AndroidX.AppCompat.App;using AlertDialog = Android.App.AlertDialog;namespace WMS.App
{
    public class adapterListViewItem : BaseAdapter
    {
        public List<ListViewItem> sList;
        private Context sContext;



        public adapterListViewItem(Context context, List<ListViewItem> list)
        {
            sList = list;
            sContext = context;
        }



        public override int Count
        {
            get
            {
                return sList.Count;
            }
        }

        public override Java.Lang.Object GetItem(int position)
        {
            return null;
        }

        public override long GetItemId(int position)
        {
            return position;
        }
        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            View row = convertView;
            try
            {
                if (row == null)
                {
                    row = LayoutInflater.From(sContext).Inflate(Resource.Layout.ListViewItem, null, false);
                }
                TextView tbSerialNumber = row.FindViewById<TextView>(Resource.Id.tbSerialNumber);
                tbSerialNumber.Text = sList[position].stKartona;


                TextView tbQuantity = row.FindViewById<TextView>(Resource.Id.tbQuantity);
                tbQuantity.Text = sList[position].quantity;

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }


            return row;

        }

        public void NotifyDataSetChanged()
        {


            NotifyDataSetChanged();
        }
    }

}