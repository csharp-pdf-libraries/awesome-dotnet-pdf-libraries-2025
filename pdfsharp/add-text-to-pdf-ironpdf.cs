// NuGet: Install-Package IronPdf
using IronPdf;
using IronPdf.Editing;
using System;

class Program
{
    static void Main()
    {
        // Open existing PDF
        var pdf = PdfDocument.FromFile("existing.pdf");
        
        // Add text stamp/watermark
        var textStamper = new TextStamper()
        {
            Text = "Watermark Text",
            FontSize = 20,
            Color = IronSoftware.Drawing.Color.Red,
            VerticalAlignment = VerticalAlignment.Middle,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        
        pdf.ApplyStamp(textStamper);
        pdf.SaveAs("modified.pdf");
    }
}