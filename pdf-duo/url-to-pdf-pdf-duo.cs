// NuGet: Install-Package PDFDuo.NET
using PDFDuo;
using System;

class Program
{
    static void Main()
    {
        var converter = new HtmlToPdfConverter();
        converter.ConvertUrl("https://www.example.com", "webpage.pdf");
        Console.WriteLine("Webpage converted to PDF!");
    }
}