// NuGet: Install-Package Xfinium.Pdf.NetStandard (or .NetCore)
using Xfinium.Pdf;
using Xfinium.Pdf.Graphics;
using System.IO;

class Program
{
    static void Main()
    {
        PdfFixedDocument document = new PdfFixedDocument();
        PdfPage page = document.Pages.Add();

        PdfStandardFont font = new PdfStandardFont(PdfStandardFontFace.Helvetica, 24);
        PdfBrush brush = new PdfBrush(new PdfRgbColor(0, 0, 0));

        page.Graphics.DrawString("Sample PDF Document", font, brush, 50, 50);

        using (FileStream imageStream = File.OpenRead("image.jpg"))
        {
            PdfJpegImage image = new PdfJpegImage(imageStream);
            page.Graphics.DrawImage(image, 50, 100, 200, 150);
        }

        document.Save("output.pdf");
    }
}