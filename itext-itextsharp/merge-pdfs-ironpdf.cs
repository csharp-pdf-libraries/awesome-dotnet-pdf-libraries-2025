// NuGet: Install-Package IronPdf
using IronPdf;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        var pdfDocuments = new List<PdfDocument>
        {
            PdfDocument.FromFile("document1.pdf"),
            PdfDocument.FromFile("document2.pdf"),
            PdfDocument.FromFile("document3.pdf")
        };
        
        var merged = PdfDocument.Merge(pdfDocuments);
        merged.SaveAs("merged.pdf");
    }
}