namespace PdfLibraryApi.Models;

public class Book
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string FileName { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? Author { get; set; }
    public int PageCount { get; set; }
    public string? ThumbnailKey { get; set; }
    public string PdfKey { get; set; } = string.Empty; // Ruta de la imagen en R2
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    [NotMapped]
    public string? ThumbnailUrl { get; set; }
    [NotMapped]
    public string? DownloadUrl { get; set; }
}
