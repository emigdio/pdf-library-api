using Microsoft.AspNetCore.Mvc;
using PdfLibraryApi.Models;

namespace PdfLibraryApi.Controllers;

[ApiController]
[Route("api/books")]
public class BooksController : ControllerBase
{
    private readonly R2Storage _r2;

    public BooksController(R2Storage r2)
    {
        _r2 = r2;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<Book>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<Book>>> GetBooks(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        if (page < 1)
            page = 1;

        if (pageSize < 1)
            pageSize = 10;

        if (pageSize > 50)
            pageSize = 50;

        var keys = await _r2.ListPdfKeysAsync(ct);

        IEnumerable<Book> booksQuery = keys.Select(k => new Book
        {
            Id = Path.GetFileNameWithoutExtension(k),
            FileName = Path.GetFileName(k)
        });

        if (!string.IsNullOrWhiteSpace(search))
        {
            booksQuery = booksQuery.Where(b =>
                b.FileName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                b.Id.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        var books = booksQuery
            .OrderBy(b => b.FileName)
            .ToList();

        var totalItems = books.Count;
        var totalPages = totalItems == 0
            ? 0
            : (int)Math.Ceiling(totalItems / (double)pageSize);

        var items = books
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var result = new PagedResult<Book>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalPages,
            HasNextPage = page < totalPages,
            HasPreviousPage = page > 1
        };

        return Ok(result);
    }

    [HttpGet("{id}/file")]
    public ActionResult GetBookFile(string id)
    {
        var key = _r2.KeyForId(id);
        var url = _r2.GetPresignedDownloadUrl(key, TimeSpan.FromMinutes(10));

        return Redirect(url);
    }
}