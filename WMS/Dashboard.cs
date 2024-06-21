using WMS.ExceptionStore;

namespace WMS
{
    [Activity(Label = "Dashboard")]
    public class Dashboard : CustomBaseActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                SetContentView(Resource.Layout.TakeOverEnteredPositionsView);
            }
            catch (Exception ex)
            {
                GlobalExceptions.ReportGlobalException(ex);
            }
        }
    }
}