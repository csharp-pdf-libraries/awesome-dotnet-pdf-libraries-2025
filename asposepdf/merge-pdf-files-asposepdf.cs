// NuGet: Install-Package Aspose.PDF
using Aspose.Pdf;
using System;

class Program
{
    static void Main()
    {
        var document1 = new Document("file1.pdf");
        var document2 = new Document("file2.pdf");
        
        foreach (Page page in document2.Pages)
        {
            document1.Pages.Add(page);
        }
        
        document1.Save("merged.pdf");
        Console.WriteLine("PDFs merged successfully");
    }
}