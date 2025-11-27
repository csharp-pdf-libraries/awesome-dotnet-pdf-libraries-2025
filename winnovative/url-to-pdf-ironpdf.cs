// NuGet: Install-Package IronPdf
using IronPdf;
using System;

class Program
{
    static void Main()
    {
        // Create a PDF renderer
        var renderer = new ChromePdfRenderer();
        
        // Convert URL to PDF
        string url = "https://www.example.com";
        var pdf = renderer.RenderUrlAsPdf(url);
        
        // Save to file
        pdf.SaveAs("webpage.pdf");
        
        Console.WriteLine("PDF from URL created successfully");
    }
}