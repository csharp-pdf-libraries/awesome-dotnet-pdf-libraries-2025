// NuGet: Install-Package IronPdf
using IronPdf;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        // Load PDF documents
        var pdf1 = PdfDocument.FromFile("Document1.pdf");
        var pdf2 = PdfDocument.FromFile("Document2.pdf");
        
        // Merge PDFs
        var merged = PdfDocument.Merge(new List<PdfDocument> { pdf1, pdf2 });
        
        // Save the merged document
        merged.SaveAs("Merged.pdf");
    }
}