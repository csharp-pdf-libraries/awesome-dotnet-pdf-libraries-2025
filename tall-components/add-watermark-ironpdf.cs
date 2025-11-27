// NuGet: Install-Package IronPdf
using IronPdf;
using IronPdf.Editing;

class Program
{
    static void Main()
    {
        // Load existing PDF
        var pdf = PdfDocument.FromFile("input.pdf");
        
        // Create watermark
        var watermark = new TextStamper()
        {
            Text = "CONFIDENTIAL",
            FontSize = 60,
            Opacity = 50,
            Rotation = 45,
            VerticalAlignment = VerticalAlignment.Middle,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        
        // Apply watermark to all pages
        pdf.ApplyStamp(watermark);
        
        // Save watermarked PDF
        pdf.SaveAs("watermarked.pdf");
    }
}