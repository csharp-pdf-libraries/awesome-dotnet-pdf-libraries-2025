// NuGet: Install-Package IronPdf
using IronPdf;

class Program
{
    static void Main()
    {
        var htmlContent = "<html><body><h1>Hello World</h1></body></html>";
        
        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf(htmlContent);
        pdf.SaveAs("output.pdf");
    }
}