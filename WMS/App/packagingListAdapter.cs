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

namespace WMS.App
{

    class packagingListAdapter : BaseAdapter
    {
        public List<PackagingList> sList;
        private Context sContext;
        public packagingListAdapter(Context context, List<PackagingList> list)
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
                    row = LayoutInflater.From(sContext).Inflate(Resource.Layout.PackagingListAdapterView, null, false);
                }

                TextView Ident = row.FindViewById<TextView>(Resource.Id.Ident);
                Ident.Text = sList[position].Ident;
                Ident.SetTextColor(Android.Graphics.Color.Black);
                TextView Name = row.FindViewById<TextView>(Resource.Id.Name);
                Name.Text = sList[position].Name;
                Name.SetTextColor(Android.Graphics.Color.Black);
                TextView SerialNumber = row.FindViewById<TextView>(Resource.Id.SerialNumber);
                SerialNumber.Text = sList[position].SerialNumber;
                SerialNumber.SetTextColor(Android.Graphics.Color.Black);

                TextView Quantity = row.FindViewById<TextView>(Resource.Id.Quantity);
                Quantity.Text = sList[position].Quantity;
                Quantity.SetTextColor(Android.Graphics.Color.Black);
                TextView Position = row.FindViewById<TextView>(Resource.Id.Position);
                Position.Text = sList[position].Position;
                Position.SetTextColor(Android.Graphics.Color.Black);
                TextView SSCC = row.FindViewById<TextView>(Resource.Id.SSCC);
                SSCC.Text = sList[position].SSCC;
                SSCC.SetTextColor(Android.Graphics.Color.Black);



            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }


            return row;

        }


   

    }
}