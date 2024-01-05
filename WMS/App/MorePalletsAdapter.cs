using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;

namespace Scanner.App
{

    class MorePalletsAdapter : BaseAdapter
    {
        public List<MorePallets> sList;
        private Context sContext;
        public MorePalletsAdapter(Context context, List<MorePallets> list)
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




        public MorePallets retunObjectAt(int index) 
        {
            return sList[index];
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
                    row = LayoutInflater.From(sContext).Inflate(Resource.Layout.MorePalletsView, null, false);
                }

                TextView SSCC = row.FindViewById<TextView>(Resource.Id.sscc);
                SSCC.Text = sList[position].friendlySSCC;
                SSCC.SetTextColor(Android.Graphics.Color.Black);
                TextView Ident = row.FindViewById<TextView>(Resource.Id.ident);
                //Ident.Text = sList[position].Ident;
                //Ident.SetTextColor(Android.Graphics.Color.Black);
                TextView Name = row.FindViewById<TextView>(Resource.Id.name);
                Name.Text = sList[position].Name.Substring(sList[position].Name.Length - 5);
                Name.SetTextColor(Android.Graphics.Color.Black);
                TextView Quantity = row.FindViewById<TextView>(Resource.Id.quantity);
                Quantity.Text = sList[position].Quantity;
                Quantity.SetTextColor(Android.Graphics.Color.Black);         
                TextView Serial = row.FindViewById<TextView>(Resource.Id.serial);
                //Serial.Text = sList[position].Serial;
                //Serial.SetTextColor(Android.Graphics.Color.Black);
               




            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }


            return row;

        }


      

    }
}