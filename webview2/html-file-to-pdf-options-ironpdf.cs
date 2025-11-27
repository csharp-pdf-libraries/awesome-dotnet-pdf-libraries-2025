// NuGet: Install-Package IronPdf
using IronPdf;
using IronPdf.Rendering;

class Program
{
    static void Main()
    {
        var renderer = new ChromePdfRenderer();
        renderer.RenderingOptions.PaperSize = PdfPaperSize.Letter;
        renderer.RenderingOptions.MarginTop = 40;
        renderer.RenderingOptions.MarginBottom = 40;
        renderer.RenderingOptions.MarginLeft = 40;
        renderer.RenderingOptions.MarginRight = 40;
        renderer.RenderingOptions.PrintHtmlBackgrounds = true;
        
        var pdf = renderer.RenderHtmlFileAsPdf("document.html");
        pdf.SaveAs("output.pdf");
    }
}