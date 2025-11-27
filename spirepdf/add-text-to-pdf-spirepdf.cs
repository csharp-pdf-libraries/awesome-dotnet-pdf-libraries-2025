// NuGet: Install-Package Spire.PDF
using Spire.Pdf;
using Spire.Pdf.Graphics;
using System.Drawing;
using System;

class Program
{
    static void Main()
    {
        PdfDocument pdf = new PdfDocument();
        PdfPageBase page = pdf.Pages.Add();
        
        PdfFont font = new PdfFont(PdfFontFamily.Helvetica, 20);
        PdfBrush brush = new PdfSolidBrush(Color.Black);
        
        page.Canvas.DrawString("Hello from Spire.PDF!", font, brush, new PointF(50, 50));
        
        pdf.SaveToFile("output.pdf");
        pdf.Close();
    }
}