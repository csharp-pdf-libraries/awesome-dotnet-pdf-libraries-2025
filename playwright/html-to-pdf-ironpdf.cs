// NuGet: Install-Package IronPdf
using IronPdf;

class Program
{
    static void Main(string[] args)
    {
        var renderer = new ChromePdfRenderer();
        
        string html = "<h1>Hello World</h1><p>This is a test PDF.</p>";
        var pdf = renderer.RenderHtmlAsPdf(html);
        pdf.SaveAs("output.pdf");
    }
}