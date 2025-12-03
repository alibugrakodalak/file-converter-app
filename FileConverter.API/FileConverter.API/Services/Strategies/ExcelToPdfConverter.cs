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

        // Bu sınıfın kimliği: .xlsx alır, .pdf verir.
        public string SourceExtension => ".xlsx";
        public string TargetExtension => ".pdf";

        public async Task<string> ConvertAsync(IFormFile file, string outputFolder)
        {
            // 1. Çıktı dosyasının adını ve yolunu hazırla
            var newFileName = Guid.NewGuid().ToString() + ".pdf";
            var outputPath = Path.Combine(outputFolder, newFileName);

            // 2. Excel dosyasını hafızada aç (Diske kaydetmeden okuyoruz)
            using var workbook = new XLWorkbook(file.OpenReadStream());
            var worksheet = workbook.Worksheet(1); // İlk sayfayı al

            // 3. QuestPDF ile PDF oluştur
            // Not: Gerçek projelerde bu çizim kodları ayrı bir servise alınabilir.
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Text("Dosya Dönüştürücü Raporu").SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                    page.Content().PaddingVertical(1, Unit.Centimetre).Table(table =>
                    {
                        // Sütunları tanımla (Basitlik için 3 sütun varsayalım veya dinamik yapılabilir)
                        // Burada örnek olarak ilk 3 sütunu alıyoruz.
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                        // Başlıkları (Header) yazdır
                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("Sütun 1");
                            header.Cell().Element(CellStyle).Text("Sütun 2");
                            header.Cell().Element(CellStyle).Text("Sütun 3");

                            static IContainer CellStyle(IContainer container)
                            {
                                return container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                            }
                        });

                        // Verileri (Rows) yazdır
                        // İlk satır başlık olduğu için 2. satırdan başlıyoruz
                        var rows = worksheet.RowsUsed().Skip(1);

                        foreach (var row in rows)
                        {
                            // Excel'deki hücreleri PDF tablosuna ekle
                            table.Cell().Element(CellStyle).Text(row.Cell(1).GetValue<string>());
                            table.Cell().Element(CellStyle).Text(row.Cell(2).GetValue<string>());
                            table.Cell().Element(CellStyle).Text(row.Cell(3).GetValue<string>());
                        }

                        static IContainer CellStyle(IContainer container)
                        {
                            return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
                        }
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Sayfa ");
                        x.CurrentPageNumber();
                    });
                });
            })
            .GeneratePdf(outputPath); // PDF'i diske kaydet

            // Dönüştürülen dosyanın tam yolunu dön
            return outputPath;
        }
    }
}