// NuGet: Install-Package IronPdf
using IronPdf;
using System;

class Program
{
    static void Main(string[] args)
    {
        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderUrlAsPdf("https://example.com");
        pdf.SaveAs("webpage.pdf");
        Console.WriteLine("Webpage PDF created successfully!");
    }
}