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

using AndroidX.AppCompat.App;using AlertDialog = Android.App.AlertDialog;namespace WMS.App
{
    class TakeOverIdentAdapter : BaseAdapter
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

                TextView Name = row.FindViewById<TextView>(Resource.Id.Name);
                Name.Text = sList[position].Name;
                Name.SetTextColor(Android.Graphics.Color.Black);
                TextView Open = row.FindViewById<TextView>(Resource.Id.Open);
                Open.Text = sList[position].Open;
                Open.SetTextColor(Android.Graphics.Color.Black);
                TextView Ordered = row.FindViewById<TextView>(Resource.Id.Ordered);
                Ordered.Text = sList[position].Ordered;
                Ordered.SetTextColor(Android.Graphics.Color.Black);
                TextView Received = row.FindViewById<TextView>(Resource.Id.Received);
                Received.Text = sList[position].Received;
                Received.SetTextColor(Android.Graphics.Color.Black);


            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }


            return row;

        }


    

    }
}