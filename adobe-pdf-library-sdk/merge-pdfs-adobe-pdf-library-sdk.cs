// Adobe PDF Library SDK (Datalogics APDFL)
// NuGet: Install-Package Adobe.PDF.Library.LM.NET
// Namespace: Datalogics.PDFL
using Datalogics.PDFL;
using System;

class AdobeMergePdfs
{
    static void Main()
    {
        // Library lifecycle is required.
        using (Library lib = new Library())
        {
            using (Document doc1 = new Document("document1.pdf"))
            using (Document doc2 = new Document("document2.pdf"))
            {
                // InsertPages(insertAfter, sourceDoc, sourceStartPage, pageCount, flags)
                // Document.LastPage and Document.AllPages are well-known constants.
                // Reference: github.com/datalogics/apdfl-csharp-dotnet-samples
                //            ContentModification/MergePDF
                doc1.InsertPages(
                    Document.LastPage,
                    doc2,
                    0,
                    Document.AllPages,
                    PageInsertFlags.Bookmarks | PageInsertFlags.Threads);

                doc1.Save(SaveFlags.Full, "merged.pdf");
            }
        }
    }
}
