// NuGet: Install-Package IronPdf
using IronPdf;

class Program
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        // Create a PDF from a URL
        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderUrlAsPdf("https://www.example.com");

        // Save the PDF
        pdf.SaveAs("Output.pdf");
    }
}