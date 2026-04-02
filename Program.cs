using Microsoft.EntityFrameworkCore;
using PdfLibraryApi.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<R2Storage>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=library.db"));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.MapGet("/", () => Results.Redirect("/swagger"));

app.Run();
