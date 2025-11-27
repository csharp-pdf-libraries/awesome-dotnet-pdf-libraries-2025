// NuGet: Install-Package Xfinium.Pdf
using Xfinium.Pdf;
using Xfinium.Pdf.Actions;
using Xfinium.Pdf.FlowDocument;
using System.IO;

class Program
{
    static void Main()
    {
        PdfFixedDocument document = new PdfFixedDocument();
        PdfFlowDocument flowDocument = new PdfFlowDocument();
        
        string html = "<html><body><h1>Hello World</h1><p>This is a PDF from HTML.</p></body></html>";
        
        PdfFlowContent content = new PdfFlowContent();
        content.AppendHtml(html);
        flowDocument.AddContent(content);
        
        flowDocument.RenderDocument(document);
        document.Save("output.pdf");
    }
}