// NuGet: Install-Package IronPdf
using IronPdf;
using System;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        var pdf1 = PdfDocument.FromFile("file1.pdf");
        var pdf2 = PdfDocument.FromFile("file2.pdf");
        
        var merged = PdfDocument.Merge(new List<PdfDocument> { pdf1, pdf2 });
        merged.SaveAs("merged.pdf");
    }
}