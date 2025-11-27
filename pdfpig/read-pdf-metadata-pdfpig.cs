// NuGet: Install-Package PdfPig
using UglyToad.PdfPig;
using System;

class Program
{
    static void Main()
    {
        using (var document = PdfDocument.Open("input.pdf"))
        {
            var info = document.Information;
            Console.WriteLine($"Title: {info.Title}");
            Console.WriteLine($"Author: {info.Author}");
            Console.WriteLine($"Subject: {info.Subject}");
            Console.WriteLine($"Creator: {info.Creator}");
            Console.WriteLine($"Producer: {info.Producer}");
            Console.WriteLine($"Number of Pages: {document.NumberOfPages}");
        }
    }
}