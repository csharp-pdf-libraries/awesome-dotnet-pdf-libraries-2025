// NuGet: Install-Package IronPdf
using IronPdf;
using System;

class Program
{
    static void Main()
    {
        var pdf = PdfDocument.FromFile("input.pdf");
        var info = pdf.MetaData;
        Console.WriteLine($"Title: {info.Title}");
        Console.WriteLine($"Author: {info.Author}");
        Console.WriteLine($"Subject: {info.Subject}");
        Console.WriteLine($"Creator: {info.Creator}");
        Console.WriteLine($"Producer: {info.Producer}");
        Console.WriteLine($"Number of Pages: {pdf.PageCount}");
    }
}