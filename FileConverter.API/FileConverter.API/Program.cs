using FileConverter.API.Services;
using FileConverter.API.Services.Strategies;
using FileConverter.API.Interfaces;
using FileConverter.API.Factory;

// 1. Linux Grafik Ayarý (Doðru yerdesin!)
AppContext.SetSwitch("System.Drawing.EnableUnixSupport", true);

var builder = WebApplication.CreateBuilder(args);

// 2. CORS Ayarý
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- SERVÝSLER ---
builder.Services.AddScoped<IConverter, ExcelToPdfConverter>();
builder.Services.AddScoped<IConverter, PngToJpgConverter>();
builder.Services.AddScoped<IConverter, TextToZipConverter>();

// --- KRÝTÝK DÜZELTME BURADA ---
// Eskisi: builder.Services.AddScoped<ConverterFactory>();  <-- YANLIÞ
// Yenisi: Interface ile Class'ý eþleþtiriyoruz:
builder.Services.AddScoped<IConverterFactory, ConverterFactory>();
// ------------------------------

builder.Services.AddHostedService<FileCleanupService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

// 3. CORS Aktif
app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();