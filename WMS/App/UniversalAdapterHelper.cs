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




        public static UniversalAdapter<TakeoverDocument> GetTakeoverSerialOrSSCCEntry(Context context, List<TakeoverDocument> data)
        {

            var adapter = new UniversalAdapter<TakeoverDocument>(context, data,
            Resource.Layout.FiveElements,
            (view, item) =>
            {
                TextView Ident = view.FindViewById<TextView>(Resource.Id.first);
                Ident.Text = item.ident;
                Ident.SetTextColor(Android.Graphics.Color.Black);

                TextView SSCC = view.FindViewById<TextView>(Resource.Id.second);
                SSCC.Text = item.sscc;
                SSCC.SetTextColor(Android.Graphics.Color.Black);

                TextView Serial = view.FindViewById<TextView>(Resource.Id.third);
                Serial.Text = item.serial;
                Serial.SetTextColor(Android.Graphics.Color.Black);

                TextView Quantity = view.FindViewById<TextView>(Resource.Id.fourth);
                Quantity.Text = item.quantity;
                Quantity.SetTextColor(Android.Graphics.Color.Black);

                TextView Location = view.FindViewById<TextView>(Resource.Id.fifth);
                Location.Text = item.location;
                Location.SetTextColor(Android.Graphics.Color.Black);
            });



            return adapter;
        }



        public static UniversalAdapter<TakeOverIdentList> GetTakeoverIdentEntry(Context context, List<TakeOverIdentList> data)
        {

            var adapter = new UniversalAdapter<TakeOverIdentList>(context, data,
            Resource.Layout.FiveElements,
            (view, item) =>
            {
                TextView Ident = view.FindViewById<TextView>(Resource.Id.first);
                Ident.Text = item.Ident;
                Ident.SetTextColor(Android.Graphics.Color.Black);

                TextView Order = view.FindViewById<TextView>(Resource.Id.second);
                Order.Text = item.Order;
                Order.SetTextColor(Android.Graphics.Color.Black);

                TextView Position = view.FindViewById<TextView>(Resource.Id.third);
                Position.Text = item.Position.ToString();
                Position.SetTextColor(Android.Graphics.Color.Black);

                TextView Subject = view.FindViewById<TextView>(Resource.Id.fourth);
                Subject.Text = item.Subject.ToString();
                Subject.SetTextColor(Android.Graphics.Color.Black);

                TextView Quantity = view.FindViewById<TextView>(Resource.Id.fifth);
                Quantity.Text = item.Quantity.ToString();
                Quantity.SetTextColor(Android.Graphics.Color.Black);
            });



            return adapter;
        }



        public static UniversalAdapter<TakeOverEnteredPositionsViewListItems> GetTakeOverEnteredPositionsView(Context context, List<TakeOverEnteredPositionsViewListItems> data)
        {

            var adapter = new UniversalAdapter<TakeOverEnteredPositionsViewListItems>(context, data,
            Resource.Layout.SixElements,
            (view, item) =>
            {
                TextView Ident = view.FindViewById<TextView>(Resource.Id.first);
                Ident.Text = item.Ident;
                Ident.SetTextColor(Android.Graphics.Color.Black);

                TextView Name = view.FindViewById<TextView>(Resource.Id.second);
                Name.Text = item.Name;
                Name.SetTextColor(Android.Graphics.Color.Black);

                TextView Quantity = view.FindViewById<TextView>(Resource.Id.third);
                Quantity.Text = item.Quantity;
                Quantity.SetTextColor(Android.Graphics.Color.Black);

                TextView Position = view.FindViewById<TextView>(Resource.Id.fourth);
                Position.Text = item.Position;
                Position.SetTextColor(Android.Graphics.Color.Black);

                TextView SerialNumber = view.FindViewById<TextView>(Resource.Id.fifth);
                SerialNumber.Text = item.SerialNumber;
                SerialNumber.SetTextColor(Android.Graphics.Color.Black);

                TextView SSCC = view.FindViewById<TextView>(Resource.Id.sixth);
                SSCC.Text = item.SSCC;
                SSCC.SetTextColor(Android.Graphics.Color.Black);
            });



            return adapter;
        }



        public static UniversalAdapter<ProductionSerialOrSSCCList> GetProductionSerialOrSSCCEntry(Context context, List<ProductionSerialOrSSCCList> data)
        {

            var adapter = new UniversalAdapter<ProductionSerialOrSSCCList>(context, data,
            Resource.Layout.FiveElements,
            (view, item) =>
            {
                TextView Ident = view.FindViewById<TextView>(Resource.Id.first);
                Ident.Text = item.Ident;
                Ident.SetTextColor(Android.Graphics.Color.Black);

                TextView SerialNumber = view.FindViewById<TextView>(Resource.Id.second);
                SerialNumber.Text = item.SerialNumber;
                SerialNumber.SetTextColor(Android.Graphics.Color.Black);

                TextView Location = view.FindViewById<TextView>(Resource.Id.third);
                Location.Text = item.Location;
                Location.SetTextColor(Android.Graphics.Color.Black);

                TextView Qty = view.FindViewById<TextView>(Resource.Id.fourth);
                Qty.Text = item.Qty;
                Qty.SetTextColor(Android.Graphics.Color.Black);

                TextView Filled = view.FindViewById<TextView>(Resource.Id.fifth);
                Filled.Text = item.Filled;
                Filled.SetTextColor(Android.Graphics.Color.Black);

            });



            return adapter;
        }


        public static UniversalAdapter<ProductionEnteredPositionList> GetProductionEnteredPositionsView(Context context, List<ProductionEnteredPositionList> data)
        {

            var adapter = new UniversalAdapter<ProductionEnteredPositionList>(context, data,
            Resource.Layout.FiveElements,
            (view, item) =>
            {
                TextView Ident = view.FindViewById<TextView>(Resource.Id.first);
                Ident.Text = item.Ident;
                Ident.SetTextColor(Android.Graphics.Color.Black);

                TextView Quantity = view.FindViewById<TextView>(Resource.Id.second);
                Quantity.Text = item.Quantity;
                Quantity.SetTextColor(Android.Graphics.Color.Black);

                TextView Location = view.FindViewById<TextView>(Resource.Id.third);
                Location.Text = item.Location;
                Location.SetTextColor(Android.Graphics.Color.Black);

                TextView SerialNumber = view.FindViewById<TextView>(Resource.Id.fourth);
                SerialNumber.Text = item.SerialNumber;
                SerialNumber.SetTextColor(Android.Graphics.Color.Black);

                TextView SSCC = view.FindViewById<TextView>(Resource.Id.fifth);
                SSCC.Text = item.SSCC;
                SSCC.SetTextColor(Android.Graphics.Color.Black);

            });



            return adapter;
        }

        public static UniversalAdapter<CleanupLocation> GetMainMenu(Context context, List<CleanupLocation> data)
        {

            var adapter = new UniversalAdapter<CleanupLocation>(context, data,
            Resource.Layout.FiveElements,
            (view, item) =>
            {
                TextView Name = view.FindViewById<TextView>(Resource.Id.first);
                Name.Text = item.Ident;
                Name.SetTextColor(Android.Graphics.Color.Black);

                TextView Ident = view.FindViewById<TextView>(Resource.Id.second);
                Ident.Text = item.Ident;
                Ident.SetTextColor(Android.Graphics.Color.Black);

                TextView Location = view.FindViewById<TextView>(Resource.Id.third);
                Location.Text = item.Location;
                Location.SetTextColor(Android.Graphics.Color.Black);

                TextView SSCC = view.FindViewById<TextView>(Resource.Id.fourth);
                SSCC.Text = item.SSCC;
                SSCC.SetTextColor(Android.Graphics.Color.Black);

                TextView Serial = view.FindViewById<TextView>(Resource.Id.fifth);
                Serial.Text = item.Serial;
                Serial.SetTextColor(Android.Graphics.Color.Black);

            });



            return adapter;
        }


        public static UniversalAdapter<LocationClass> GetIssuedGoodsSerialOrSSCCEntryClientPicking(Context context, List<LocationClass> data)
        {

            var adapter = new UniversalAdapter<LocationClass>(context, data,
            Resource.Layout.FiveElements,
            (view, item) =>
            {
                TextView Ident = view.FindViewById<TextView>(Resource.Id.first);
                Ident.Text = item.ident;
                Ident.SetTextColor(Android.Graphics.Color.Black);

                TextView Quantity = view.FindViewById<TextView>(Resource.Id.second);
                Quantity.Text = item.quantity;
                Quantity.SetTextColor(Android.Graphics.Color.Black);

                TextView Location = view.FindViewById<TextView>(Resource.Id.third);
                Location.Text = item.location;
                Location.SetTextColor(Android.Graphics.Color.Black);

                TextView Serial = view.FindViewById<TextView>(Resource.Id.fourth);
                Serial.Text = item.serial;
                Serial.SetTextColor(Android.Graphics.Color.Black);

                TextView SSCC = view.FindViewById<TextView>(Resource.Id.fifth);
                SSCC.Text = item.sscc;
                SSCC.SetTextColor(Android.Graphics.Color.Black);

            });



            return adapter;
        }



        public static UniversalAdapter<LocationClass> GetIssuedGoodsSerialOrSSCCEntry(Context context, List<LocationClass> data)
        {

            var adapter = new UniversalAdapter<LocationClass>(context, data,
            Resource.Layout.FiveElements,
            (view, item) =>
            {
                TextView Ident = view.FindViewById<TextView>(Resource.Id.first);
                Ident.Text = item.ident;
                Ident.SetTextColor(Android.Graphics.Color.Black);

                TextView Quantity = view.FindViewById<TextView>(Resource.Id.second);
                Quantity.Text = item.quantity;
                Quantity.SetTextColor(Android.Graphics.Color.Black);

                TextView Location = view.FindViewById<TextView>(Resource.Id.third);
                Location.Text = item.location;
                Location.SetTextColor(Android.Graphics.Color.Black);

                TextView Serial = view.FindViewById<TextView>(Resource.Id.fourth);
                Serial.Text = item.serial;
                Serial.SetTextColor(Android.Graphics.Color.Black);

                TextView SSCC = view.FindViewById<TextView>(Resource.Id.fifth);
                SSCC.Text = item.sscc;
                SSCC.SetTextColor(Android.Graphics.Color.Black);

            });



            return adapter;
        }


        public static UniversalAdapter<LocationClass> GetInterWarehouseSerialOrSSCCEntry(Context context, List<LocationClass> data)
        {

            var adapter = new UniversalAdapter<LocationClass>(context, data,
            Resource.Layout.FiveElements,
            (view, item) =>
            {
                TextView Ident = view.FindViewById<TextView>(Resource.Id.first);
                Ident.Text = item.ident;
                Ident.SetTextColor(Android.Graphics.Color.Black);

                TextView Quantity = view.FindViewById<TextView>(Resource.Id.second);
                Quantity.Text = item.quantity;
                Quantity.SetTextColor(Android.Graphics.Color.Black);

                TextView Location = view.FindViewById<TextView>(Resource.Id.third);
                Location.Text = item.location;
                Location.SetTextColor(Android.Graphics.Color.Black);

                TextView Serial = view.FindViewById<TextView>(Resource.Id.fourth);
                Serial.Text = item.serial;
                Serial.SetTextColor(Android.Graphics.Color.Black);

                TextView SSCC = view.FindViewById<TextView>(Resource.Id.fifth);
                SSCC.Text = item.sscc;
                SSCC.SetTextColor(Android.Graphics.Color.Black);

            });



            return adapter;
        }


        public static UniversalAdapter<Trail> GetIssueGoodsIdentWithTrail(Context context, List<Trail> data)
        {

            var adapter = new UniversalAdapter<Trail>(context, data,
            Resource.Layout.FiveElements,
            (view, item) =>
            {
                TextView Ident = view.FindViewById<TextView>(Resource.Id.first);
                Ident.Text = item.Ident;
                Ident.SetTextColor(Android.Graphics.Color.Black);

                TextView Name = view.FindViewById<TextView>(Resource.Id.second);
                Name.Text = item.Name;
                Name.SetTextColor(Android.Graphics.Color.Black);

                TextView Position = view.FindViewById<TextView>(Resource.Id.third);
                Position.Text = item.No.ToString();
                Position.SetTextColor(Android.Graphics.Color.Black);

                TextView Location = view.FindViewById<TextView>(Resource.Id.fourth);
                Location.Text = item.Location;
                Location.SetTextColor(Android.Graphics.Color.Black);

                TextView Quantity = view.FindViewById<TextView>(Resource.Id.fifth);
                Quantity.Text = item.Qty;
                Quantity.SetTextColor(Android.Graphics.Color.Black);

            });



            return adapter;
        }





        public static UniversalAdapter<OpenOrder> GetIssuedGoodsIdentEntry(Context context, List<OpenOrder> data)
        {

            var adapter = new UniversalAdapter<OpenOrder>(context, data,
            Resource.Layout.SixElements,
            (view, item) =>
            {
                TextView Ident = view.FindViewById<TextView>(Resource.Id.first);
                Ident.Text = item.Ident;
                Ident.SetTextColor(Android.Graphics.Color.Black);

                TextView Order = view.FindViewById<TextView>(Resource.Id.second);
                Order.Text = item.Order;
                Order.SetTextColor(Android.Graphics.Color.Black);

                TextView Position = view.FindViewById<TextView>(Resource.Id.third);
                Position.Text = item.Position.ToString();
                Position.SetTextColor(Android.Graphics.Color.Black);

                TextView Client = view.FindViewById<TextView>(Resource.Id.fourth);
                Client.Text = item.Client;
                Client.SetTextColor(Android.Graphics.Color.Black);

                TextView Date = view.FindViewById<TextView>(Resource.Id.fifth);
                Date.Text = item.Date.ToString();
                Date.SetTextColor(Android.Graphics.Color.Black);

                TextView Quantity = view.FindViewById<TextView>(Resource.Id.sixth);
                Quantity.Text = item.Quantity.ToString();
                Quantity.SetTextColor(Android.Graphics.Color.Black);

            });



            return adapter;
        }


        public static UniversalAdapter<IssuedEnteredPositionViewList> GetIssuedGoodsEnteredPositionsView(Context context, List<IssuedEnteredPositionViewList> data)
        {

            var adapter = new UniversalAdapter<IssuedEnteredPositionViewList>(context, data,
            Resource.Layout.SixElements,
            (view, item) =>
            {
                TextView Ident = view.FindViewById<TextView>(Resource.Id.first);
                Ident.Text = item.Ident;
                Ident.SetTextColor(Android.Graphics.Color.Black);

                TextView Name = view.FindViewById<TextView>(Resource.Id.second);
                Name.Text = item.Name;
                Name.SetTextColor(Android.Graphics.Color.Black);

                TextView Quantity = view.FindViewById<TextView>(Resource.Id.third);
                Quantity.Text = item.Quantity;
                Quantity.SetTextColor(Android.Graphics.Color.Black);

                TextView Position = view.FindViewById<TextView>(Resource.Id.fourth);
                Position.Text = item.Position;
                Position.SetTextColor(Android.Graphics.Color.Black);

                TextView SerialNumber = view.FindViewById<TextView>(Resource.Id.fifth);
                SerialNumber.Text = item.SerialNumber;
                SerialNumber.SetTextColor(Android.Graphics.Color.Black);

                TextView SSCC = view.FindViewById<TextView>(Resource.Id.sixth);
                SSCC.Text = item.SSCC;
                SSCC.SetTextColor(Android.Graphics.Color.Black);

            });



            return adapter;
        }


        public static UniversalAdapter<InterWarehouseEnteredPositionsViewList> GetInterWarehouseEnteredPositionsView(Context context, List<InterWarehouseEnteredPositionsViewList> data)
        {

            var adapter = new UniversalAdapter<InterWarehouseEnteredPositionsViewList>(context, data,
            Resource.Layout.SixElements,
            (view, item) =>
            {
                TextView Ident = view.FindViewById<TextView>(Resource.Id.first);
                Ident.Text = item.Ident;
                Ident.SetTextColor(Android.Graphics.Color.Black);

                TextView Name = view.FindViewById<TextView>(Resource.Id.second);
                Name.Text = item.Name;
                Name.SetTextColor(Android.Graphics.Color.Black);

                TextView Quantity = view.FindViewById<TextView>(Resource.Id.third);
                Quantity.Text = item.Quantity;
                Quantity.SetTextColor(Android.Graphics.Color.Black);

                TextView Position = view.FindViewById<TextView>(Resource.Id.fourth);
                Position.Text = item.Position;
                Position.SetTextColor(Android.Graphics.Color.Black);

                TextView SerialNumber = view.FindViewById<TextView>(Resource.Id.fifth);
                SerialNumber.Text = item.SerialNumber;
                SerialNumber.SetTextColor(Android.Graphics.Color.Black);

                TextView SSCC = view.FindViewById<TextView>(Resource.Id.sixth);
                SSCC.Text = item.SSCC;
                SSCC.SetTextColor(Android.Graphics.Color.Black);

            });



            return adapter;
        }


        public static UniversalAdapter<ClientPickingPosition> GetClientPicking(Context context, List<ClientPickingPosition> data)
        {

            var adapter = new UniversalAdapter<ClientPickingPosition>(context, data,
            Resource.Layout.SixElements,
            (view, item) =>
            {
                TextView Ident = view.FindViewById<TextView>(Resource.Id.first);
                Ident.Text = item.Ident;
                Ident.SetTextColor(Android.Graphics.Color.Black);

                TextView Name = view.FindViewById<TextView>(Resource.Id.second);
                Name.Text = item.Name;
                Name.SetTextColor(Android.Graphics.Color.Black);

                TextView Location = view.FindViewById<TextView>(Resource.Id.third);
                Location.Text = item.Location;
                Location.SetTextColor(Android.Graphics.Color.Black);

                TextView Quantity = view.FindViewById<TextView>(Resource.Id.fourth);
                Quantity.Text = item.Quantity;
                Quantity.SetTextColor(Android.Graphics.Color.Black);

                TextView Order = view.FindViewById<TextView>(Resource.Id.fifth);
                Order.Text = item.Order;
                Order.SetTextColor(Android.Graphics.Color.Black);

                TextView No = view.FindViewById<TextView>(Resource.Id.sixth);
                No.Text = item.No.ToString();
                No.SetTextColor(Android.Graphics.Color.Black);

            });



            return adapter;
        }



        public static UniversalAdapter<CheckStockAddonList> GetCheckStock(Context context, List<CheckStockAddonList> data)
        {

            var adapter = new UniversalAdapter<CheckStockAddonList>(context, data,
            Resource.Layout.ThreeElements,
            (view, item) =>
            {
                TextView Ident = view.FindViewById<TextView>(Resource.Id.first);
                Ident.Text = item.Ident;
                Ident.SetTextColor(Android.Graphics.Color.Black);

                TextView Location = view.FindViewById<TextView>(Resource.Id.second);
                Location.Text = item.Location;
                Location.SetTextColor(Android.Graphics.Color.Black);

                TextView Quantity = view.FindViewById<TextView>(Resource.Id.third);
                Quantity.Text = item.Quantity;
                Quantity.SetTextColor(Android.Graphics.Color.Black);


            });



            return adapter;
        }

    }
}
