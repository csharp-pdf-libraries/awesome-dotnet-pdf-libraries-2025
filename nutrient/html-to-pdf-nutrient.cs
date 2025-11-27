// NuGet: Install-Package PSPDFKit.Dotnet
using PSPDFKit.Pdf;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        var htmlContent = "<html><body><h1>Hello World</h1></body></html>";
        
        using var processor = await PdfProcessor.CreateAsync();
        var document = await processor.GeneratePdfFromHtmlStringAsync(htmlContent);
        await document.SaveAsync("output.pdf");
    }
}