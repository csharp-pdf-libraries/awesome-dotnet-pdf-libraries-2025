// NuGet: Install-Package IronPdf
using System;
using IronPdf;
using IronPdf.Rendering;

class IronPdfCustomSize
{
    static void Main()
    {
        var renderer = new ChromePdfRenderer();
        
        renderer.RenderingOptions.PaperSize = PdfPaperSize.Letter;
        renderer.RenderingOptions.MarginTop = 50;
        renderer.RenderingOptions.MarginBottom = 50;
        
        var html = "<html><body><h1>Custom Size PDF</h1></body></html>";
        var pdf = renderer.RenderHtmlAsPdf(html);
        
        pdf.SaveAs("custom-size.pdf");
        Console.WriteLine("Custom size PDF generated successfully");
    }
}