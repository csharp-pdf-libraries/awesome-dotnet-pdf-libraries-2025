// NuGet: Install-Package TallComponents.PDF.Kit
using TallComponents.PDF.Kit;
using TallComponents.PDF.Layout;
using System.IO;
using System.Drawing;

class Program
{
    static void Main()
    {
        // Load existing PDF
        using (FileStream fs = new FileStream("input.pdf", FileMode.Open))
        using (Document document = new Document(fs))
        {
            // Iterate through pages
            foreach (Page page in document.Pages)
            {
                // Create watermark text
                TextShape watermark = new TextShape();
                watermark.Text = "CONFIDENTIAL";
                watermark.Font = new Font("Arial", 60);
                watermark.PenColor = Color.FromArgb(128, 255, 0, 0);
                watermark.X = 200;
                watermark.Y = 400;
                watermark.Rotate = 45;
                
                // Add to page
                page.Overlay.Shapes.Add(watermark);
            }
            
            // Save document
            using (FileStream output = new FileStream("watermarked.pdf", FileMode.Create))
            {
                document.Write(output);
            }
        }
    }
}