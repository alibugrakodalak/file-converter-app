using Microsoft.AspNetCore.Http;

namespace FileConverter.API.Interfaces
{
    public interface IConverter
    {
        string SourceExtension { get; }
        string TargetExtension { get; }

        // YENİ: Oluşan dosyanın internet tipi nedir? (Örn: application/pdf)
        string TargetMimeType { get; }

        Task<string> ConvertAsync(IFormFile file, string outputFolder);
    }
}
