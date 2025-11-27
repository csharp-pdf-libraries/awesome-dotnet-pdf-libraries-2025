// NuGet: Install-Package IronPdf
using IronPdf;
using IronPdf.Rendering;

class Program
{
    static void Main()
    {
        var renderer = new ChromePdfRenderer();
        renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
        renderer.RenderingOptions.MarginTop = 10;
        renderer.RenderingOptions.MarginBottom = 10;
        renderer.RenderingOptions.MarginLeft = 10;
        renderer.RenderingOptions.MarginRight = 10;
        var htmlContent = "<html><body><h1>Custom Page Size</h1><p>A4 size document with margins.</p></body></html>";
        var pdf = renderer.RenderHtmlAsPdf(htmlContent);
        pdf.SaveAs("custom-size.pdf");
    }
}