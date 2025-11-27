// Apache PDFBox .NET port attempt (incomplete support)
using PdfBoxDotNet.Pdmodel;
using PdfBoxDotNet.Multipdf;
using System;
using System.IO;

class Program
{
    static void Main()
    {
        // PDFBox-dotnet ports have incomplete API coverage
        var merger = new PDFMergerUtility();
        merger.AddSource("document1.pdf");
        merger.AddSource("document2.pdf");
        merger.SetDestinationFileName("merged.pdf");
        merger.MergeDocuments();
        Console.WriteLine("PDFs merged");
    }
}