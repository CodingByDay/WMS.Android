using Android.Content;
using Android.Views;

namespace WMS.App
{
    public class ClientPickingAdapter : BaseAdapter
    {
        public List<ClientPickingPosition> sList;
        private Context sContext;
        private ClientPickingPosition selected;
        private int selectedIndex = -1;
        public ClientPickingAdapter(Context context, List<ClientPickingPosition> list)
        {
            sList = list;
            sContext = context;
        }

        public ClientPickingPosition returnSelected()
        {
            return selected;
        }

        public void Filter(List<ClientPickingPosition> data, bool byIdent, string val, bool restart)
        {
            if (restart)
            {
                sList = data;
            }
            if (byIdent)
            {
                string searchFilter = val;
                if (val.StartsWith("P"))
                {
                    searchFilter = val.Substring(1);
                }
                sList = data.Where(data => data.Ident.Contains(searchFilter)).ToList();
            }
            else
            {
                sList = data.Where(data => data.Location.Contains(val)).ToList();
            }
            if (sList.Count == 1)
            {
                selected = sList.ElementAt(0);
                selectedIndex = 0;
            }
            base.NotifyDataSetChanged();
        }

        public List<ClientPickingPosition> returnData()
        {
            return sList;
        }

        public int returnNumberOfItems()
        {
            return sList.Count;
        }

        public void SetSelected(int position)
        {
            selected = sList[position];
            selectedIndex = position;
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
                    row = LayoutInflater.From(sContext).Inflate(Resource.Layout.ClientPickingRow, null, false);
                }

                TextView Ident = row.FindViewById<TextView>(Resource.Id.Ident);
                Ident.Text = sList[position].Ident;

                TextView Location = row.FindViewById<TextView>(Resource.Id.Location);
                Location.Text = sList[position].Location;

                TextView Quantity = row.FindViewById<TextView>(Resource.Id.Qty);
                Quantity.Text = sList[position].Quantity;

                TextView Order = row.FindViewById<TextView>(Resource.Id.Order);
                Order.Text = sList[position].Order;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }

            return row;
        }
    }
}