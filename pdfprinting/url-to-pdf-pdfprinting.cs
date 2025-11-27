// NuGet: Install-Package PDFPrinting.NET
using PDFPrinting.NET;
using System;

class Program
{
    static void Main()
    {
        var converter = new WebPageToPdfConverter();
        string url = "https://www.example.com";
        converter.Convert(url, "webpage.pdf");
        Console.WriteLine("PDF from URL created successfully");
    }
}