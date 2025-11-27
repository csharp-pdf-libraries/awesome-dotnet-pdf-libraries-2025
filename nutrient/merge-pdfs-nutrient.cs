// NuGet: Install-Package PSPDFKit.Dotnet
using PSPDFKit.Pdf;
using System.Threading.Tasks;
using System.Collections.Generic;

class Program
{
    static async Task Main()
    {
        using var processor = await PdfProcessor.CreateAsync();
        
        var document1 = await processor.OpenAsync("document1.pdf");
        var document2 = await processor.OpenAsync("document2.pdf");
        
        var mergedDocument = await processor.MergeAsync(new List<PdfDocument> { document1, document2 });
        await mergedDocument.SaveAsync("merged.pdf");
    }
}