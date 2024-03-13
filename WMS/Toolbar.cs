using Android.App;
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

    public void SetNavigationIcon(string imageUrl)
    {   try { 
        ImageView navIconImageView = _toolbar.FindViewById<ImageView>(_navIconImageViewId);
        
        // Load and set the image with Picasso
        Picasso.Get()
            .Load(imageUrl)
            .Into(navIconImageView);

        // Make the ImageView visible
        navIconImageView.Visibility = Android.Views.ViewStates.Visible;
       } catch
        {
            return;
        }
    }
}
