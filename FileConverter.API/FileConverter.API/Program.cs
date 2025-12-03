using FileConverter.API.Services;
using FileConverter.API.Services.Strategies;
using FileConverter.API.Interfaces;
using FileConverter.API.Factory;

var builder = WebApplication.CreateBuilder(args);

// 1. CORS Politikasýný Tanýmla (En üste yakýn olsun)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()   // Kim gelirse gelsin (Vercel, Localhost vs.)
               .AllowAnyMethod()   // GET, POST, PUT...
               .AllowAnyHeader();  // Tüm baþlýklara izin ver
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Servislerin (Strategy Pattern & Background Service)
builder.Services.AddScoped<IConverter, ExcelToPdfConverter>();
builder.Services.AddScoped<IConverter, PngToJpgConverter>();
builder.Services.AddScoped<IConverter, TextToZipConverter>();
builder.Services.AddScoped<ConverterFactory>();
builder.Services.AddHostedService<FileCleanupService>();

var app = builder.Build();

// Swagger (Production'da da çalýþsýn diye if kontrolünü kaldýrdýk veya içine aldýk)
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

// 2. CORS'u Aktif Et (Tam olarak burada olmalý!)
// UseRouting'den SONRA, UseAuthorization'dan ÖNCE
app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();