using LibraryApi.Common;
using LibraryApi.DTOs.Authors;
using LibraryApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers;

[Authorize]
public class AuthorsController : ApiControllerBase
{
    private readonly IAuthorService _service;

    public AuthorsController(IAuthorService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> ListAsync(CancellationToken cancellationToken)
    {
        return ToActionResult(await _service.ListAsync(cancellationToken));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return ToActionResult(await _service.GetByIdAsync(id, cancellationToken));
    }

    [HttpPost]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> CreateAsync(CreateAuthorRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(request, cancellationToken);

        return ToActionResult(result, () => CreatedAtAction(nameof(GetByIdAsync), new { id = result.Data!.Id }, result));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> UpdateAsync(int id, UpdateAuthorRequest request, CancellationToken cancellationToken)
    {
        return ToActionResult(await _service.UpdateAsync(id, request, cancellationToken));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        return ToActionResult(await _service.DeleteAsync(id, cancellationToken), NoContent);
    }
}
