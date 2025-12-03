using FileConverter.API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FileConverter.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConverterController : ControllerBase
    {
        private readonly IConverterFactory _converterFactory;

        public ConverterController(IConverterFactory converterFactory)
        {
            _converterFactory = converterFactory;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Dosya yüklenmedi.");

            // --- KRİTİK DÜZELTME: Render için Temp klasörü kontrolü ---
            // Git boş klasörleri yüklemediği için sunucuda bu klasör yok.
            // Bu kod, klasör yoksa oluşturur ve 500 hatasını engeller.
            var tempFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "Temp");

            if (!Directory.Exists(tempFolderPath))
            {
                Directory.CreateDirectory(tempFolderPath);
            }
            // -----------------------------------------------------------

            // 1. Dosyanın uzantısını bul
            var extension = Path.GetExtension(file.FileName);

            // 2. Fabrikaya sor: "Bu uzantıyı işleyecek bir elemanın var mı?"
            var converter = _converterFactory.GetConverter(extension);

            // 3. Eğer fabrika "Yok" derse
            if (converter == null)
            {
                return BadRequest($"Üzgünüz, '{extension}' uzantısı henüz desteklenmiyor.");
            }

            try
            {
                // Bulunan dönüştürücüyü çalıştır (tempFolderPath'i gönderiyoruz)
                var convertedFilePath = await converter.ConvertAsync(file, tempFolderPath);

                var fileBytes = await System.IO.File.ReadAllBytesAsync(convertedFilePath);
                var fileName = Path.GetFileName(convertedFilePath);

                // Converter nesnesinden gelen MimeType'ı kullanarak dosyayı döndür
                return File(fileBytes, converter.TargetMimeType, fileName);
            }
            catch (Exception ex)
            {
                // Hatanın detayını görmek için mesajı dönüyoruz
                return StatusCode(500, $"Sunucu Hatası: {ex.Message}");
            }
        }
    }
}