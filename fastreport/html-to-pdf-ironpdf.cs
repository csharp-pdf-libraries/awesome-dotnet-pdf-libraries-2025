// NuGet: Install-Package IronPdf
using IronPdf;

class Program
{
    static void Main()
    {
        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf("<html><body><h1>Hello World</h1><p>This is a test PDF</p></body></html>");
        pdf.SaveAs("output.pdf");
    }
}