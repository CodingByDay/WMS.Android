using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.Lang;
using Exception = Java.Lang.Exception;

namespace Scanner.App
{
   public class adapter : BaseAdapter
    {
        public List<Trail> sList;
        private Context sContext;


        private Trail selected;

        public adapter(Context context, List<Trail> list)
        {
            sList = list;
            sContext = context;
        }

        public Trail returnSelected()
        {
            return selected;
        } 

        public void setSelected(Trail selected)
        {
            this.selected = selected;
        }


        public void setSelected(int position)
        {
            selected = sList[position];
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
               
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }

            
            return row;

        }
        

        public void Filter(List<Trail> data, bool byIdent, string val, bool restart)
        {
            if(restart)
            {
                sList = data;
            }     
            if (byIdent)
            {
                string searchFilter = val;
                if(val.StartsWith("P"))
                {
                    searchFilter = val.Substring(1);
                }
                sList = data.Where(data => data.Ident.Contains(searchFilter)).ToList(); 
            } else
            {
                sList = data.Where(data => data.Location.Contains(val)).ToList();
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

        public void NotifyDataSetChanged()
        {
            base.NotifyDataSetChanged();

        }
    }

}