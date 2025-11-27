// NuGet: Install-Package IronPdf
using IronPdf;
using IronPdf.Rendering;
using System;

namespace IronPdfExample
{
    class Program
    {
        static void Main(string[] args)
        {
            var renderer = new ChromePdfRenderer();
            
            renderer.RenderingOptions.TextHeader = new TextHeaderFooter()
            {
                CenterText = "Page Header",
                DrawDividerLine = true
            };
            
            renderer.RenderingOptions.TextFooter = new TextHeaderFooter()
            {
                CenterText = "Page {page} of {total-pages}",
                DrawDividerLine = true
            };
            
            var htmlContent = "<h1>Report Title</h1><p>Report content goes here.</p>";
            var pdf = renderer.RenderHtmlAsPdf(htmlContent);
            pdf.SaveAs("report.pdf");
            
            Console.WriteLine("PDF with headers and footers created successfully!");
        }
    }
}