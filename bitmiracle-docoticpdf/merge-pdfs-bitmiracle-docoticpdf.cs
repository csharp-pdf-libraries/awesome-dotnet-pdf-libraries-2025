// NuGet: Install-Package Docotic.Pdf
using BitMiracle.Docotic.Pdf;
using System;

class Program
{
    static void Main()
    {
        using (var pdf1 = new PdfDocument("document1.pdf"))
        using (var pdf2 = new PdfDocument("document2.pdf"))
        {
            pdf1.Append(pdf2);
            pdf1.Save("merged.pdf");
        }
        
        Console.WriteLine("PDFs merged successfully");
    }
}