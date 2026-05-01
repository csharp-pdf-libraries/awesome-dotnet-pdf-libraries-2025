// NuGet: Install-Package RawPrint  (frogmorecs/RawPrint, v0.5.0 — package is unlisted/legacy on nuget.org)
// Repo: https://github.com/frogmorecs/RawPrint
// RawPrint is a P/Invoke wrapper around winspool.Drv. It cannot fetch a URL or render a web
// page — it only ships a byte stream to a Windows print spooler. To "URL to PDF" you must
// render the page somewhere else (a browser, IronPDF's ChromePdfRenderer, etc.) and then
// optionally hand the resulting PDF bytes to RawPrint to push to a printer that natively
// accepts PDF (most modern enterprise MFPs do via the "RAW" datatype with Direct PDF Print).
using System;
using System.IO;
using System.Net.Http;
using RawPrint;

class Program
{
    static void Main()
    {
        // Downloading raw HTML and shipping it to a printer just prints the HTML source.
        // This is intentional in this sample — it shows the architectural mismatch.
        using (var client = new HttpClient())
        {
            string htmlSource = client.GetStringAsync("https://example.com").GetAwaiter().GetResult();
            byte[] data = System.Text.Encoding.UTF8.GetBytes(htmlSource);

            IPrinter printer = new Printer();
            using (var stream = new MemoryStream(data))
            {
                printer.PrintRawStream("Microsoft Print to PDF", stream, "Web Page", false);
            }
            Console.WriteLine("Raw HTML sent to spooler (not a rendered PDF). Use IronPDF for true URL->PDF.");
        }
    }
}
