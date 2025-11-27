// NuGet: Install-Package EO.Pdf
using EO.Pdf;
using System;

class Program
{
    static void Main()
    {
        PdfDocument doc1 = new PdfDocument("file1.pdf");
        PdfDocument doc2 = new PdfDocument("file2.pdf");
        
        PdfDocument mergedDoc = new PdfDocument();
        mergedDoc.Append(doc1);
        mergedDoc.Append(doc2);
        
        mergedDoc.Save("merged.pdf");
        
        Console.WriteLine("PDFs merged successfully!");
    }
}