using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PdfLibraryApi.Data;
using PdfLibraryApi.Models;
using PDFtoImage;
using UglyToad;

namespace PdfLibraryApi.Controllers;

[ApiController]
[Route("api/books")]
public class BooksController : ControllerBase
{
    private readonly R2Storage _r2;
    private readonly AppDbContext _context;

    public BooksController(R2Storage r2, AppDbContext context)
    {
        _r2 = r2;
        _context = context;
    }

    private byte[] GeneratePdfThumbnail(Stream pdfStream)
    {
        // Reiniciamos la posición del stream por si fue leído antes
        pdfStream.Position = 0;

        // Convertimos la primera página (índice 0) a una imagen PNG
        // No necesitas inicializar librerías complejas, es un método estático
        using var outputStream = new MemoryStream();
        
        // Genera la imagen de la página 0 con una resolución de 96 DPI (ideal para miniaturas)
        Conversion.SavePng(outputStream, pdfStream, page: 0);

        return outputStream.ToArray();
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<Book>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<Book>>> GetBooks(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        // 1. Limpieza de parámetros de paginación
        page = page < 1 ? 1 : page;
        pageSize = Math.Clamp(pageSize, 1, 50);

        // 2. Consulta a la Base de Datos (en lugar de listar R2)
        var query = _context.Books.AsQueryable();

        // 3. Filtro de búsqueda (ahora busca en Título, Autor y Nombre de Archivo)
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(b => 
                (b.Title != null && b.Title.Contains(search)) || 
                (b.Author != null && b.Author.Contains(search)) ||
                b.FileName.Contains(search));
        }

        // 4. Paginación y ejecución de la consulta
        var totalItems = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(b => b.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        // 5. GENERAR URLS FIRMADAS para cada libro
        // Esto permite que el navegador vea la imagen y descargue el PDF
        foreach (var book in items)
        {
            // URL para la miniatura (válida por 30 minutos)
            if (!string.IsNullOrEmpty(book.ThumbnailKey))
            {
                book.ThumbnailUrl = _r2.GetPresignedDownloadUrl(book.ThumbnailKey, TimeSpan.FromMinutes(30));
            }

            // URL para descargar el PDF (válida por 10 minutos)
            book.DownloadUrl = _r2.GetPresignedDownloadUrl(book.PdfKey, TimeSpan.FromMinutes(10));
        }

        var result = new PagedResult<Book>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
            HasNextPage = page * pageSize < totalItems,
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

    [HttpPost("upload")]
    public async Task<IActionResult> UploadBook(IFormFile file)
    {
        if (file == null || file.Length == 0) return BadRequest("No se proporcionó un archivo.");

        using var stream = file.OpenReadStream();
        
        // 1. EXTRAER METADATOS (PdfPig)
        using var document = UglyToad.PdfPig.PdfDocument.Open(stream);
        var info = document.Information;
        var pageCount = document.NumberOfPages;
        var title = info.Title ?? Path.GetFileNameWithoutExtension(file.FileName);

        // 2. GENERAR MINIATURA (DocLib)
        stream.Position = 0; // Reiniciar el puntero del stream
        byte[] thumbnailBytes = GeneratePdfThumbnail(stream);

        // 3. SUBIR PDF A R2
        var pdfKey = $"books/{Guid.NewGuid()}_{file.FileName}";
        stream.Position = 0;
        await _r2.UploadStreamAsync(pdfKey, stream, file.ContentType);

        // 4. SUBIR MINIATURA A R2
        var thumbKey = $"thumbnails/{Path.GetFileNameWithoutExtension(pdfKey)}.png";
        using var thumbStream = new MemoryStream(thumbnailBytes);
        await _r2.UploadStreamAsync(thumbKey, thumbStream, "image/png");

        var newBook = new Book {
            Id = Guid.NewGuid().ToString(),
            FileName = file.FileName,
            Title = title,         // Extraído del PDF
            Author = info.Author,       // Extraído del PDF
            PageCount = document.NumberOfPages,     // Extraído del PDF
            ThumbnailKey = thumbKey,
            PdfKey = pdfKey
        };

        // GUARDAR EN SQLITE
        _context.Books.Add(newBook);
        await _context.SaveChangesAsync();

        return Ok(newBook);
    }

    [HttpPost("sync-from-r2")]
    public async Task<IActionResult> SyncBooksFromR2(CancellationToken ct)
    {
        // 1. Obtener todas las llaves (keys) de los archivos actuales en R2
        var allKeys = await _r2.ListPdfKeysAsync(ct); 
        var existingKeys = new HashSet<string>(await _context.Books.Select(b => b.PdfKey).ToListAsync(ct));
        int processedCount = 0;

        foreach (var key in allKeys)
        {
            // Si ya existe en la DB por su Key, lo saltamos (consulta ultra rápida en memoria)
            if (existingKeys.Contains(key)) continue;

            try 
            {
                // 2. Descargar el archivo a memoria temporalmente
                using var pdfStream = await _r2.DownloadFileAsync(key, ct);
                if (pdfStream == null) continue;

                // 3. Extraer Metadatos (PdfPig)
                using var document = UglyToad.PdfPig.PdfDocument.Open(pdfStream);
                var info = document.Information;
                
                // 4. Generar Miniatura (DocLib)
                pdfStream.Position = 0;
                byte[] thumbBytes = GeneratePdfThumbnail(pdfStream); // El método que definimos antes
                
                // 5. Subir la miniatura generada a R2
                var thumbKey = $"thumbnails/{Path.GetFileNameWithoutExtension(key)}.png";
                using var thumbStream = new MemoryStream(thumbBytes);
                await _r2.UploadStreamAsync(thumbKey, thumbStream, "image/png");

                // 6. Registrar en la Base de Datos SQLite
                var book = new Book
                {
                    Id = Guid.NewGuid().ToString(),
                    FileName = Path.GetFileName(key),
                    Title = info.Title ?? Path.GetFileNameWithoutExtension(key),
                    Author = info.Author,
                    PageCount = document.NumberOfPages,
                    PdfKey = key,
                    ThumbnailKey = thumbKey
                };

                _context.Books.Add(book);
                processedCount++;
            }
            catch (Exception ex)
            {
                // Log de error por si un PDF está corrupto o protegido
                Console.WriteLine($"Error procesando {key}: {ex.Message}");
            }
        }

        await _context.SaveChangesAsync(ct);
        return Ok(new { Message = $"Sincronización completada. Se añadieron {processedCount} libros." });
    }

}