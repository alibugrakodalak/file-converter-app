using FileConverter.API.Interfaces;
using System.IO.Compression; // Zip işlemleri için

namespace FileConverter.API.Services.Strategies
{
    public class TextToZipConverter : IConverter
    {
        public string SourceExtension => ".txt";
        public string TargetExtension => ".zip";
        // Tarayıcının bunun bir zip dosyası olduğunu anlaması için standart MIME type
        public string TargetMimeType => "application/zip";

        public async Task<string> ConvertAsync(IFormFile file, string outputFolder)
        {
            // 1. Çıktı yolunu hazırla
            var newFileName = Guid.NewGuid().ToString() + TargetExtension;
            var zipPath = Path.Combine(outputFolder, newFileName);

            // 2. FileStream kullanarak Zip dosyasını oluştur (Daha güvenli yöntem)
            using (var fileStream = new FileStream(zipPath, FileMode.Create))
            {
                using (var archive = new ZipArchive(fileStream, ZipArchiveMode.Create))
                {
                    // 3. Zip'in içine dosya ekle (Dosya ismini temizle)
                    // Path.GetFileName kullanarak C:/User/Desktop... gibi yolları temizliyoruz.
                    var entryName = Path.GetFileName(file.FileName);
                    var entry = archive.CreateEntry(entryName);

                    // 4. İçeriği kopyala
                    using (var entryStream = entry.Open())
                    using (var uploadStream = file.OpenReadStream())
                    {
                        await uploadStream.CopyToAsync(entryStream);
                    }
                }
            }

            return zipPath;
        }
    }
}