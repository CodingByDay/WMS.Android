using Android.Content;
using Android.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.App
{
    public static class UniversalAdapterHelper
    {

        public static UniversalAdapter<UnfinishedTakeoverList> GetUnfinishedTakeover(Context context, List<UnfinishedTakeoverList> data)
        {

            var adapter = new UniversalAdapter<UnfinishedTakeoverList>(context, data,
            Resource.Layout.FourElements,
            (view, item) =>
            {
                TextView Document = view.FindViewById<TextView>(Resource.Id.first);
                Document.Text = item.Document;
                Document.SetTextColor(Android.Graphics.Color.Black);

                TextView Issuer = view.FindViewById<TextView>(Resource.Id.second);
                Issuer.Text = item.Issuer;
                Issuer.SetTextColor(Android.Graphics.Color.Black);

                TextView Date = view.FindViewById<TextView>(Resource.Id.third);
                Date.Text = item.Date;
                Date.SetTextColor(Android.Graphics.Color.Black);

                TextView NumberOfPositions = view.FindViewById<TextView>(Resource.Id.fourth);
                NumberOfPositions.Text = item.NumberOfPositions;
                NumberOfPositions.SetTextColor(Android.Graphics.Color.Black);
            });
   


            return adapter;
        }
    }
}
