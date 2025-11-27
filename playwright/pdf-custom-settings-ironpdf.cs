// NuGet: Install-Package IronPdf
using IronPdf;
using IronPdf.Rendering;

class Program
{
    static void Main(string[] args)
    {
        var renderer = new ChromePdfRenderer();
        
        renderer.RenderingOptions.MarginTop = 10;
        renderer.RenderingOptions.MarginBottom = 10;
        renderer.RenderingOptions.MarginLeft = 10;
        renderer.RenderingOptions.MarginRight = 10;
        renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
        renderer.RenderingOptions.TextHeader.CenterText = "Header";
        renderer.RenderingOptions.TextFooter.CenterText = "Page {page}";
        
        string html = "<h1>Custom PDF</h1><p>With margins and headers.</p>";
        var pdf = renderer.RenderHtmlAsPdf(html);
        pdf.SaveAs("custom.pdf");
    }
}