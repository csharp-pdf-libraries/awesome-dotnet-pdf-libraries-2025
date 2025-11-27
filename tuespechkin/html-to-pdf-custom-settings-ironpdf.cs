// NuGet: Install-Package IronPdf
using IronPdf;
using IronPdf.Engines.Chrome;
using System;

class Program
{
    static void Main()
    {
        var renderer = new ChromePdfRenderer();
        
        renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape;
        renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
        renderer.RenderingOptions.MarginTop = 10;
        renderer.RenderingOptions.MarginBottom = 10;
        
        string html = "<html><body><h1>Custom PDF</h1></body></html>";
        var pdf = renderer.RenderHtmlAsPdf(html);
        
        pdf.SaveAs("custom.pdf");
    }
}