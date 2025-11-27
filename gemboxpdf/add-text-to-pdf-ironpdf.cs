// NuGet: Install-Package IronPdf
using IronPdf;
using IronPdf.Editing;

class Program
{
    static void Main()
    {
        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf("<p>Original Content</p>");
        
        var stamper = new TextStamper()
        {
            Text = "Hello World",
            FontSize = 24,
            HorizontalOffset = 100,
            VerticalOffset = 700
        };
        
        pdf.ApplyStamp(stamper);
        pdf.SaveAs("output.pdf");
    }
}