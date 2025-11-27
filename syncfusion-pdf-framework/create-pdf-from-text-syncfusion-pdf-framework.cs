// NuGet: Install-Package Syncfusion.Pdf.Net.Core
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Drawing;
using System.IO;

class Program
{
    static void Main()
    {
        // Create a new PDF document
        PdfDocument document = new PdfDocument();
        
        // Add a page
        PdfPage page = document.Pages.Add();
        
        // Create a font
        PdfFont font = new PdfStandardFont(PdfFontFamily.Helvetica, 12);
        
        // Draw text
        page.Graphics.DrawString("Hello, World!", font, PdfBrushes.Black, new PointF(10, 10));
        
        // Save the document
        FileStream fileStream = new FileStream("Output.pdf", FileMode.Create);
        document.Save(fileStream);
        document.Close(true);
        fileStream.Close();
    }
}