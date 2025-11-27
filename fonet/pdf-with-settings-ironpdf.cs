// NuGet: Install-Package IronPdf
using IronPdf;
using IronPdf.Engines.Chrome;

class Program
{
    static void Main()
    {
        var renderer = new ChromePdfRenderer();
        renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
        renderer.RenderingOptions.MarginTop = 20;
        renderer.RenderingOptions.MarginBottom = 20;
        renderer.RenderingOptions.MarginLeft = 25;
        renderer.RenderingOptions.MarginRight = 25;
        
        string html = "<h1 style='font-size:14pt'>Custom PDF</h1>";
        var pdf = renderer.RenderHtmlAsPdf(html);
        pdf.SaveAs("custom.pdf");
    }
}