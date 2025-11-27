// NuGet: Install-Package QuestPDF
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

class Program
{
    static void Main()
    {
        QuestPDF.Settings.License = LicenseType.Community;
        
        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.Content().Column(column =>
                {
                    column.Item().Text("INVOICE").FontSize(24).Bold();
                    column.Item().Text("Invoice #: 12345").FontSize(12);
                    column.Item().PaddingTop(20);
                    column.Item().Text("Customer: John Doe");
                    column.Item().Text("Total: $100.00").Bold();
                });
            });
        }).GeneratePdf("invoice.pdf");
    }
}