namespace WMS
{
    [Activity(Label = "Dashboard")]
    public class Dashboard : CustomBaseActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.TakeOverEnteredPositionsView);

        }
    }
}