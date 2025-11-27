// NuGet: Install-Package IronPdf
using IronPdf;

class Program
{
    static void Main()
    {
        // Load PDFs
        var pdf1 = PdfDocument.FromFile("document1.pdf");
        var pdf2 = PdfDocument.FromFile("document2.pdf");
        
        // Merge PDFs
        var merged = PdfDocument.Merge(pdf1, pdf2);
        
        // Save merged document
        merged.SaveAs("merged.pdf");
    }
}