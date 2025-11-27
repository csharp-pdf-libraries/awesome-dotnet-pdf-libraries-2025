// NuGet: Install-Package PdfSharp-MigraDoc-GDI
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;

class Program
{
    static void Main()
    {
        Document document = new Document();
        Section section = document.AddSection();
        
        Table table = section.AddTable();
        table.Borders.Width = 0.75;
        
        Column column1 = table.AddColumn("3cm");
        Column column2 = table.AddColumn("3cm");
        
        Row row1 = table.AddRow();
        row1.Cells[0].AddParagraph("Name");
        row1.Cells[1].AddParagraph("Age");
        
        Row row2 = table.AddRow();
        row2.Cells[0].AddParagraph("John");
        row2.Cells[1].AddParagraph("30");
        
        PdfDocumentRenderer pdfRenderer = new PdfDocumentRenderer();
        pdfRenderer.Document = document;
        pdfRenderer.RenderDocument();
        pdfRenderer.PdfDocument.Save("table.pdf");
    }
}