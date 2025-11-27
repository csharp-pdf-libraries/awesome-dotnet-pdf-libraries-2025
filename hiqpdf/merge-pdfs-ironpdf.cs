// NuGet: Install-Package IronPdf
using IronPdf;
using System;

class Program
{
    static void Main()
    {
        var renderer = new ChromePdfRenderer();
        
        // Create first PDF
        var pdf1 = renderer.RenderHtmlAsPdf("<h1>First Document</h1>");
        pdf1.SaveAs("doc1.pdf");
        
        // Create second PDF
        var pdf2 = renderer.RenderHtmlAsPdf("<h1>Second Document</h1>");
        pdf2.SaveAs("doc2.pdf");
        
        // Merge PDFs
        var merged = PdfDocument.Merge(pdf1, pdf2);
        merged.SaveAs("merged.pdf");
    }
}