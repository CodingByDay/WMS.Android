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

    class AdapterIssued : BaseAdapter
    {
        public List<IssuedClass> sList;
        private Context sContext;
        public AdapterIssued(Context context, List<IssuedClass> list)
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
                    row = LayoutInflater.From(sContext).Inflate(Resource.Layout.IssuedView, null, false);
                }

                TextView Ident = row.FindViewById<TextView>(Resource.Id.ident);
                Ident.Text = sList[position].ident;
                Ident.SetTextColor(Android.Graphics.Color.Black);
                TextView Name = row.FindViewById<TextView>(Resource.Id.name);
                Name.Text = sList[position].name;
                Name.SetTextColor(Android.Graphics.Color.Black);
                TextView Ordered = row.FindViewById<TextView>(Resource.Id.ordered);
                Ordered.Text = sList[position].ordered;
                Ordered.SetTextColor(Android.Graphics.Color.Black);
                TextView Issued = row.FindViewById<TextView>(Resource.Id.issued);
                Issued.Text = sList[position].issued;
                Issued.SetTextColor(Android.Graphics.Color.Black);
                TextView Open = row.FindViewById<TextView>(Resource.Id.open);
                Open.Text = sList[position].open;
                Open.SetTextColor(Android.Graphics.Color.Black);






            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }


            return row;

        }


     

    }
}