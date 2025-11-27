// NuGet: Install-Package Spire.PDF
using Spire.Pdf;
using System;

class Program
{
    static void Main()
    {
        PdfDocument pdf1 = new PdfDocument();
        pdf1.LoadFromFile("document1.pdf");
        
        PdfDocument pdf2 = new PdfDocument();
        pdf2.LoadFromFile("document2.pdf");
        
        pdf1.InsertPageRange(pdf2, 0, pdf2.Pages.Count - 1);
        
        pdf1.SaveToFile("merged.pdf");
        pdf1.Close();
        pdf2.Close();
    }
}