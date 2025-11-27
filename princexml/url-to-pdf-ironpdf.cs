// NuGet: Install-Package IronPdf
using IronPdf;
using System;

class Program
{
    static void Main()
    {
        var renderer = new ChromePdfRenderer();
        renderer.RenderingOptions.EnableJavaScript = true;
        renderer.RenderingOptions.PdfTitle = "Website Export";
        
        var pdf = renderer.RenderUrlAsPdf("https://example.com");
        pdf.Encrypt("password");
        pdf.SaveAs("webpage.pdf");
        Console.WriteLine("URL converted to PDF");
    }
}