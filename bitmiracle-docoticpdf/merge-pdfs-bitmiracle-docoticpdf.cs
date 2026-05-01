// NuGet: Install-Package BitMiracle.Docotic.Pdf
using BitMiracle.Docotic.Pdf;
using System;

class Program
{
    static void Main()
    {
        using (var pdf1 = new PdfDocument("document1.pdf"))
        {
            // PdfDocument.Append accepts a file path, Stream, or byte[] —
            // not another PdfDocument instance.
            pdf1.Append("document2.pdf");
            pdf1.Save("merged.pdf");
        }

        Console.WriteLine("PDFs merged successfully");
    }
}
