// NuGet: Install-Package PdfSharp
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using System;

class Program
{
    static void Main()
    {
        // Create new PDF document
        PdfDocument document = new PdfDocument();
        PdfPage page = document.AddPage();
        XGraphics gfx = XGraphics.FromPdfPage(page);
        
        // Load and draw image
        XImage image = XImage.FromFile("image.jpg");
        
        // Calculate size to fit page
        double width = 200;
        double height = 200;
        
        gfx.DrawImage(image, 50, 50, width, height);
        
        // Add text
        XFont font = new XFont("Arial", 16);
        gfx.DrawString("Image in PDF", font, XBrushes.Black,
            new XPoint(50, 270));
        
        document.Save("output.pdf");
    }
}