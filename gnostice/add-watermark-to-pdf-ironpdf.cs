// NuGet: Install-Package IronPdf
using IronPdf;
using IronPdf.Editing;
using System;

class Program
{
    static void Main()
    {
        var pdf = PdfDocument.FromFile("input.pdf");
        
        var watermark = new TextStamper()
        {
            Text = "CONFIDENTIAL",
            FontSize = 48,
            Opacity = 50,
            Rotation = 45,
            VerticalAlignment = VerticalAlignment.Middle,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        
        pdf.ApplyStamp(watermark);
        pdf.SaveAs("watermarked.pdf");
    }
}