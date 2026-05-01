// NuGet: Install-Package RawPrint  (frogmorecs/RawPrint, v0.5.0 — package is unlisted/legacy on nuget.org)
// Repo: https://github.com/frogmorecs/RawPrint
// RawPrint is a thin P/Invoke wrapper over winspool.Drv (WritePrinter et al.) that sends a
// RAW byte stream to a Windows print spooler. It does NOT render HTML and does NOT generate
// PDFs — it ships bytes the printer already understands (PCL, PostScript, ESC/POS, ZPL, an
// already-rendered PDF for printers with PDF firmware, etc.).
using System;
using System.IO;
using RawPrint;

class Program
{
    static void Main()
    {
        // RawPrint cannot convert HTML to PDF. The closest you can do is hand it bytes the
        // printer firmware can interpret. Sending raw HTML to a printer just makes the printer
        // print the HTML source as text — it does not render the markup.
        string html = "<html><body><h1>Hello World</h1></body></html>";
        byte[] data = System.Text.Encoding.ASCII.GetBytes(html);

        IPrinter printer = new Printer();
        using (var stream = new MemoryStream(data))
        {
            // PrintRawStream(printerName, stream, documentName, paused)
            printer.PrintRawStream("Microsoft Print to PDF", stream, "HTML Document", false);
        }
        Console.WriteLine("Raw HTML bytes sent to spooler (not rendered). Use IronPDF for true HTML->PDF.");
    }
}
