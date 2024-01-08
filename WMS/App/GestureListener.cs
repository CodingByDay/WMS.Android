// Define a custom GestureDetector.SimpleOnGestureListenerusing Android.App;
using Android.Views;

internal class GestureListener : GestureDetector.SimpleOnGestureListener, View.IOnTouchListener
{
    private static readonly int SWIPE_THRESHOLD = 5;  // Adjust this value for sensitivity
    private static readonly int SWIPE_VELOCITY_THRESHOLD = 5;  // Adjust this value for sensitivity
    private ISwipeListener swipeListener;

    public GestureListener(ISwipeListener listener)
    {
        swipeListener = listener;
    }

    public bool OnTouch(View v, MotionEvent e)
    {
        float x = e.GetX();

        switch (e.Action)
        {
            case MotionEventActions.Down:
                // Save the initial touch position if needed
                break;

            case MotionEventActions.Up:
                // Determine if the touch was on the left or right side
                float viewWidth = v.Width;

                if (x < viewWidth / 2)
                {
                    swipeListener.OnSwipeLeft();
                }
                else
                {
                    swipeListener.OnSwipeRight();
                }
                break;

                // Handle other touch events as needed
        }

        return true;
    }
}