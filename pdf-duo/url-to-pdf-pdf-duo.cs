// PDF Duo .NET is NOT distributed via NuGet.
// Download the trial from https://www.duodimension.com/ and add reference to PDFDuo.dll
// Last released v2.4 in December 2010; targets .NET Framework 1.1-3.5 only.
using System;

class Program
{
    static void Main()
    {
        // The real API: namespace DuoDimension, class HtmlToPdf, methods OpenHTML / SavePDF.
        // OpenHTML accepts a URL string in addition to local HTML file paths.
        var conv = new DuoDimension.HtmlToPdf();
        conv.OpenHTML("https://www.example.com");
        conv.SavePDF("webpage.pdf");
        Console.WriteLine("Webpage converted to PDF!");
    }
}
