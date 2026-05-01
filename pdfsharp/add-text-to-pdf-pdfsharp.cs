// NuGet: Install-Package PDFsharp  (official PDFsharp-Team package; v6.2.4 as of Jan 2026, MIT)
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
        // Note: in PDFsharp 6.x, XFontStyle was renamed to XFontStyleEx
        XFont font = new XFont("Arial", 20, XFontStyleEx.Bold);
        
        // Draw text at specific position
        gfx.DrawString("Watermark Text", font, XBrushes.Red,
            new XPoint(200, 400));
        
        document.Save("modified.pdf");
    }
}