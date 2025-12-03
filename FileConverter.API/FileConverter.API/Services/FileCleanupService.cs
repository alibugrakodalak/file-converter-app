namespace FileConverter.API.Services
{
    // BackgroundService, uygulama başladığında çalışır ve durana kadar devam eder.
    public class FileCleanupService : BackgroundService
    {
        private readonly ILogger<FileCleanupService> _logger;
        private readonly string _tempPath;

        // Temizlik ne sıklıkla yapılsın? (Örn: 1 saatte bir)
        private readonly TimeSpan _cleanupInterval = TimeSpan.FromHours(1);

        // Dosyalar ne kadar süre saklansın? (Örn: 1 saatten eski olanlar silinsin)
        private readonly TimeSpan _fileRetentionPeriod = TimeSpan.FromHours(1);

        public FileCleanupService(ILogger<FileCleanupService> logger)
        {
            _logger = logger;
            _tempPath = Path.Combine(Directory.GetCurrentDirectory(), "Temp");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("🧹 Temizlik servisi başlatıldı.");

            // Uygulama durdurulana kadar döngü devam etsin
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (Directory.Exists(_tempPath))
                    {
                        var files = Directory.GetFiles(_tempPath);
                        foreach (var file in files)
                        {
                            var fileInfo = new FileInfo(file);

                            // Eğer dosya oluşturulma zamanı + 1 saat < Şu an ise (Yani süresi dolmuşsa)
                            if (fileInfo.CreationTime < DateTime.Now - _fileRetentionPeriod)
                            {
                                fileInfo.Delete();
                                _logger.LogInformation($"Silindi: {fileInfo.Name}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Temizlik sırasında hata: {ex.Message}");
                }

                // Bir sonraki temizlik zamanına kadar uyu
                await Task.Delay(_cleanupInterval, stoppingToken);
            }
        }
    }
}