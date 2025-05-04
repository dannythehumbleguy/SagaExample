using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace SellersService.Api.Common;

[ApiController]
[Route("api/[controller]")]
public class AbstractController : ControllerBase
{
    protected async Task<Results<Ok<T>, BadRequest<Error>>> Wrap<T>(Task<Result<T, Error>> task)
    {
        var result = await task;
        if (result.IsFailure)
            return TypedResults.BadRequest(result.Error);
        
        return TypedResults.Ok(result.Value);
    }

    protected Guid UserId => GetUserId();

    private Guid GetUserId()
    {
        if (!HttpContext.Items.TryGetValue("UserId", out var userId))
            throw new UnauthorizedAccessException("User ID not found in context");
        
        return (Guid)userId;
    }
}