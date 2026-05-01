// NuGet: Install-Package IronPdf
using IronPdf;
using System;
using System.Collections.Generic;
using System.Linq;

class Program
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        List<string> pdfFiles = new List<string>
        {
            "document1.pdf",
            "document2.pdf",
            "document3.pdf"
        };

        // PdfDocument.Merge accepts an IEnumerable<PdfDocument>, so load each file first.
        var docs = pdfFiles.Select(path => PdfDocument.FromFile(path)).ToList();
        var merged = PdfDocument.Merge(docs);
        merged.SaveAs("merged.pdf");

        Console.WriteLine("PDFs merged successfully");
    }
}