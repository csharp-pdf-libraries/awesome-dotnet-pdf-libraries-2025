// NuGet: Install-Package IronPdf
using IronPdf;
using System;

class IronPdfExample
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var pdf = PdfDocument.FromFile("input.pdf");

        // One-shot: write every page as PNG matching Ghostscript's -r300 -sDEVICE=png16m
        pdf.RasterizeToImageFiles("output_page*.png", DPI: 300);
    }
}