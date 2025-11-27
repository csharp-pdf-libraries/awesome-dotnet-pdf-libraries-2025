// NuGet: Install-Package IronPdf
using IronPdf;
using IronPdf.Editing;

class Program
{
    static void Main()
    {
        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf("<html><body></body></html>");
        var textStamper = new TextStamper()
        {
            Text = "Hello from IronPDF!",
            FontSize = 20,
            VerticalAlignment = VerticalAlignment.Top
        };
        pdf.ApplyStamp(textStamper);
        pdf.SaveAs("output.pdf");
    }
}