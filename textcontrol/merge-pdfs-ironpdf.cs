// NuGet: Install-Package IronPdf
using IronPdf;

namespace IronPdfExample
{
    class Program
    {
        static void Main(string[] args)
        {
            var pdf1 = PdfDocument.FromFile("document1.pdf");
            var pdf2 = PdfDocument.FromFile("document2.pdf");
            
            var merged = PdfDocument.Merge(pdf1, pdf2);
            merged.SaveAs("merged.pdf");
        }
    }
}