using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/books")]
public class BooksController : ControllerBase
{
    private readonly R2Storage _r2;

    public BooksController(R2Storage r2) => _r2 = r2;

    // GET /api/books
    [HttpGet]
    public async Task<IActionResult> GetBooks(CancellationToken ct)
    {
        var keys = await _r2.ListPdfKeysAsync(ct);

        // Convierte "books/mi-libro.pdf" -> "mi-libro"
        var books = keys.Select(k => new
        {
            id = Path.GetFileNameWithoutExtension(k),
            fileName = Path.GetFileName(k),
            key = k
        });

        return Ok(books);
    }

    // GET /api/books/{id}/file
    [HttpGet("{id}/file")]
    public IActionResult GetBookFile(string id)
    {
        var key = _r2.KeyForId(id);

        var url = _r2.GetPresignedDownloadUrl(key, TimeSpan.FromMinutes(10));
        return Redirect(url); // 302 hacia R2
    }
}