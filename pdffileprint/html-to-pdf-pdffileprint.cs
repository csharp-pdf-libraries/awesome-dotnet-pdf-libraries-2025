// NuGet: Install-Package PDFFilePrint
// NOTE: PDFFilePrint is a print-only wrapper around Google Pdfium / PdfiumViewer.
// It does NOT support HTML-to-PDF conversion — there is no CreateFromHtml,
// SaveToFile, or document-creation API on the FilePrint class. To produce a PDF
// from HTML you must first render the HTML with a separate library, then hand
// the resulting PDF to PDFFilePrint for printing.
using System;

class Program
{
    static void Main()
    {
        throw new NotSupportedException(
            "PDFFilePrint cannot convert HTML to PDF. Use a renderer such as " +
            "IronPDF, PuppeteerSharp, or wkhtmltopdf to produce the PDF first, " +
            "then pass the file path to new FilePrint(pdfPath, null).Print().");
    }
}
