// NuGet: Install-Package O2S.Components.PDFView4NET.Win
// Note: PDFView4NET is a viewer/render/print toolkit (O2 Solutions). It does
// NOT provide HTML-to-PDF conversion. The vendor's separate product PDF4NET
// (O2S.Components.PDF4NET.Net) is the creation library, and even that does
// not ship a Chromium-based HTML renderer. To produce a PDF from a URL with
// PDFView4NET in scope you must pre-render externally and then load the
// resulting PDF for viewing/printing, as shown below.
using O2S.Components.PDFView4NET;
using System;

class Program
{
    static void Main()
    {
        // PDFView4NET cannot fetch https://example.com and convert it to PDF.
        // Assume an upstream step has produced "input.pdf"; PDFView4NET loads
        // it for display, printing, or rendering to image.
        PDFDocument document = new PDFDocument();
        document.Load("input.pdf");
        Console.WriteLine($"Loaded {document.PageCount} page(s); render via PDFPage.Render or display in PDFViewer control.");
        document.Close();
    }
}
