using Microsoft.EntityFrameworkCore;
using PdfLibraryApi.Models;

namespace PdfLibraryApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Esta es tu tabla de libros
    public DbSet<Book> Books { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configuraciones extra (opcional)
        modelBuilder.Entity<Book>().HasKey(b => b.Id);
    }
}
