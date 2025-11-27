// NuGet: Install-Package PDFDuo.NET
using PDFDuo;
using System;

class Program
{
    static void Main()
    {
        var converter = new HtmlToPdfConverter();
        var htmlContent = "<h1>Hello World</h1><p>This is a PDF document.</p>";
        converter.ConvertHtmlString(htmlContent, "output.pdf");
        Console.WriteLine("PDF created successfully!");
    }
}