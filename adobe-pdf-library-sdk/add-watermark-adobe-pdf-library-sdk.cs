// Adobe PDF Library SDK
using Datalogics.PDFL;
using System;

class AdobeAddWatermark
{
    static void Main()
    {
        using (Library lib = new Library())
        {
            Document doc = new Document("input.pdf");
            
            // Create watermark with complex API
            WatermarkParams watermarkParams = new WatermarkParams();
            watermarkParams.Opacity = 0.5;
            watermarkParams.Rotation = 45.0;
            watermarkParams.VerticalAlignment = WatermarkVerticalAlignment.Center;
            watermarkParams.HorizontalAlignment = WatermarkHorizontalAlignment.Center;
            
            WatermarkTextParams textParams = new WatermarkTextParams();
            textParams.Text = "CONFIDENTIAL";
            
            Watermark watermark = new Watermark(doc, textParams, watermarkParams);
            
            doc.Save(SaveFlags.Full, "watermarked.pdf");
            doc.Dispose();
        }
    }
}