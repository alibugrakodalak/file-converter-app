using FileConverter.API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FileConverter.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConverterController : ControllerBase
    {
        // Artık direkt dönüştürücüyü değil, Fabrikayı çağırıyoruz.
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

            // --- BU KODU YAPIŞTIR ---

            // 1. Klasör yolunu belirle (Render'ın anlayacağı yol)
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "Temp");

            // 2. Eğer klasör yoksa OLUŞTUR (Hayat kurtaran satır!)
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            // 3. Dosyayı kaydetme işlemin bundan SONRA gelsin
            // var filePath = Path.Combine(folderPath, file.FileName);
            // ...

            // 1. Dosyanın uzantısını bul (Örn: .xlsx)
            var extension = Path.GetExtension(file.FileName);

            // 2. Fabrikaya sor: "Bu uzantıyı işleyecek bir elemanın var mı?"
            var converter = _converterFactory.GetConverter(extension);

            // 3. Eğer fabrika "Yok" derse (null dönerse)
            if (converter == null)
            {
                return BadRequest($"Üzgünüz, '{extension}' uzantısı henüz desteklenmiyor.");
            }

            // --- Buradan sonrası aynı ---
            var tempFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "Temp");
            if (!Directory.Exists(tempFolderPath)) Directory.CreateDirectory(tempFolderPath);

            try
            {
                // Bulunan dönüştürücüyü çalıştır
                var convertedFilePath = await converter.ConvertAsync(file, tempFolderPath);
                var fileBytes = await System.IO.File.ReadAllBytesAsync(convertedFilePath);
                var fileName = Path.GetFileName(convertedFilePath);

                // Content-Type dinamik olmalı (PDF ise application/pdf, Zip ise application/zip)
                // Şimdilik basitçe 'application/octet-stream' (genel dosya) diyebiliriz 
                // ya da converter içine 'MimeType' özelliği ekleyebiliriz.
                // Şimdilik PDF varsayalım.

                // Converter nesnesinden gelen MimeType'ı kullan
                return File(fileBytes, converter.TargetMimeType, fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Hata: {ex.Message}");
            }
        }
    }
}