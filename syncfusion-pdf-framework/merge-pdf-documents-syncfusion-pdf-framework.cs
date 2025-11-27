// NuGet: Install-Package Syncfusion.Pdf.Net.Core
using Syncfusion.Pdf;
using Syncfusion.Pdf.Parsing;
using System.IO;

class Program
{
    static void Main()
    {
        // Load the first PDF document
        FileStream stream1 = new FileStream("Document1.pdf", FileMode.Open, FileAccess.Read);
        PdfLoadedDocument loadedDocument1 = new PdfLoadedDocument(stream1);
        
        // Load the second PDF document
        FileStream stream2 = new FileStream("Document2.pdf", FileMode.Open, FileAccess.Read);
        PdfLoadedDocument loadedDocument2 = new PdfLoadedDocument(stream2);
        
        // Merge the documents
        PdfDocument finalDocument = new PdfDocument();
        finalDocument.ImportPageRange(loadedDocument1, 0, loadedDocument1.Pages.Count - 1);
        finalDocument.ImportPageRange(loadedDocument2, 0, loadedDocument2.Pages.Count - 1);
        
        // Save the merged document
        FileStream outputStream = new FileStream("Merged.pdf", FileMode.Create);
        finalDocument.Save(outputStream);
        
        // Close all documents
        finalDocument.Close(true);
        loadedDocument1.Close(true);
        loadedDocument2.Close(true);
        stream1.Close();
        stream2.Close();
        outputStream.Close();
    }
}