// NuGet: Install-Package IronPdf
using IronPdf;
using System.IO;

class Program
{
    static void Main()
    {
        var renderer = new ChromePdfRenderer();
        var htmlContent = "<html><body><h1>Hello World</h1><p>This is a PDF document.</p></body></html>";
        var pdf = renderer.RenderHtmlAsPdf(htmlContent);
        pdf.SaveAs("output.pdf");
    }
}