// Adobe PDF Library SDK (Datalogics APDFL)
// NuGet: Install-Package Adobe.PDF.Library.LM.NET
// Namespace: Datalogics.PDFL
using Datalogics.PDFL;
using System;

class AdobeAddWatermark
{
    static void Main()
    {
        // Library lifecycle is required: Initialize at startup, Terminate at shutdown.
        using (Library lib = new Library())
        {
            using (Document doc = new Document("input.pdf"))
            {
                // Configure watermark placement / appearance
                WatermarkParams watermarkParams = new WatermarkParams();
                watermarkParams.Opacity = 0.5;
                watermarkParams.Rotation = 45.0;
                watermarkParams.Scale = -1; // auto-fit

                // Configure the text content of the watermark
                WatermarkTextParams textParams = new WatermarkTextParams();
                textParams.Text = "CONFIDENTIAL";
                textParams.Color = new Color(0.8, 0.8, 0.8);
                textParams.TextAlign = HorizontalAlignment.Center;

                // Apply the watermark via the Document.Watermark(...) method.
                // Reference: github.com/datalogics/apdfl-csharp-dotnet-samples
                //            ContentModification/Watermark
                doc.Watermark(textParams, watermarkParams);

                doc.Save(SaveFlags.Full, "watermarked.pdf");
            }
        }
    }
}
