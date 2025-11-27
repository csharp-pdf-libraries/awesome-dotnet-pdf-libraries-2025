// NuGet: Install-Package EO.Pdf
using EO.Pdf;
using System;

class Program
{
    static void Main()
    {
        string url = "https://www.example.com";
        
        HtmlToPdf.ConvertUrl(url, "webpage.pdf");
        
        Console.WriteLine("PDF from URL created successfully!");
    }
}