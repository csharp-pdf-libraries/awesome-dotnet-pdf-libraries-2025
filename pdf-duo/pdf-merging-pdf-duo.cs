// NuGet: Install-Package PDFDuo.NET
using PDFDuo;
using System;

class Program
{
    static void Main()
    {
        var merger = new PdfMerger();
        merger.AddFile("document1.pdf");
        merger.AddFile("document2.pdf");
        merger.Merge("merged.pdf");
        Console.WriteLine("PDFs merged successfully!");
    }
}