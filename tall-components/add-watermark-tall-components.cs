// NuGet: Install-Package TallComponents.PDFKit5
// (real package name is TallComponents.PDFKit5 — current 5.0.216, targets
//  .NET Standard 2.0. The 4.x line ships as TallComponents.PDFKit.)
// Note: TallComponents was acquired by Apryse (May 27, 2025) and no longer
// sells new licenses. Existing customers can still use the package; Apryse
// directs new buyers to the iText SDK.
using TallComponents.PDF;
using TallComponents.PDF.Shapes;
using System.IO;
using System.Drawing;

class Program
{
    static void Main()
    {
        // Load existing PDF
        using (FileStream fs = new FileStream("input.pdf", FileMode.Open, FileAccess.Read))
        {
            Document document = new Document(fs);

            // Iterate through pages
            foreach (Page page in document.Pages)
            {
                // Create watermark text shape
                TextShape watermark = new TextShape();
                watermark.Text = "CONFIDENTIAL";
                watermark.Font = new Font("Arial", 60);
                watermark.Pen = new Pen(Color.FromArgb(128, 255, 0, 0));
                watermark.X = 200;
                watermark.Y = 400;
                // Diagonal rotation is applied via a transform on PDFKit.NET
                // (TextShape has no Rotate property in the PDFKit API).
                watermark.Transform = new RotateTransform(45);

                // Add to the page's overlay canvas
                page.Overlay.Add(watermark);
            }

            // Save document
            using (FileStream output = new FileStream("watermarked.pdf", FileMode.Create))
            {
                document.Write(output);
            }
        }
    }
}
