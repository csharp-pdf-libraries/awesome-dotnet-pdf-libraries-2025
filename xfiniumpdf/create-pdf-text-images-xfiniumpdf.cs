// NuGet: Install-Package Xfinium.Pdf
using Xfinium.Pdf;
using Xfinium.Pdf.Graphics;
using Xfinium.Pdf.Core;
using System.IO;

class Program
{
    static void Main()
    {
        PdfFixedDocument document = new PdfFixedDocument();
        PdfPage page = document.Pages.Add();
        
        PdfStandardFont font = new PdfStandardFont(PdfStandardFontFace.Helvetica, 24);
        PdfBrush brush = new PdfBrush(PdfRgbColor.Black);
        
        page.Graphics.DrawString("Sample PDF Document", font, brush, 50, 50);
        
        FileStream imageStream = File.OpenRead("image.jpg");
        PdfJpegImage image = new PdfJpegImage(imageStream);
        page.Graphics.DrawImage(image, 50, 100, 200, 150);
        imageStream.Close();
        
        document.Save("output.pdf");
    }
}