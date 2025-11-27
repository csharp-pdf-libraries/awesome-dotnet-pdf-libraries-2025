// NuGet: Install-Package IronPdf
using IronPdf;

class Program
{
    static void Main()
    {
        // Create a PDF from HTML string
        var renderer = new ChromePdfRenderer();
        string html = "<html><body><h1>Hello World</h1><p>This is a PDF from HTML.</p></body></html>";
        
        var pdf = renderer.RenderHtmlAsPdf(html);
        pdf.SaveAs("output.pdf");
    }
}