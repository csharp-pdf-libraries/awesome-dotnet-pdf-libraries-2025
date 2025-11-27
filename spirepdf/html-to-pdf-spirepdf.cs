// NuGet: Install-Package Spire.PDF
using Spire.Pdf;
using Spire.Pdf.Graphics;
using System;

class Program
{
    static void Main()
    {
        PdfDocument pdf = new PdfDocument();
        PdfHtmlLayoutFormat htmlLayoutFormat = new PdfHtmlLayoutFormat();
        
        string htmlString = "<html><body><h1>Hello World</h1><p>This is a PDF from HTML.</p></body></html>";
        
        pdf.LoadFromHTML(htmlString, false, true, true);
        pdf.SaveToFile("output.pdf");
        pdf.Close();
    }
}