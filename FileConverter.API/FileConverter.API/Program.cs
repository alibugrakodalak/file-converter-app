using FileConverter.API.Services;
using FileConverter.API.Services.Strategies;
using FileConverter.API.Interfaces;
using FileConverter.API.Factory;
using System.Text; // <-- 1. BU KÜTÜPHANEYÝ EKLEDÝK

// 2. Linux Grafik Ýzni (Zaten vardý, dursun)
AppContext.SetSwitch("System.Drawing.EnableUnixSupport", true);

// 3. ÝÞTE BU EKSÝKTÝ: Excel için Karakter Seti Ýzni (Linux için ÞART!)
System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

var builder = WebApplication.CreateBuilder(args);

// CORS Ayarý
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

// Servisler
builder.Services.AddScoped<IConverter, ExcelToPdfConverter>();
builder.Services.AddScoped<IConverter, PngToJpgConverter>();
builder.Services.AddScoped<IConverter, TextToZipConverter>();

// Factory Düzeltmesi (Bunu yapmýþtýk, koruyoruz)
builder.Services.AddScoped<IConverterFactory, ConverterFactory>();

builder.Services.AddHostedService<FileCleanupService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();