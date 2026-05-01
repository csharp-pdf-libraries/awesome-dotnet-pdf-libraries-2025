// NuGet: Install-Package IronPdf
using IronPdf;

class Program
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var renderer = new ChromePdfRenderer();
        renderer.RenderingOptions.PaperSize = IronPdf.Rendering.PdfPaperSize.A4;
        renderer.RenderingOptions.MarginTop = 40;
        renderer.RenderingOptions.MarginBottom = 40;

        string html = "<html><body><h1>Custom PDF</h1><p>With custom margins and settings.</p></body></html>";
        var pdf = renderer.RenderHtmlAsPdf(html);
        pdf.SaveAs("custom.pdf");
    }
}
