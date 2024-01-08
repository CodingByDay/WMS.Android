using Android.Content;
using Android.Views;

namespace WMS.App
{
    internal class ProductionSerialOrSSCCAdapter : BaseAdapter
    {
        public List<ProductionSerialOrSSCCList> sList;
        private Context sContext;

        public ProductionSerialOrSSCCAdapter(Context context, List<ProductionSerialOrSSCCList> list)
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
                    row = LayoutInflater.From(sContext).Inflate(Resource.Layout.ProductionSerialOrSSCCListView, null, false);
                }

                TextView Ident = row.FindViewById<TextView>(Resource.Id.Ident);
                Ident.Text = sList[position].Ident;
                Ident.SetTextColor(Android.Graphics.Color.Black);

                TextView SerialNumber = row.FindViewById<TextView>(Resource.Id.SerialNumber);
                SerialNumber.Text = sList[position].SerialNumber;
                SerialNumber.SetTextColor(Android.Graphics.Color.Black);

                TextView Location = row.FindViewById<TextView>(Resource.Id.Location);
                Location.Text = sList[position].Location;
                Location.SetTextColor(Android.Graphics.Color.Black);
                TextView Qty = row.FindViewById<TextView>(Resource.Id.Qty);
                Qty.Text = sList[position].Qty;
                Qty.SetTextColor(Android.Graphics.Color.Black);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }

            return row;
        }
    }
}