// NuGet: Install-Package PDFPrinting.NET
using PDFPrinting.NET;
using System;

class Program
{
    static void Main()
    {
        var converter = new HtmlToPdfConverter();
        string html = "<html><body><h1>Hello World</h1></body></html>";
        converter.ConvertHtmlToPdf(html, "output.pdf");
        Console.WriteLine("PDF created successfully");
    }
}