// NuGet: Install-Package MuPDF.NET
using MuPDFCore;
using System.IO;

class Program
{
    static void Main()
    {
        using (MuPDFDocument doc1 = new MuPDFDocument("file1.pdf"))
        using (MuPDFDocument doc2 = new MuPDFDocument("file2.pdf"))
        {
            // Create a new document
            using (MuPDFDocument mergedDoc = MuPDFDocument.Create())
            {
                // Copy pages from first document
                for (int i = 0; i < doc1.Pages.Count; i++)
                {
                    mergedDoc.CopyPage(doc1, i);
                }
                
                // Copy pages from second document
                for (int i = 0; i < doc2.Pages.Count; i++)
                {
                    mergedDoc.CopyPage(doc2, i);
                }
                
                mergedDoc.Save("merged.pdf");
            }
        }
    }
}