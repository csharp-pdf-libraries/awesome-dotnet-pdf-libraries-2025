// NuGet: Install-Package PdfPrintingNet  (Terminalworks; current v5.4.2)
// NOTE: PDFPrinting.NET is a print-only library. There is no
// WebPageToPdfConverter class — URL-to-PDF is not part of the API.
// The library only handles printing, viewing, editing, and rasterizing
// existing PDF documents. To capture a URL as PDF you need a separate
// library (such as IronPDF) and can then hand the resulting PDF to
// PDFPrinting.NET for printing.
using PdfPrintingNet;
using System;

class Program
{
    static void Main()
    {
        // Workaround: produce the PDF elsewhere, then print it.
        string existingPdf = "captured-page.pdf";

        var pdfPrint = new PdfPrint("license-owner", "license-key");
        var status = pdfPrint.Print(existingPdf);

        Console.WriteLine($"Print status: {status}");
        Console.WriteLine("URL-to-PDF is not supported by PDFPrinting.NET.");
    }
}
