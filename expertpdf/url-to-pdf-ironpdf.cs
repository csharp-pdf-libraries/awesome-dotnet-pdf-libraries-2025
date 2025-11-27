// NuGet: Install-Package IronPdf
using IronPdf;
using System;

class Program
{
    static void Main()
    {
        // Create a PDF renderer
        var renderer = new ChromePdfRenderer();
        
        // Set page size and orientation
        renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
        renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Portrait;
        
        // Convert URL to PDF
        var pdf = renderer.RenderUrlAsPdf("https://www.example.com");
        
        // Save to file
        pdf.SaveAs("webpage.pdf");
        
        Console.WriteLine("PDF from URL created successfully!");
    }
}