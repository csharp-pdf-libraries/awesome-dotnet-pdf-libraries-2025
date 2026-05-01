// NuGet: Install-Package PDFFilePrint
// PDFFilePrint is a .NET 4.6.1+ NuGet wrapper around PdfiumViewer / Google Pdfium.
// It only PRINTS existing PDF (or XPS) files — it cannot create or edit PDFs.
// Configuration (PrinterName, PaperName, Copies, PrintToFile, DefaultPrintToDirectory)
// is read from the consuming app's app.config / Settings.Default.
// Latest release: 1.0.3 (2020-02-10), MIT, author christiandersen.
using System;
using PDFFilePrint;

class Program
{
    static void Main()
    {
        // Second argument is the XPS / output filename when PrintToFile is enabled,
        // or null to print to the printer configured in app.config.
        var fileprint = new FilePrint("document.pdf", null);
        fileprint.Print();
        Console.WriteLine("PDF sent to printer");
    }
}
