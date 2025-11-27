// NuGet: Install-Package IronPdf
using IronPdf;
using System;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        var pdfs = new List<PdfDocument>
        {
            PdfDocument.FromFile("document1.pdf"),
            PdfDocument.FromFile("document2.pdf")
        };
        var merged = PdfDocument.Merge(pdfs);
        merged.SaveAs("merged.pdf");
        Console.WriteLine("PDFs merged successfully");
    }
}