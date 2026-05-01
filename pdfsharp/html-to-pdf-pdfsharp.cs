// NuGet: Install-Package PDFsharp  (official PDFsharp-Team package; v6.2.4 as of Jan 2026, MIT)
// PDFsharp does NOT support HTML-to-PDF natively. The community add-on
// HtmlRenderer.PdfSharp covers HTML 4.01 / CSS level 2 only (no flexbox, grid, JS).
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