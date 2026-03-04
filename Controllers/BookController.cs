using Microsoft.AspNetCore.Mvc;
using PdfLibraryApi.Models;

[ApiController]
[Route("api/books")]
public class BooksController : ControllerBase
{
    private readonly string pdfPath = Path.Combine(Directory.GetCurrentDirectory(), "storage/pdfs");

    [HttpGet]
    public IActionResult GetBooks()
    {
        var files = Directory.GetFiles(pdfPath);

        var books = files.Select(f => new Book
        {
            Id = Path.GetFileNameWithoutExtension(f),
            Title = Path.GetFileNameWithoutExtension(f),
            FileName = Path.GetFileName(f)
        });

        return Ok(books);
    }

    [HttpGet("{id}")]
    public IActionResult GetBook(string id)
    {
        var file = Directory.GetFiles(pdfPath)
            .FirstOrDefault(f => Path.GetFileNameWithoutExtension(f) == id);

        if (file == null)
            return NotFound();

        var stream = System.IO.File.OpenRead(file);
        return File(stream, "application/pdf");
    }
}