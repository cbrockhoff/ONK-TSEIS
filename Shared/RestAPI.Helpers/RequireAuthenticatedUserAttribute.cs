using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RestAPI.Helpers
{
    public class RequireAuthenticatedUserAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if(!context.HttpContext.Request.Headers.ContainsKey(Constants.AuthenticatedUserHeaderKey))
                context.Result = new UnauthorizedResult();
        }
    }
}