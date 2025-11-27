// NuGet: Install-Package IronPdf
using IronPdf;

class IronPdfExample
{
    static void Main()
    {
        var renderer = new ChromePdfRenderer();
        
        renderer.RenderingOptions.TextHeader.CenterText = "Page {page}";
        renderer.RenderingOptions.TextFooter.CenterText = "{date}";
        
        var pdf = renderer.RenderUrlAsPdf("https://example.com");
        pdf.SaveAs("output.pdf");
    }
}