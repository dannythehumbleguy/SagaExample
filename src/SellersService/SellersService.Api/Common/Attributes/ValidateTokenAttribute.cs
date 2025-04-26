using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SellersService.Api.Services;

namespace SellersService.Api.Common.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class ValidateTokenAttribute : Attribute, IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!context.HttpContext.Request.Headers.TryGetValue("Authorization", out var token))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var authService = context.HttpContext.RequestServices.GetService(typeof(IAuthService)) as IAuthService;
        if (authService == null)
        {
            context.Result = new StatusCodeResult(500);
            return;
        }

        var tokenValue = token.ToString();
        if (!await authService.ValidateToken(tokenValue))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        await next();
    }
}