// PDF Duo .NET is NOT distributed via NuGet.
// Download the trial from https://www.duodimension.com/ and add reference to PDFDuo.dll
// Last released v2.4 in December 2010; targets .NET Framework 1.1-3.5 only.
//
// NOTE: PDF Duo .NET is an HTML-to-PDF converter. The vendor docs do not document
// a built-in PDF-merge API on the DuoDimension.HtmlToPdf class. Teams that needed
// to merge PDFs typically combined PDF Duo .NET with a separate library
// (e.g., iTextSharp 4.x). This sample shows the conceptual pattern only.
using System;

class Program
{
    static void Main()
    {
        // PDF Duo .NET has no native PdfMerger class.
        // Practical workaround used by 2010-era ASP.NET teams: render each HTML page
        // to its own PDF with DuoDimension.HtmlToPdf, then post-process / concatenate
        // them with another component. There is no first-party merge call.
        var conv = new DuoDimension.HtmlToPdf();
        conv.OpenHTML("page1.html");
        conv.SavePDF("document1.pdf");

        conv = new DuoDimension.HtmlToPdf();
        conv.OpenHTML("page2.html");
        conv.SavePDF("document2.pdf");

        // To produce merged.pdf you must use a separate PDF library.
        Console.WriteLine("PDF Duo produces individual PDFs; merging requires another library.");
    }
}
