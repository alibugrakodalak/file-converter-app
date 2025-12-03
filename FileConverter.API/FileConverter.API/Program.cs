using System.Numerics;
using FileConverter.API.Interfaces;
using FileConverter.API.Services;
using QuestPDF.Infrastructure; 

// Bu satýrý builder oluþturulmadan hemen önce ekle:
QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

// 1.Servisleri Ekleme Alaný
builder.Services.AddControllers();

// Tüm Dönüþtürücü Stratejilerini Kaydet
// --- Stratejiler ---
builder.Services.AddScoped<IConverter, FileConverter.API.Services.Strategies.ExcelToPdfConverter>();
builder.Services.AddScoped<IConverter, FileConverter.API.Services.Strategies.PngToJpgConverter>(); // Yeni
builder.Services.AddScoped<IConverter, FileConverter.API.Services.Strategies.TextToZipConverter>(); // Yeni

// Arka plan temizlik servisini baþlat
builder.Services.AddHostedService<FileCleanupService>();

// --- Fabrika ---
builder.Services.AddScoped<IConverterFactory, FileConverter.API.Factory.ConverterFactory>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS Ayarý (Vue uygulamasýna izin)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVueApp",
        policy =>
        {
            policy.WithOrigins("http://localhost:5173") // Vue'nun varsayýlan portu
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

var app = builder.Build();

// 2.Middleware(Ara Yazýlým) Alaný
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// CORS'u aktif et (Authorization'dan ÖNCE olmalý)
app.UseCors("AllowVueApp");

app.UseAuthorization();

app.MapControllers();

app.Run();