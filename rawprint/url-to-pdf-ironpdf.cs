// NuGet: Install-Package IronPdf
using IronPdf;
using System;

class Program
{
    static void Main()
    {
        var renderer = new ChromePdfRenderer();
        // Render a live website directly to PDF with full CSS, JavaScript, and images
        var pdf = renderer.RenderUrlAsPdf("https://example.com");
        pdf.SaveAs("webpage.pdf");
        Console.WriteLine("Website rendered to PDF successfully");
    }
}