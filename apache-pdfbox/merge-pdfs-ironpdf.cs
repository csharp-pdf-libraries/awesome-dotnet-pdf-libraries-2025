// NuGet: Install-Package IronPdf
using IronPdf;
using System;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        var pdf1 = PdfDocument.FromFile("document1.pdf");
        var pdf2 = PdfDocument.FromFile("document2.pdf");
        var pdf3 = PdfDocument.FromFile("document3.pdf");
        
        var merged = PdfDocument.Merge(pdf1, pdf2, pdf3);
        merged.SaveAs("merged.pdf");
        Console.WriteLine("PDFs merged successfully");
    }
}