// NuGet: Install-Package PdfiumViewer
using PdfiumViewer;
using System;
using System.Text;

string pdfPath = "document.pdf";

// PDFiumViewer has limited text extraction capabilities
// It's primarily designed for rendering, not text extraction
using (var document = PdfDocument.Load(pdfPath))
{
    int pageCount = document.PageCount;
    Console.WriteLine($"Total pages: {pageCount}");
    
    // PDFiumViewer does not have built-in text extraction
    // You would need to use OCR or another library
    // It can only render pages as images
    for (int i = 0; i < pageCount; i++)
    {
        var pageImage = document.Render(i, 96, 96, false);
        Console.WriteLine($"Rendered page {i + 1}");
    }
}