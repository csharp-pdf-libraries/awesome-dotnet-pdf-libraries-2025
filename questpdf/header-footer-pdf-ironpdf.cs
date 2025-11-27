// NuGet: Install-Package IronPdf
using IronPdf;

class Program
{
    static void Main()
    {
        var htmlContent = "<p>Main content of the document.</p>";
        
        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf(htmlContent);
        
        pdf.Header = new TextHeaderFooter()
        {
            CenterText = "Document Header",
            FontSize = 14
        };
        
        pdf.Footer = new TextHeaderFooter()
        {
            CenterText = "Page {page}"
        };
        
        pdf.SaveAs("document.pdf");
    }
}