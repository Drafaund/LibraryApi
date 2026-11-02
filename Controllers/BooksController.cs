using LibraryApi.DTOs.Books;
using LibraryApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers;

[Authorize]
public class BooksController : ApiControllerBase
{
    private readonly IBookService _service;

    public BooksController(IBookService service)
    {
        _service = service;
    }

    // Only a missing "title" lists everything; "?title=" with a blank value goes to the search and gets a 400.
    [HttpGet]
    public async Task<IActionResult> ListAsync([FromQuery] string? title, CancellationToken cancellationToken)
    {
        var result = title is null
            ? await _service.ListAsync(cancellationToken)
            : await _service.SearchByTitleAsync(title, cancellationToken);

        return ToActionResult(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return ToActionResult(await _service.GetByIdAsync(id, cancellationToken));
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync(CreateBookRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(request, cancellationToken);

        return ToActionResult(result, () => CreatedAtAction(nameof(GetByIdAsync), new { id = result.Data!.Id }, result));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateAsync(int id, UpdateBookRequest request, CancellationToken cancellationToken)
    {
        return ToActionResult(await _service.UpdateAsync(id, request, cancellationToken));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        return ToActionResult(await _service.DeleteAsync(id, cancellationToken), NoContent);
    }
}
