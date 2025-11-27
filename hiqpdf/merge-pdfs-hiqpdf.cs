// NuGet: Install-Package HiQPdf
using HiQPdf;
using System;

class Program
{
    static void Main()
    {
        // Create first PDF
        HtmlToPdf converter1 = new HtmlToPdf();
        byte[] pdf1 = converter1.ConvertHtmlToMemory("<h1>First Document</h1>", "");
        System.IO.File.WriteAllBytes("doc1.pdf", pdf1);
        
        // Create second PDF
        HtmlToPdf converter2 = new HtmlToPdf();
        byte[] pdf2 = converter2.ConvertHtmlToMemory("<h1>Second Document</h1>", "");
        System.IO.File.WriteAllBytes("doc2.pdf", pdf2);
        
        // Merge PDFs
        PdfDocument document1 = PdfDocument.FromFile("doc1.pdf");
        PdfDocument document2 = PdfDocument.FromFile("doc2.pdf");
        document1.AddDocument(document2);
        document1.WriteToFile("merged.pdf");
    }
}