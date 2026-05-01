// NuGet: Install-Package PdfPrintingNet  (Terminalworks; current v5.4.2)
// NOTE: PDFPrinting.NET is a print-only library. It does NOT expose any
// HTML-to-PDF API — there is no HtmlToPdfConverter class. The library
// can only print, view, edit, and rasterize EXISTING PDF documents.
// To render HTML to PDF you must use a separate library (e.g. IronPDF)
// to produce the PDF first, then hand the file to PDFPrinting.NET.
using PdfPrintingNet;
using System;

class Program
{
    static void Main()
    {
        // PDFPrinting.NET cannot create a PDF from an HTML string.
        // The closest workflow is: generate the PDF with another tool,
        // then load and print it via PdfPrint.
        string existingPdf = "input.pdf";

        var pdfPrint = new PdfPrint("license-owner", "license-key");
        var status = pdfPrint.Print(existingPdf);

        Console.WriteLine($"Print status: {status}");
        Console.WriteLine("HTML-to-PDF is not supported by PDFPrinting.NET.");
    }
}
