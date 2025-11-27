// NuGet: Install-Package IronPdf
using IronPdf;
using IronPdf.Editing;
using System;

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
            VerticalOffset = 50,
            HorizontalOffset = 50
        };
        
        pdf.ApplyStamp(textStamper);
        pdf.SaveAs("output.pdf");
    }
}