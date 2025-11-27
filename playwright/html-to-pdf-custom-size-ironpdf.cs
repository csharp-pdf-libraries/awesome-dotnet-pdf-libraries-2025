// NuGet: Install-Package IronPdf
using IronPdf;
using IronPdf.Rendering;

class Program
{
    static void Main()
    {
        var renderer = new ChromePdfRenderer();
        renderer.RenderingOptions.PaperSize = PdfPaperSize.Letter;
        renderer.RenderingOptions.MarginTop = 25;
        renderer.RenderingOptions.MarginBottom = 25;
        renderer.RenderingOptions.MarginLeft = 12;
        renderer.RenderingOptions.MarginRight = 12;
        var pdf = renderer.RenderHtmlAsPdf("<h1>Custom PDF</h1><p>Letter size with margins</p>");
        pdf.SaveAs("custom.pdf");
    }
}