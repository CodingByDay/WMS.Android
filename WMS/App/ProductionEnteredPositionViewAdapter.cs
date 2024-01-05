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

    class ProductionEnteredPositionViewAdapter : BaseAdapter
    {
        public List<ProductionEnteredPositionList> sList;
        private Context sContext;
        public ProductionEnteredPositionViewAdapter(Context context, List<ProductionEnteredPositionList> list)
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
                    row = LayoutInflater.From(sContext).Inflate(Resource.Layout.ProductionEnteredPositionViewList, null, false);
                }

                TextView Ident = row.FindViewById<TextView>(Resource.Id.Ident);
                Ident.Text = sList[position].Ident;
                Ident.SetTextColor(Android.Graphics.Color.Black);

                TextView Quantity = row.FindViewById<TextView>(Resource.Id.Quantity);
                Quantity.Text = sList[position].Quantity;
                Quantity.SetTextColor(Android.Graphics.Color.Black);

                TextView Location = row.FindViewById<TextView>(Resource.Id.Location);
                Location.Text = sList[position].Location;
                Location.SetTextColor(Android.Graphics.Color.Black);
                TextView SerialNumber = row.FindViewById<TextView>(Resource.Id.SerialNumber);
                SerialNumber.Text = sList[position].SerialNumber;
                SerialNumber.SetTextColor(Android.Graphics.Color.Black);
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