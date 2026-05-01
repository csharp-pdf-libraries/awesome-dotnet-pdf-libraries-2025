// NuGet: Install-Package IronPdf
using IronPdf;
using IronPdf.Editing;
using System;

class IronPdfAddWatermark
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var pdf = PdfDocument.FromFile("input.pdf");

        // Apply text watermark with a simple API. Opacity is an integer 0-100.
        pdf.ApplyWatermark("<h1 style='color:red;'>CONFIDENTIAL</h1>",
            opacity: 50,
            rotation: 45,
            verticalAlignment: VerticalAlignment.Middle,
            horizontalAlignment: HorizontalAlignment.Center);

        pdf.SaveAs("watermarked.pdf");
    }
}