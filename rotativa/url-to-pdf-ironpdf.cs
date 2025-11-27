// NuGet: Install-Package IronPdf
using IronPdf;
using System;

namespace IronPdfExample
{
    class Program
    {
        static void Main(string[] args)
        {
            var renderer = new ChromePdfRenderer();
            
            var pdf = renderer.RenderUrlAsPdf("https://www.example.com");
            pdf.SaveAs("webpage.pdf");
            
            Console.WriteLine("URL converted to PDF successfully!");
        }
    }
}