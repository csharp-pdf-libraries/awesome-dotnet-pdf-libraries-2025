// NuGet: Install-Package PdfiumViewer  (representative PDFium wrapper)
// PDFium is a PDF rendering / parsing engine. It has NO HTML parser, so no
// PDFium .NET wrapper (PdfiumViewer, PdfiumViewer.Updated, PDFiumCore,
// Pdfium.Net.SDK) can convert HTML to PDF on its own.
using PdfiumViewer;
using System;
using System.IO;
using System.Drawing.Printing;

class Program
{
    static void Main()
    {
        // PDFium has no native HTML-to-PDF capability.
        // Produce the PDF with another engine (wkhtmltopdf, headless Chromium,
        // IronPDF, etc.) and then load it with PdfDocument.Load(...) for rendering.
        string htmlContent = "<h1>Hello World</h1>";

        Console.WriteLine("HTML to PDF conversion is not supported by PDFium.");
    }
}