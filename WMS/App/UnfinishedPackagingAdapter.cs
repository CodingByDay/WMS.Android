using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WMS.App
{
    class UnfinishedPackagingAdapter : BaseAdapter
    {

        public List<UnfinishedPackagingList> sList;
        private Context sContext;
        public UnfinishedPackagingAdapter(Context context, List<UnfinishedPackagingList> list)
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
                    row = LayoutInflater.From(sContext).Inflate(Resource.Layout.UnfinishedPackaging, null, false);
                }

                TextView SerialNumber = row.FindViewById<TextView>(Resource.Id.SerialNumber);
                SerialNumber.Text = sList[position].SerialNumber;
                SerialNumber.SetTextColor(Android.Graphics.Color.Black);

                TextView ssccCode = row.FindViewById<TextView>(Resource.Id.ssccCode);
                ssccCode.Text = sList[position].ssccCode;
                ssccCode.SetTextColor(Android.Graphics.Color.Black);

                TextView CreatedBy = row.FindViewById<TextView>(Resource.Id.CreatedBy);
                CreatedBy.Text = sList[position].CreatedBy;
                CreatedBy.SetTextColor(Android.Graphics.Color.Black);

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