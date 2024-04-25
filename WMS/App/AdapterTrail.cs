using Android.Content;
using Android.Views;

namespace WMS.App
{
    internal class AdapterTrail : BaseAdapter
    {
        public List<Trail> sList;
        private Context sContext;

        public AdapterTrail(Context context, List<Trail> list)
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
                    row = LayoutInflater.From(sContext).Inflate(Resource.Layout.TrailView, null, false);
                }

                TextView Ident = row.FindViewById<TextView>(Resource.Id.ident);
                Ident.Text = sList[position].Ident;
                Ident.SetTextColor(Android.Graphics.Color.Black);

                TextView Name = row.FindViewById<TextView>(Resource.Id.name);
                Name.Text = sList[position].Name;
                Name.SetTextColor(Android.Graphics.Color.Black);

                TextView Position = row.FindViewById<TextView>(Resource.Id.position);
                Position.Text = sList[position].No.ToString();
                Position.SetTextColor(Android.Graphics.Color.Black);

                TextView Location = row.FindViewById<TextView>(Resource.Id.location);
                Location.Text = sList[position].Location;
                Location.SetTextColor(Android.Graphics.Color.Black);

                TextView Quantity = row.FindViewById<TextView>(Resource.Id.quantity);
                Quantity.Text = sList[position].Qty;
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