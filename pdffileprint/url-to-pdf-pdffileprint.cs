// NuGet: Install-Package PDFFilePrint
// NOTE: PDFFilePrint is a print-only wrapper around Google Pdfium / PdfiumViewer.
// It cannot fetch a URL or render HTML — it only sends an existing PDF/XPS file
// to a printer. URL-to-PDF requires a separate renderer (IronPDF, PuppeteerSharp,
// wkhtmltopdf). Once you have a PDF on disk, PDFFilePrint can print it.
using System;

class Program
{
    static void Main()
    {
        throw new NotSupportedException(
            "PDFFilePrint cannot render a URL. Render the URL to a PDF with a " +
            "different library first, then call new FilePrint(pdfPath, null).Print().");
    }
}
