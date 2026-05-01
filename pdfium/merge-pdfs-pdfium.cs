// NuGet: Install-Package PdfiumViewer  (representative open-source PDFium wrapper)
// PdfiumViewer and PDFiumCore do not expose PDF merge APIs - PDFium itself
// is a rendering / parsing engine, not a document-authoring engine.
// (Patagames Pdfium.Net.SDK exposes some document-edit operations but is commercial.)
using PdfiumViewer;
using System;
using System.IO;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        List<string> pdfFiles = new List<string>
        {
            "document1.pdf",
            "document2.pdf",
            "document3.pdf"
        };

        // To merge PDFs from a PDFium wrapper you must reach for another library
        // (PdfSharp, iText, IronPDF) or, with Patagames Pdfium.Net.SDK, use its
        // document-edit APIs.

        Console.WriteLine("PDF merging is not supported by PdfiumViewer / PDFiumCore.");
    }
}