using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Shuryan.API.Filters
{
    public class DevelopmentOnlyAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var env = context.HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>();
            
            // Allow access only if it's Development or Testing environment
            if (!env.IsDevelopment() && !env.IsEnvironment("Testing"))
            {
                context.Result = new NotFoundResult();
            }
        }
    }
}
