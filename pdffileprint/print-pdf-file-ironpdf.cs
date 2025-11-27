// NuGet: Install-Package IronPdf
using IronPdf;
using System;

class Program
{
    static void Main()
    {
        var pdf = PdfDocument.FromFile("document.pdf");
        pdf.Print();
        Console.WriteLine("PDF sent to printer");
    }
}