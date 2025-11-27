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
    }
}