// Apache PDFBox .NET ports are experimental and incomplete
using PdfBoxDotNet.Pdmodel;
using PdfBoxDotNet.Text;
using System;
using System.IO;

class Program
{
    static void Main()
    {
        // Note: PDFBox-dotnet has limited functionality
        using (var document = PDDocument.Load("document.pdf"))
        {
            var stripper = new PDFTextStripper();
            string text = stripper.GetText(document);
            Console.WriteLine(text);
        }
    }
}