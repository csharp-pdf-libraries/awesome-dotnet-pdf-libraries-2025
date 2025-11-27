// NuGet: Install-Package IronPdf
using IronPdf;
using IronPdf.Rendering;

class Program
{
    static void Main()
    {
        var renderer = new ChromePdfRenderer();
        renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
        renderer.RenderingOptions.MarginTop = 40;
        renderer.RenderingOptions.MarginBottom = 40;
        
        string html = "<html><body><h1>Custom PDF</h1><p>With custom margins and settings.</p></body></html>";
        var pdf = renderer.RenderHtmlAsPdf(html);
        pdf.SaveAs("custom.pdf");
    }
}