// NuGet: Install-Package PSPDFKit.Dotnet
using PSPDFKit.Pdf;
using PSPDFKit.Pdf.Annotation;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        using var processor = await PdfProcessor.CreateAsync();
        var document = await processor.OpenAsync("document.pdf");
        
        for (int i = 0; i < document.PageCount; i++)
        {
            var watermark = new TextAnnotation("CONFIDENTIAL")
            {
                Opacity = 0.5,
                FontSize = 48
            };
            await document.AddAnnotationAsync(i, watermark);
        }
        
        await document.SaveAsync("watermarked.pdf");
    }
}