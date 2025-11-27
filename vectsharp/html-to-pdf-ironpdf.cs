// NuGet: Install-Package IronPdf
using IronPdf;

class Program
{
    static void Main()
    {
        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf("<h1>Hello from IronPDF</h1><p>This is HTML content.</p>");
        pdf.SaveAs("output.pdf");
    }
}