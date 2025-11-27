// NuGet: Install-Package IronPdf
using System;
using IronPdf;

class IronPdfUrlToPdf
{
    static void Main()
    {
        var renderer = new ChromePdfRenderer();
        
        var pdf = renderer.RenderUrlAsPdf("https://example.com");
        
        pdf.SaveAs("webpage.pdf");
        Console.WriteLine("PDF from URL generated successfully");
    }
}