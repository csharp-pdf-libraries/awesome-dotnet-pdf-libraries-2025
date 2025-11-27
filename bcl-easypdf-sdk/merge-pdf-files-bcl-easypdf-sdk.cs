// NuGet: Install-Package BCL.EasyPDF
using BCL.EasyPDF;
using System;

class Program
{
    static void Main()
    {
        var pdf1 = new PDFDocument("document1.pdf");
        var pdf2 = new PDFDocument("document2.pdf");
        pdf1.Append(pdf2);
        pdf1.Save("merged.pdf");
        pdf1.Close();
        pdf2.Close();
    }
}