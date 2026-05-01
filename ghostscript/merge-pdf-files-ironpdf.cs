// NuGet: Install-Package IronPdf
using IronPdf;
using System.Collections.Generic;

class IronPdfExample
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

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