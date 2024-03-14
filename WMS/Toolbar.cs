using Android.App;
using Android.Graphics;
using Android.Widget;
using Square.Picasso;

public class CustomToolbar
{
    private readonly Activity _activity;
    public readonly AndroidX.AppCompat.Widget.Toolbar _toolbar;
    private readonly int _navIconImageViewId;

    public CustomToolbar(Activity activity, AndroidX.AppCompat.Widget.Toolbar toolbar, int navIconImageViewId)
    {
        _activity = activity;
        _toolbar = toolbar;
        _navIconImageViewId = navIconImageViewId;
    }

    public void SetNavigationIcon(string imageUrl, ImageView image = null)
    {
        ImageView navIconImageView = _toolbar.FindViewById<ImageView>(_navIconImageViewId);
        ImageView cachedImage = image;
        // Set the Bitmap to the ImageView
        if (cachedImage != null)
        {
            navIconImageView = cachedImage;
        }
        navIconImageView.Visibility = Android.Views.ViewStates.Visible;


    }
}
