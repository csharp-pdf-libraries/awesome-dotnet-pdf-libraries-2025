// NuGet: Install-Package IronPdf
using IronPdf;
using IronPdf.Rendering;

class Program
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        // Create a PDF from HTML string
        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf("<h1>Hello, World!</h1>");

        // Save the document
        pdf.SaveAs("Output.pdf");
    }
}