using Microsoft.AspNetCore.Mvc.Filters;
using System.Diagnostics;

namespace BestStoreMVC.NewFolder
{
    public class CustomeFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.ModelState.IsValid)
            {
                Trace.WriteLine("Success login");
            }
            else
            {
                Trace.WriteLine("Success not login");
            }
        }
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            base.OnActionExecuted(context);
        }
    }
}
