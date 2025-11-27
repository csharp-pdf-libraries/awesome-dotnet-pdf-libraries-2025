// NuGet: Install-Package IronPdf
using IronPdf;
using System;

class Program
{
    static void Main()
    {
        var pdf = PdfDocument.FromFile("document.pdf");
        string text = pdf.ExtractAllText();
        Console.WriteLine(text);
        
        // Or extract text from specific pages
        string pageText = pdf.ExtractTextFromPage(0);
        Console.WriteLine(pageText);
    }
}