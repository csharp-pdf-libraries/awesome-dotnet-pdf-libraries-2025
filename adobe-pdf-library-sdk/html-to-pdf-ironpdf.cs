// NuGet: Install-Package IronPdf
using IronPdf;
using System;

class IronPdfHtmlToPdf
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var renderer = new ChromePdfRenderer();
        string htmlContent = "<html><body><h1>Hello World</h1></body></html>";

        // Convert HTML to PDF with a simple API.
        var pdf = renderer.RenderHtmlAsPdf(htmlContent);
        pdf.SaveAs("output.pdf");
    }
}