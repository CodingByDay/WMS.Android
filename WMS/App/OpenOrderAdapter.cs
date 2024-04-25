using Android.Content;
using Android.Views;

namespace WMS.App
{
    internal class OpenOrderAdapter : BaseAdapter
    {
        public List<OpenOrder> sList;
        private Context sContext;

        public OpenOrderAdapter(Context context, List<OpenOrder> list)
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
                    row = LayoutInflater.From(sContext).Inflate(Resource.Layout.OpenOrderView, null, false);
                }

                TextView Ident = row.FindViewById<TextView>(Resource.Id.ident);
                Ident.Text = sList[position].Ident;
                Ident.SetTextColor(Android.Graphics.Color.Black);

                TextView Client = row.FindViewById<TextView>(Resource.Id.client);
                Client.Text = sList[position].Client;
                Client.SetTextColor(Android.Graphics.Color.Black);

                TextView Position = row.FindViewById<TextView>(Resource.Id.position);
                Position.Text = sList[position].Position.ToString();
                Position.SetTextColor(Android.Graphics.Color.Black);


                TextView Quantity = row.FindViewById<TextView>(Resource.Id.quantity);
                Quantity.Text = sList[position].Quantity.ToString();
                Quantity.SetTextColor(Android.Graphics.Color.Black);


            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }

            return row;
        }
    }
}