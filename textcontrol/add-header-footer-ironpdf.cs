// NuGet: Install-Package IronPdf
using IronPdf;
using IronPdf.Rendering;

namespace IronPdfExample
{
    class Program
    {
        static void Main(string[] args)
        {
            IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

            var renderer = new ChromePdfRenderer();

            // Set headers/footers as part of rendering options before producing the PDF.
            renderer.RenderingOptions.TextHeader = new TextHeaderFooter
            {
                CenterText = "Document Header"
            };
            renderer.RenderingOptions.TextFooter = new TextHeaderFooter
            {
                CenterText = "Page {page} of {total-pages}"
            };

            string html = "<html><body><h1>Document Content</h1><p>Main body text.</p></body></html>";

            var pdf = renderer.RenderHtmlAsPdf(html);
            pdf.SaveAs("output.pdf");
        }
    }
}