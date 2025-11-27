// NuGet: Install-Package ZetPDF
using ZetPDF;
using System;

class Program
{
    static void Main()
    {
        var converter = new HtmlToPdfConverter();
        var url = "https://www.example.com";
        converter.ConvertUrlToPdf(url, "webpage.pdf");
        Console.WriteLine("PDF from URL created successfully");
    }
}