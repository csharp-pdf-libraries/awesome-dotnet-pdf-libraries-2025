// NuGet: Install-Package IronPdf
using IronPdf;
using System;
using System.Drawing.Printing;

class Program
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var pdf = PdfDocument.FromFile("document.pdf");

        // Silent print to the default printer
        pdf.Print();

        // Or print to a named printer with explicit PrinterSettings:
        // var settings = new PrinterSettings { PrinterName = "HP LaserJet Pro", Copies = 1 };
        // pdf.GetPrintDocument(settings).Print();

        Console.WriteLine("PDF sent to printer");
    }
}
