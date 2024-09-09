using Android.Content;
using Android.Views;
using Exception = Java.Lang.Exception;

namespace WMS.App
{
    public class TrailAdapter : BaseAdapter
    {
        public List<Trail> sList;
        private Context sContext;
        private int selectedIndex = -1;
        private Trail selected;

        public override int Count => sList.Count;

        public TrailAdapter(Context context, List<Trail> list)
        {
            sList = list;
            sContext = context;
        }

        public Trail ReturnSelected()
        {
            return selected;
        }



        public void SetSelected(int position)
        {
            selected = sList[position];
            selectedIndex = position;
            NotifyDataSetChanged();
        }

  
        public int GetIdFromAdapter(Trail trail)
        {
            int result = -1;
            int counter = 0;
            foreach(Trail item in sList)
            {
                if (item == trail)
                {
                    result = counter;
                }

                counter += 1;
            }

            return result;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            View row = convertView;
            try
            {
                if (row == null)
                {
                    row = LayoutInflater.From(sContext).Inflate(Resource.Layout.ListViewTrail, null, false);
                }
                TextView Ident = row.FindViewById<TextView>(Resource.Id.Ident);
                Ident.Text = sList[position].Ident;

                TextView Location = row.FindViewById<TextView>(Resource.Id.Location);
                Location.Text = sList[position].Location;

                TextView Qty = row.FindViewById<TextView>(Resource.Id.Qty);
                Qty.Text = sList[position].Qty;

                TextView Name = row.FindViewById<TextView>(Resource.Id.Name);
                Name.Text = sList[position].Name;


                if (position == selectedIndex)
                {
                    row.SetBackgroundColor(Android.Graphics.Color.ParseColor("#80000000")); // screen_background_dark_transparent equivalent 5. jul .2024 Janko Jovičić

                }
                else
                {
                    row.SetBackgroundColor(Android.Graphics.Color.Transparent); 
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }

            return row;
        }

        public void Filter(List<Trail> data, bool byIdent, string val, bool restart)
        {
            selected = null;
            selectedIndex = -1;
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
            if(sList.Count == 1)
            {
                selected = sList.ElementAt(0);
                selectedIndex = 0;
            }
            base.NotifyDataSetChanged();
        }

        public List<Trail> returnData()
        {
            return sList;
        }

        public int returnNumberOfItems()
        {
            return sList.Count;
        }

        public override void NotifyDataSetChanged()
        {
            base.NotifyDataSetChanged();
        }

        public override Java.Lang.Object? GetItem(int position)
        {
            return 1111;
        }

        public override long GetItemId(int position)
        {
            return 1111;
        }
    }
}