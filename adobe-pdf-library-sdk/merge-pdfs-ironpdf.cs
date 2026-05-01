// NuGet: Install-Package IronPdf
using IronPdf;
using System;

class IronPdfMergePdfs
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        // Load PDF documents
        var pdf1 = PdfDocument.FromFile("document1.pdf");
        var pdf2 = PdfDocument.FromFile("document2.pdf");

        // Merge PDFs with a single static call.
        var merged = PdfDocument.Merge(pdf1, pdf2);
        merged.SaveAs("merged.pdf");
    }
}