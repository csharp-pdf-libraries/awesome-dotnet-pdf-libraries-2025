// NuGet: Install-Package IronPdf
using IronPdf;

class Program
{
    static void Main()
    {
        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf("<h1>Main content of the document</h1>");
        
        pdf.AddTextHeader("Document Header");
        pdf.AddTextFooter("Page {page}");
        
        pdf.SaveAs("header-footer.pdf");
    }
}