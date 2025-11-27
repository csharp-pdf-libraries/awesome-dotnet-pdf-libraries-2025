// NuGet: Install-Package IronPdf
using IronPdf;
using System;

class Program
{
    static void Main()
    {
        var renderer = new ChromePdfRenderer();
        var htmlContent = "<h1>Hello World</h1><p>This is a PDF document.</p>";
        var pdf = renderer.RenderHtmlAsPdf(htmlContent);
        pdf.SaveAs("output.pdf");
        Console.WriteLine("PDF created successfully!");
    }
}