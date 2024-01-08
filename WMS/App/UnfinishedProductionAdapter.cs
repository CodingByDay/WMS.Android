using Android.Content;
using Android.Views;

namespace WMS.App
{
    internal class UnfinishedProductionAdapter : BaseAdapter
    {
        public List<UnfinishedProductionList> sList;
        private Context sContext;

        public UnfinishedProductionAdapter(Context context, List<UnfinishedProductionList> list)
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
                    row = LayoutInflater.From(sContext).Inflate(Resource.Layout.UnfinishedProduction, null, false);
                }

                TextView WorkOrder = row.FindViewById<TextView>(Resource.Id.WorkOrder);
                WorkOrder.Text = sList[position].WorkOrder;
                WorkOrder.SetTextColor(Android.Graphics.Color.Black);

                TextView Orderer = row.FindViewById<TextView>(Resource.Id.Orderer);
                Orderer.Text = sList[position].Orderer;
                Orderer.SetTextColor(Android.Graphics.Color.Black);
                TextView Ident = row.FindViewById<TextView>(Resource.Id.Ident);
                Ident.Text = sList[position].Ident;
                Ident.SetTextColor(Android.Graphics.Color.Black);

                TextView NumberOfPositions = row.FindViewById<TextView>(Resource.Id.NumberOfPositions);
                NumberOfPositions.Text = sList[position].NumberOfPositions;
                NumberOfPositions.SetTextColor(Android.Graphics.Color.Black);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }

            return row;
        }
    }
}