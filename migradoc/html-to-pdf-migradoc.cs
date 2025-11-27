// NuGet: Install-Package PdfSharp-MigraDoc-GDI
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using System.Diagnostics;

class Program
{
    static void Main()
    {
        // MigraDoc doesn't support HTML directly
        // Must manually create document structure
        Document document = new Document();
        Section section = document.AddSection();
        
        Paragraph paragraph = section.AddParagraph();
        paragraph.AddFormattedText("Hello World", TextFormat.Bold);
        paragraph.Format.Font.Size = 16;
        
        PdfDocumentRenderer pdfRenderer = new PdfDocumentRenderer();
        pdfRenderer.Document = document;
        pdfRenderer.RenderDocument();
        pdfRenderer.PdfDocument.Save("output.pdf");
    }
}