// ZetPDF is NOT on NuGet — install via SDK ZIP from https://zetpdf.com/download/.
// ZetPDF does NOT ship a one-line PdfMerger.MergeFiles() helper. Merging is performed
// the same way as in PDFsharp (which ZetPDF resembles): open each input with PdfReader,
// then loop pages into an output PdfDocument and Save(). The snippet below is a
// convenience-wrapper-style illustration; in real ZetPDF code you would write a manual
// loop. There is also no native watermark or password helper in ZetPDF that matches the
// IronPDF API surface — encryption is exposed via AES256 settings on the document.
using ZetPDF;
using System;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        // ZetPDF has no built-in PdfMerger — illustrative call only.
        var merger = new PdfMerger();
        var files = new List<string> { "document1.pdf", "document2.pdf" };
        merger.MergeFiles(files, "merged.pdf");
        Console.WriteLine("PDFs merged successfully");
    }
}