using LibraryApi.DTOs.Auth;
using LibraryApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers;

[AllowAnonymous]
public class AuthController : ApiControllerBase
{
    private readonly IAuthService _service;

    public AuthController(IAuthService service)
    {
        _service = service;
    }

    // 201 without Location: there is no endpoint to fetch a user by id.
    [HttpPost("register")]
    public async Task<IActionResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.RegisterAsync(request, cancellationToken);

        return ToActionResult(result, () => StatusCode(StatusCodes.Status201Created, result));
    }

    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        return ToActionResult(await _service.LoginAsync(request, cancellationToken));
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshAsync(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        return ToActionResult(await _service.RefreshAsync(request, cancellationToken));
    }

    // 200 with the envelope instead of 204, so every body keeps the same shape.
    [HttpPost("logout")]
    public async Task<IActionResult> LogoutAsync(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        return ToActionResult(await _service.LogoutAsync(request, cancellationToken));
    }
}
