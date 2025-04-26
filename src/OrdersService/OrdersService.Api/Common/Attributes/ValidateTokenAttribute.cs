using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using OrdersService.Api.Services;

namespace OrdersService.Api.Common.Attributes;

/// <summary>
/// Атрибут для проверки токена авторизации. Если токен недействителен или отсутствует, возвращается 401 Unauthorized.
/// </summary>
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

        await using var scope = context.HttpContext.RequestServices.CreateAsyncScope();
        var authService = scope.ServiceProvider.GetRequiredService<AuthService>();

        var tokenValue = token.ToString();
        var userId = await authService.ValidateToken(tokenValue);
        if (userId.HasNoValue)
        {
            context.Result = new UnauthorizedResult();
            return;
        }
        
        context.HttpContext.Items["UserId"] = userId.Value;

        await next();
    }
}