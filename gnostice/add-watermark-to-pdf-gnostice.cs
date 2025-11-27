// NuGet: Install-Package Gnostice.PDFOne.DLL
using Gnostice.PDFOne;
using Gnostice.PDFOne.Graphics;
using System;
using System.Drawing;

class Program
{
    static void Main()
    {
        PDFDocument doc = new PDFDocument();
        doc.Load("input.pdf");
        
        PDFFont font = new PDFFont(PDFStandardFont.Helvetica, 48);
        
        foreach (PDFPage page in doc.Pages)
        {
            PDFTextElement watermark = new PDFTextElement();
            watermark.Text = "CONFIDENTIAL";
            watermark.Font = font;
            watermark.Color = Color.FromArgb(128, 255, 0, 0);
            watermark.RotationAngle = 45;
            
            watermark.Draw(page, 200, 400);
        }
        
        doc.Save("watermarked.pdf");
        doc.Close();
    }
}