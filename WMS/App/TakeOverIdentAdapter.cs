using Android.Content;
using Android.Views;

namespace WMS.App
{
    internal class TakeOverIdentAdapter : BaseAdapter
    {
        public List<TakeOverIdentList> sList;
        private Context sContext;

        public TakeOverIdentAdapter(Context context, List<TakeOverIdentList> list)
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
                    row = LayoutInflater.From(sContext).Inflate(Resource.Layout.TakeOverIdentListView, null, false);
                }

                TextView Ident = row.FindViewById<TextView>(Resource.Id.Ident);
                Ident.Text = sList[position].Ident;
                Ident.SetTextColor(Android.Graphics.Color.Black);

                TextView Order = row.FindViewById<TextView>(Resource.Id.Order);
                Order.Text = sList[position].Order;
                Order.SetTextColor(Android.Graphics.Color.Black);

                TextView Position = row.FindViewById<TextView>(Resource.Id.Position);
                Position.Text = sList[position].Position.ToString();
                Position.SetTextColor(Android.Graphics.Color.Black);

                TextView Subject = row.FindViewById<TextView>(Resource.Id.Subject);
                Subject.Text = sList[position].Subject;
                Subject.SetTextColor(Android.Graphics.Color.Black);

                TextView Quantity = row.FindViewById<TextView>(Resource.Id.Quantity);
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