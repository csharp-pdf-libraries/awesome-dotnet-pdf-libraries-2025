// NuGet: Install-Package IronPdf
using IronPdf;
using System;

class Program
{
    static void Main()
    {
        var pdf = PdfDocument.FromFile("document.pdf");
        
        // Extract text from all pages
        string allText = pdf.ExtractAllText();
        Console.WriteLine("Extracted Text:");
        Console.WriteLine(allText);
        
        // Extract text from specific page
        string pageText = pdf.ExtractTextFromPage(0);
        Console.WriteLine($"\nFirst Page Text:\n{pageText}");
    }
}