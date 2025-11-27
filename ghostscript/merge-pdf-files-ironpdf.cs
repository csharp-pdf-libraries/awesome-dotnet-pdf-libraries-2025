// NuGet: Install-Package IronPdf
using IronPdf;
using System.Collections.Generic;

class IronPdfExample
{
    static void Main()
    {
        var pdfs = new List<PdfDocument>
        {
            PdfDocument.FromFile("file1.pdf"),
            PdfDocument.FromFile("file2.pdf"),
            PdfDocument.FromFile("file3.pdf")
        };
        
        var merged = PdfDocument.Merge(pdfs);
        merged.SaveAs("merged.pdf");
    }
}