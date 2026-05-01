// NuGet: Install-Package IronPdf
using IronPdf;

class Program
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
        var renderer = new ChromePdfRenderer();
        renderer.RenderingOptions.TextHeader = new TextHeaderFooter
        {
            CenterText = "Document Header"
        };
        renderer.RenderingOptions.TextFooter = new TextHeaderFooter
        {
            CenterText = "Page {page}"
        };

        var pdf = renderer.RenderHtmlAsPdf("<h1>Main content of the document</h1>");
        pdf.SaveAs("header-footer.pdf");
    }
}