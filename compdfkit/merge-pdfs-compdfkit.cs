// NuGet: Install-Package ComPDFKit.NetCore
using ComPDFKit.PDFDocument;
using ComPDFKit.Import;
using System;

class Program
{
    static void Main()
    {
        var document1 = CPDFDocument.InitWithFilePath("file1.pdf");
        var document2 = CPDFDocument.InitWithFilePath("file2.pdf");
        
        // Import pages from document2 into document1
        document1.ImportPagesAtIndex(document2, "0-" + (document2.PageCount - 1), document1.PageCount);
        
        document1.WriteToFilePath("merged.pdf");
        document1.Release();
        document2.Release();
    }
}