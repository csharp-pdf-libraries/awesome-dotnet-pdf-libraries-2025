// NuGet: Install-Package Gnostice.PDFOne.DLL
using Gnostice.PDFOne;
using Gnostice.PDFOne.Graphics;
using System;

class Program
{
    static void Main()
    {
        PDFDocument doc = new PDFDocument();
        doc.Open();
        
        PDFPage page = doc.Pages.Add();
        
        // PDFOne doesn't have direct HTML to PDF conversion
        // You need to use Document Studio for HTML conversion
        // Or manually parse and render HTML elements
        
        PDFTextElement textElement = new PDFTextElement();
        textElement.Text = "Simple text conversion instead of HTML";
        textElement.Draw(page, 10, 10);
        
        doc.Save("output.pdf");
        doc.Close();
    }
}