// NuGet: Install-Package IronPdf
using IronPdf;
using IronPdf.Rendering;

namespace IronPdfExample
{
    class Program
    {
        static void Main(string[] args)
        {
            var renderer = new ChromePdfRenderer();
            
            string html = "<html><body><h1>Document Content</h1><p>Main body text.</p></body></html>";
            
            var pdf = renderer.RenderHtmlAsPdf(html);
            
            pdf.AddTextHeader("Document Header");
            pdf.AddTextFooter("Page {page} of {total-pages}");
            
            pdf.SaveAs("output.pdf");
        }
    }
}