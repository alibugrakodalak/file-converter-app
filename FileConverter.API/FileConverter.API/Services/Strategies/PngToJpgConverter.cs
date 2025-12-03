using FileConverter.API.Interfaces;
using SixLabors.ImageSharp; // ImageSharp kütüphanesi

namespace FileConverter.API.Services.Strategies
{
    public class PngToJpgConverter : IConverter
    {
        public string SourceExtension => ".png";
        public string TargetExtension => ".jpg";
        public string TargetMimeType => "image/jpeg"; // Tarayıcı bunun resim olduğunu anlasın

        public async Task<string> ConvertAsync(IFormFile file, string outputFolder)
        {
            var newFileName = Guid.NewGuid().ToString() + TargetExtension;
            var outputPath = Path.Combine(outputFolder, newFileName);

            // 1. Gelen dosyayı ImageSharp ile aç
            using (var image = Image.Load(file.OpenReadStream()))
            {
                // 2. JPG olarak kaydet
                await image.SaveAsJpegAsync(outputPath);
            }

            return outputPath;
        }
    }
}