// NuGet: Install-Package EO.Pdf
using EO.Pdf;
using System;

class Program
{
    static void Main()
    {
        PdfDocument doc1 = new PdfDocument("file1.pdf");
        PdfDocument doc2 = new PdfDocument("file2.pdf");

        // EO.Pdf has no Append; merge is a static method that returns a new PdfDocument.
        PdfDocument mergedDoc = PdfDocument.Merge(doc1, doc2);

        mergedDoc.Save("merged.pdf");

        Console.WriteLine("PDFs merged successfully!");
    }
}