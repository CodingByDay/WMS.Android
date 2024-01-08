using Android.Graphics;
using Square.Picasso;
using static _Microsoft.Android.Resource.Designer.Resource;

public class DrawableTarget : Java.Lang.Object, ITarget
{
    private readonly ImageView _imageView;

    public DrawableTarget(ImageView imageView)
    {
        _imageView = imageView;
    }

    public void OnBitmapFailed(Drawable errorDrawable)
    { }

    public void OnBitmapFailed(Java.Lang.Exception errorDrawable, Android.Graphics.Drawables.Drawable p1)
    {
        throw new NotImplementedException();
    }

    public void OnBitmapLoaded(Bitmap bitmap, Picasso.LoadedFrom from)
    {
        _imageView.SetImageBitmap(bitmap);
    }

    public void OnPrepareLoad(Drawable placeHolderDrawable)
    { }

    public void OnPrepareLoad(Android.Graphics.Drawables.Drawable placeHolderDrawable)
    {
        throw new NotImplementedException();
    }
}