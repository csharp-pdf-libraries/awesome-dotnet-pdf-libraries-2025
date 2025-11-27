// NuGet: Install-Package IronPdf
using System;
using IronPdf;

class Program
{
    static void Main(string[] args)
    {
        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf("<h1>Hello World</h1>");
        pdf.SaveAs("output.pdf");
        Console.WriteLine("PDF created successfully");
    }
}