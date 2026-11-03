using LibraryApi.Common;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]


public abstract class ApiControllerBase : ControllerBase
{
    // Failures map to one status each. A success is 200 with the envelope unless the caller
    // passes its own success result (201 with Location for POST, 204 for DELETE).
    protected IActionResult ToActionResult(ServiceResult result, Func<IActionResult>? onSuccess = null)
    {
        if (result.IsSuccess)
            return onSuccess?.Invoke() ?? Ok(result);

        var statusCode = result.ErrorType switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            _ => throw new InvalidOperationException($"A failed result cannot have error type {result.ErrorType}.")
        };

        return StatusCode(statusCode, result);
    }
}
