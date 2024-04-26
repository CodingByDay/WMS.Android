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





        public static UniversalAdapter<UnfinishedProductionList> GetUnfinishedProduction(Context context, List<UnfinishedProductionList> data)
        {

            var adapter = new UniversalAdapter<UnfinishedProductionList>(context, data,
            Resource.Layout.FourElements,
            (view, item) =>
            {
                TextView WorkOrder = view.FindViewById<TextView>(Resource.Id.first);
                WorkOrder.Text = item.WorkOrder;
                WorkOrder.SetTextColor(Android.Graphics.Color.Black);

                TextView Orderer = view.FindViewById<TextView>(Resource.Id.second);
                Orderer.Text = item.Orderer;
                Orderer.SetTextColor(Android.Graphics.Color.Black);

                TextView Ident = view.FindViewById<TextView>(Resource.Id.third);
                Ident.Text = item.Ident;
                Ident.SetTextColor(Android.Graphics.Color.Black);

                TextView NumberOfPositions = view.FindViewById<TextView>(Resource.Id.fourth);
                NumberOfPositions.Text = item.NumberOfPositions;
                NumberOfPositions.SetTextColor(Android.Graphics.Color.Black);
            });



            return adapter;
        }



        public static UniversalAdapter<UnfinishedIssuedList> GetUnfinishedIssued(Context context, List<UnfinishedIssuedList> data)
        {

            var adapter = new UniversalAdapter<UnfinishedIssuedList>(context, data,
            Resource.Layout.FourElements,
            (view, item) =>
            {
                TextView Document = view.FindViewById<TextView>(Resource.Id.first);
                Document.Text = item.Document;
                Document.SetTextColor(Android.Graphics.Color.Black);

                TextView Orderer = view.FindViewById<TextView>(Resource.Id.second);
                Orderer.Text = item.Orderer;
                Orderer.SetTextColor(Android.Graphics.Color.Black);

                TextView Date = view.FindViewById<TextView>(Resource.Id.third);
                Date.Text = item.Date;
                Date.SetTextColor(Android.Graphics.Color.Black);

                TextView NumberOfPositions = view.FindViewById<TextView>(Resource.Id.fourth);
                NumberOfPositions.Text = item.NumberOfPositions;
                NumberOfPositions.SetTextColor(Android.Graphics.Color.Black);
            });



            return adapter;
        }



        public static UniversalAdapter<UnfinishedInterWarehouseList> GetUnfinishedInterwarehouse(Context context, List<UnfinishedInterWarehouseList> data)
        {

            var adapter = new UniversalAdapter<UnfinishedInterWarehouseList>(context, data,
            Resource.Layout.FourElements,
            (view, item) =>
            {
                TextView Document = view.FindViewById<TextView>(Resource.Id.first);
                Document.Text = item.Document;
                Document.SetTextColor(Android.Graphics.Color.Black);

                TextView CreatedBy = view.FindViewById<TextView>(Resource.Id.second);
                CreatedBy.Text = item.CreatedBy;
                CreatedBy.SetTextColor(Android.Graphics.Color.Black);

                TextView Date = view.FindViewById<TextView>(Resource.Id.third);
                Date.Text = item.Date;
                Date.SetTextColor(Android.Graphics.Color.Black);

                TextView NumberOfPositions = view.FindViewById<TextView>(Resource.Id.fourth);
                NumberOfPositions.Text = item.NumberOfPositions;
                NumberOfPositions.SetTextColor(Android.Graphics.Color.Black);
            });



            return adapter;
        }




        public static UniversalAdapter<UnfinishedInterWarehouseList> GetTakeoverSerialOrSSCCEntry(Context context, List<UnfinishedInterWarehouseList> data)
        {

            var adapter = new UniversalAdapter<UnfinishedInterWarehouseList>(context, data,
            Resource.Layout.FourElements,
            (view, item) =>
            {
                TextView Document = view.FindViewById<TextView>(Resource.Id.first);
                Document.Text = item.Document;
                Document.SetTextColor(Android.Graphics.Color.Black);

                TextView CreatedBy = view.FindViewById<TextView>(Resource.Id.second);
                CreatedBy.Text = item.CreatedBy;
                CreatedBy.SetTextColor(Android.Graphics.Color.Black);

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
