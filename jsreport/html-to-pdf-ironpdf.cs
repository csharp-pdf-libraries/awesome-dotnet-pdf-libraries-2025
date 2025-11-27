// NuGet: Install-Package IronPdf
using IronPdf;
using System;

class Program
{
    static void Main(string[] args)
    {
        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf("<h1>Hello from IronPDF</h1><p>This is a PDF document.</p>");
        pdf.SaveAs("output.pdf");
        Console.WriteLine("PDF created successfully!");
    }
}