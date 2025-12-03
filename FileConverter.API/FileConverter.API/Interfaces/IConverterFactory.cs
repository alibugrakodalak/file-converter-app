namespace FileConverter.API.Interfaces
{
    public interface IConverterFactory
    {
        // Uzantıyı (örn: ".xlsx") verince, uygun dönüştürücüyü döner.
        IConverter GetConverter(string sourceExtension);
    }
}