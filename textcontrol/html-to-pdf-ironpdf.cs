// NuGet: Install-Package IronPdf
using IronPdf;

namespace IronPdfExample
{
    class Program
    {
        static void Main(string[] args)
        {
            var renderer = new ChromePdfRenderer();
            
            string html = "<html><body><h1>Hello World</h1><p>This is a PDF document.</p></body></html>";
            
            var pdf = renderer.RenderHtmlAsPdf(html);
            pdf.SaveAs("output.pdf");
        }
    }
}