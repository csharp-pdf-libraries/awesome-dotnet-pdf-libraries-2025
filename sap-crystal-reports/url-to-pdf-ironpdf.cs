// NuGet: Install-Package IronPdf
using IronPdf;
using System;

class Program
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        // Create a PDF from a URL
        var renderer = new ChromePdfRenderer();
        
        var pdf = renderer.RenderUrlAsPdf("https://example.com");
        pdf.SaveAs("output.pdf");
        
        Console.WriteLine("PDF created from URL successfully!");
    }
}