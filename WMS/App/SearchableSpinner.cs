using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Android.App;
using System.Collections.Generic;

namespace WMS
{
    public class SearchableSpinner : LinearLayout
    {
        private EditText editText;
        private ImageView icon;
        private List<string> items;
        private Context context;
        private string selectedItem;

        public SearchableSpinner(Context context) : base(context)
        {
            this.context = context;
            Initialize(context);
        }

        public SearchableSpinner(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            this.context = context;
            Initialize(context);
        }

        public SearchableSpinner(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
            this.context = context;
            Initialize(context);
        }

        protected SearchableSpinner(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
            this.context = Context;
            Initialize(context);
        }

        private void Initialize(Context context)
        {
            Orientation = Orientation.Horizontal;

            // Inflate your custom layout
            Inflate(context, Resource.Layout.SearchableSpinnerRepresentation, this);

            editText = FindViewById<EditText>(Resource.Id.editText);
            icon = FindViewById<ImageView>(Resource.Id.icon);

            icon.Click += (s, e) =>
            {
                ShowPopupDialog();
            };

        }

        private void ShowPopupDialog()
        {
            var builder = new AlertDialog.Builder(context);
            var view = LayoutInflater.From(context).Inflate(Resource.Layout.SearchableSpinner, null);

            var searchViewInDialog = view.FindViewById<SearchView>(Resource.Id.searchView);
            var listViewInDialog = view.FindViewById<ListView>(Resource.Id.listView);

            var popupAdapter = new ArrayAdapter<string>(context, Android.Resource.Layout.SimpleListItem1, items);
            listViewInDialog.Adapter = popupAdapter;

            searchViewInDialog.QueryTextChange += (s, e) =>
            {
                // Filter list based on search query
                var filteredItems = string.IsNullOrEmpty(e.NewText)
                    ? items
                    : items.FindAll(item => item.ToLower().Contains(e.NewText.ToLower()));
                popupAdapter.Clear();
                popupAdapter.AddAll(filteredItems);
                popupAdapter.NotifyDataSetChanged();
            };

            listViewInDialog.ItemClick += (s, e) =>
            {
                // Handle item selection
                selectedItem = popupAdapter.GetItem(e.Position);
                editText.Text = selectedItem; // Update EditText with the selected item
            };

            builder.SetView(view);
            builder.SetPositiveButton("OK", (s, e) =>
            {
                // Handle OK button if necessary
            });

            builder.SetNegativeButton("Cancel", (s, e) => { });

            builder.Create().Show();
        }

        public void SetItems(List<string> items)
        {
            this.items = items;
        }
    }
}
