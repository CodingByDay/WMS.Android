using Android.Content;
using Android.Graphics;
using Android.Views;

namespace WMS.App
{
    public class LanguageAdapter : BaseAdapter<LanguageItem>
    {
        private List<LanguageItem> mLanguageItems;
        private Context mContext;

        public LanguageAdapter(Context context, List<LanguageItem> languageItems)
        {
            mContext = context;
            mLanguageItems = languageItems;
        }

        public override LanguageItem this[int position] => mLanguageItems[position];

        public override int Count => mLanguageItems.Count;

        public override long GetItemId(int position) => position;

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            View view = convertView;
            if (view == null)
            {
                view = LayoutInflater.From(mContext).Inflate(Resource.Layout.language_item, null);
            }

            ImageView languageIcon = view.FindViewById<ImageView>(Resource.Id.languageIcon);
            TextView languageName = view.FindViewById<TextView>(Resource.Id.languageName);

            LanguageItem item = mLanguageItems[position];
            languageName.Text = item.Name;
            languageIcon.SetImageBitmap(item.Icon);

            return view;
        }
    }

    public class LanguageItem
    {
        public string Name { get; set; }
        public Bitmap Icon { get; set; }
    }
}
