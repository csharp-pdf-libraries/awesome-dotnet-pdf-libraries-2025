// NuGet: Install-Package PdfPrintingNet  (Terminalworks; current v5.4.2)
// NOTE: PDFPrinting.NET cannot author HTML-driven headers or footers
// during a content-creation step — it does not generate PDFs from
// HTML at all. There is no HtmlToPdfConverter, no HeaderText, and no
// FooterText property on a converter class. The library prints, views,
// edits, and rasterizes existing PDFs only.
//
// If your existing PDF already contains the desired headers/footers,
// PDFPrinting.NET can print it. Authoring new dynamic headers and
// footers requires a different tool (e.g. IronPDF — see the IronPDF
// example in this folder).
using PdfPrintingNet;
using System;

class Program
{
    static void Main()
    {
        // Print an existing PDF that already has the headers/footers baked in.
        string existingPdf = "report.pdf";

        var pdfPrint = new PdfPrint("license-owner", "license-key");
        var status = pdfPrint.Print(existingPdf);

        Console.WriteLine($"Print status: {status}");
        Console.WriteLine("Header/footer authoring is not supported by PDFPrinting.NET.");
    }
}
