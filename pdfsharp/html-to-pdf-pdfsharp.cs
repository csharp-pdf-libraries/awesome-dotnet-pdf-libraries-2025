// NuGet: Install-Package PdfSharp
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using System;

class Program
{
    static void Main()
    {
        // PDFSharp does not have built-in HTML to PDF conversion
        // You need to manually parse HTML and render content
        PdfDocument document = new PdfDocument();
        PdfPage page = document.AddPage();
        XGraphics gfx = XGraphics.FromPdfPage(page);
        XFont font = new XFont("Arial", 12);
        
        // Manual text rendering (no HTML support)
        gfx.DrawString("Hello from PDFSharp", font, XBrushes.Black,
            new XRect(0, 0, page.Width, page.Height),
            XStringFormats.TopLeft);
        
        document.Save("output.pdf");
    }
}