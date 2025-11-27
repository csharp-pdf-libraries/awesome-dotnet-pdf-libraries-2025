// NuGet: Install-Package PdfSharp-MigraDoc-GDI
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;

class Program
{
    static void Main()
    {
        Document document = new Document();
        Section section = document.AddSection();
        
        // Add header
        Paragraph headerPara = section.Headers.Primary.AddParagraph();
        headerPara.AddText("Document Header");
        headerPara.Format.Font.Size = 12;
        headerPara.Format.Alignment = ParagraphAlignment.Center;
        
        // Add footer
        Paragraph footerPara = section.Footers.Primary.AddParagraph();
        footerPara.AddText("Page ");
        footerPara.AddPageField();
        footerPara.Format.Alignment = ParagraphAlignment.Center;
        
        // Add content
        section.AddParagraph("Main content of the document");
        
        PdfDocumentRenderer pdfRenderer = new PdfDocumentRenderer();
        pdfRenderer.Document = document;
        pdfRenderer.RenderDocument();
        pdfRenderer.PdfDocument.Save("header-footer.pdf");
    }
}