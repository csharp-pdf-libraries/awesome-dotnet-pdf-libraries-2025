// NuGet: Install-Package MuPDF.NET
// MuPDF.NET exposes Document.InsertPdf to merge PDFs (mirrors PyMuPDF's
// Document.insert_pdf). See https://mupdfnet.readthedocs.io/
using MuPDF.NET;

class Program
{
    static void Main()
    {
        Document doc1 = new Document("file1.pdf");
        Document doc2 = new Document("file2.pdf");

        // Append every page of doc2 to the end of doc1.
        doc1.InsertPdf(doc2);

        doc1.Save("merged.pdf");
    }
}
