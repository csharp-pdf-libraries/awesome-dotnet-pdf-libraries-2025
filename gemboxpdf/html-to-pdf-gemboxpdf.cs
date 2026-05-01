// NuGet: Install-Package GemBox.Document
// NOTE: GemBox.Pdf does NOT support HTML-to-PDF. PdfDocument.Load only opens
// existing PDF files. To convert HTML to PDF you must use the separate
// GemBox.Document product (different SKU, different license).
// Source: https://forum.gemboxsoftware.com/t/loading-pdf-document-from-html/966
using GemBox.Document;

class Program
{
    static void Main()
    {
        ComponentInfo.SetLicense("FREE-LIMITED-KEY");

        // GemBox.Document loads HTML and saves as PDF.
        var document = DocumentModel.Load("input.html");
        document.Save("output.pdf");
    }
}