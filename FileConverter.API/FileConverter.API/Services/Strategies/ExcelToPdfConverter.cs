using ClosedXML.Excel;
using FileConverter.API.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace FileConverter.API.Services.Strategies
{
    public class ExcelToPdfConverter : IConverter
    {
        public string TargetMimeType => "application/pdf";
        public string SourceExtension => ".xlsx";
        public string TargetExtension => ".pdf";

        public async Task<string> ConvertAsync(IFormFile file, string outputFolder)
        {
            // 1. Çıktı dosyasının adını hazırla
            var newFileName = Guid.NewGuid().ToString() + ".pdf";
            var outputPath = Path.Combine(outputFolder, newFileName);

            // 2. Excel dosyasını hafızada aç
            using var workbook = new XLWorkbook(file.OpenReadStream());
            var worksheet = workbook.Worksheet(1); // İlk sayfayı al

            // --- DİNAMİK AYARLAR BAŞLANGICI ---

            // Dolu olan alanı tespit et (Boş hücreleri yazdırmamak için)
            var usedRange = worksheet.RangeUsed();

            // Eğer Excel boşsa hata vermesin, boş bir PDF oluştursun diye kontrol
            if (usedRange == null)
            {
                // Basit boş PDF oluşturup dön
                Document.Create(c => c.Page(p => p.Content().Text("Dosya Boş"))).GeneratePdf(outputPath);
                return outputPath;
            }

            var colCount = usedRange.ColumnCount();

            // 1. Sayfa Yönü Kararı: 7 sütundan fazlaysa YAN (Landscape), değilse DİK (Portrait)
            bool isLandscape = colCount > 7;

            // 2. Yazı Boyutu Kararı: 10 sütundan fazlaysa 8 punto, değilse 10 punto
            float fontSize = colCount > 10 ? 8 : 10;
            // ----------------------------------

            // 3. QuestPDF ile Akıllı PDF oluştur
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    // Dinamik sayfa boyutu
                    page.Size(isLandscape ? PageSizes.A4.Landscape() : PageSizes.A4.Portrait());
                    page.Margin(1.5f, Unit.Centimetre); // Kenar boşlukları
                    page.DefaultTextStyle(x => x.FontSize(fontSize)); // Dinamik font

                    // Başlık (Logo niyetine)
                    page.Header()
                        .Text("Dosya Dönüştürücü Raporu")
                        .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                    page.Content().PaddingVertical(1, Unit.Centimetre).Table(table =>
                    {
                        // --- A: Sütunları Otomatik Oluştur ---
                        table.ColumnsDefinition(columns =>
                        {
                            for (int i = 0; i < colCount; i++)
                            {
                                columns.RelativeColumn(); // Sütunları eşit dağıt
                            }
                        });

                        // --- B: Başlıkları (Header) Excel'in 1. Satırından Al ---
                        table.Header(header =>
                        {
                            var firstRow = usedRange.Row(1);
                            foreach (var cell in firstRow.Cells())
                            {
                                header.Cell().Element(HeaderStyle).Text(cell.Value.ToString());
                            }
                        });

                        // --- C: Verileri (Rows) Excel'in Kalanından Al ---
                        // 1. satır başlık olduğu için atlıyoruz (Skip 1)
                        foreach (var row in usedRange.Rows().Skip(1))
                        {
                            foreach (var cell in row.Cells())
                            {
                                table.Cell().Element(CellStyle).Text(cell.Value.ToString());
                            }
                        }
                    });

                    // Alt Bilgi
                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Sayfa ");
                        x.CurrentPageNumber();
                        x.Span(" / ");
                        x.TotalPages();
                    });
                });
            })
            .GeneratePdf(outputPath);

            return outputPath;
        }

        // --- YARDIMCI STİLLER (Kod tekrarını önlemek için) ---

        // Tablo Başlık Stili
        static IContainer HeaderStyle(IContainer container)
        {
            return container
                .Border(1)
                .BorderColor(Colors.Black)
                .Background(Colors.Grey.Lighten3)
                .Padding(5)
                .AlignCenter();
        }

        // Tablo Hücre Stili
        static IContainer CellStyle(IContainer container)
        {
            return container
                .BorderBottom(1)
                .BorderColor(Colors.Grey.Lighten2)
                .Padding(5);
        }
    }
}