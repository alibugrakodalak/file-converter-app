using FileConverter.API.Interfaces;

namespace FileConverter.API.Factory
{
    public class ConverterFactory : IConverterFactory
    {
        // Fabrika, sistemdeki tüm dönüştürücüleri bu listede tutar.
        private readonly IEnumerable<IConverter> _converters;

        // Dependency Injection sayesinde .NET bize tüm IConverter'ları buraya otomatik doldurur.
        public ConverterFactory(IEnumerable<IConverter> converters)
        {
            _converters = converters;
        }

        public IConverter GetConverter(string sourceExtension)
        {
            // Listeyi gez, "SourceExtension" özelliği aranan uzantıya eşit olan ilk dönüştürücüyü bul.
            // Örn: ".xlsx" gelen dosyayı ".xlsx" işleyen converter ile eşleştir.
            // Büyük/küçük harf duyarlılığını kaldırmak için ToLower() kullanıyoruz.
            return _converters.FirstOrDefault(c =>
                c.SourceExtension.ToLower() == sourceExtension.ToLower());
        }
    }
}