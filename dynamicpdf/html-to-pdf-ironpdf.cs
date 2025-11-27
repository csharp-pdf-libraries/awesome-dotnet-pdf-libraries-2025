// NuGet: Install-Package IronPdf
using IronPdf;

class Program
{
    static void Main()
    {
        string html = "<html><body><h1>Hello World</h1></body></html>";
        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf(html);
        pdf.SaveAs("output.pdf");
    }
}