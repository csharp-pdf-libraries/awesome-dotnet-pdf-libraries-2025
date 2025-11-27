// NuGet: Install-Package PdfSharp
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using PdfSharp.Drawing;
using System;

class Program
{
    static void Main()
    {
        // Open existing PDF
        PdfDocument document = PdfReader.Open("existing.pdf", PdfDocumentOpenMode.Modify);
        PdfPage page = document.Pages[0];
        
        // Get graphics object
        XGraphics gfx = XGraphics.FromPdfPage(page);
        XFont font = new XFont("Arial", 20, XFontStyle.Bold);
        
        // Draw text at specific position
        gfx.DrawString("Watermark Text", font, XBrushes.Red,
            new XPoint(200, 400));
        
        document.Save("modified.pdf");
    }
}