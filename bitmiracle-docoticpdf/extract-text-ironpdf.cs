// NuGet: Install-Package IronPdf
using IronPdf;
using System;

class Program
{
    static void Main()
    {
        var pdf = PdfDocument.FromFile("document.pdf");
        string allText = pdf.ExtractAllText();
        
        Console.WriteLine("Extracted text:");
        Console.WriteLine(allText);
    }
}