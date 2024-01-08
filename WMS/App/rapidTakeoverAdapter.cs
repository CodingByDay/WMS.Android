using Android.Content;
using Android.Views;

namespace WMS.App
{
    internal class rapidTakeoverAdapter : BaseAdapter
    {
        public List<rapidTakeoverList> sList;
        private Context sContext;

        public rapidTakeoverAdapter(Context context, List<rapidTakeoverList> list)
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
                    row = LayoutInflater.From(sContext).Inflate(Resource.Layout.rapidTakeoverListViewList, null, false);
                }

                TextView Location = row.FindViewById<TextView>(Resource.Id.Location);
                Location.Text = sList[position].Location;
                Location.SetTextColor(Android.Graphics.Color.Black);

                TextView SSCC = row.FindViewById<TextView>(Resource.Id.SSCC);
                SSCC.Text = sList[position].SSCC;
                SSCC.SetTextColor(Android.Graphics.Color.Black);
                TextView Ident = row.FindViewById<TextView>(Resource.Id.Ident);
                Ident.Text = sList[position].Ident;
                Ident.SetTextColor(Android.Graphics.Color.Black);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }

            return row;
        }
    }
}