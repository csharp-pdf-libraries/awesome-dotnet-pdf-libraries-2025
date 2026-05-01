// NuGet: Install-Package O2S.Components.PDFView4NET.Win
// Note: PDFView4NET (O2 Solutions) has no HTML-to-PDF API. There is no
// HtmlToPdfConverter / HtmlContent property in O2S.Components.PDFView4NET.
// HTML rendering is out of scope for this viewer toolkit, so an HTML string
// cannot be turned into a PDF here. The library can only load and render
// existing PDFs. The closest in-library workflow is to display a previously
// generated PDF in the viewer or render it to an image.
using O2S.Components.PDFView4NET;
using System;

class Program
{
    static void Main()
    {
        // Example: load an already-produced PDF (created by another tool such
        // as IronPDF or O2's PDF4NET) and report its page count.
        PDFDocument document = new PDFDocument();
        document.Load("document.pdf");
        Console.WriteLine($"PDFView4NET loaded document with {document.PageCount} page(s).");
        document.Close();
    }
}
