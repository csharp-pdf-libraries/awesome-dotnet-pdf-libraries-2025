// PDF Duo .NET is NOT distributed via NuGet.
// Download the trial from https://www.duodimension.com/ and add reference to PDFDuo.dll
// Last released v2.4 in December 2010; targets .NET Framework 1.1-3.5 only.
using System;

class Program
{
    static void Main()
    {
        // The real API: namespace DuoDimension, class HtmlToPdf, methods OpenHTML / SavePDF.
        var conv = new DuoDimension.HtmlToPdf();
        // OpenHTML accepts a file path, URL, or HTML string written to disk first.
        // No method to convert an in-memory HTML string directly without a file.
        string htmlPath = @"input.html";
        System.IO.File.WriteAllText(htmlPath, "<h1>Hello World</h1><p>This is a PDF document.</p>");

        conv.OpenHTML(htmlPath);
        conv.SavePDF("output.pdf");
        Console.WriteLine("PDF created successfully!");
    }
}
