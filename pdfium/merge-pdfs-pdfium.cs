// NuGet: Install-Package PdfiumViewer
using PdfiumViewer;
using System;
using System.IO;
using System.Collections.Generic;

// Note: PdfiumViewer does not have native PDF merging functionality
// You would need to use additional libraries or implement custom logic
class Program
{
    static void Main()
    {
        List<string> pdfFiles = new List<string> 
        { 
            "document1.pdf", 
            "document2.pdf", 
            "document3.pdf" 
        };
        
        // PdfiumViewer is primarily for rendering/viewing
        // PDF merging is not natively supported
        // You would need to use another library like iTextSharp or PdfSharp
        
        Console.WriteLine("PDF merging not natively supported in PdfiumViewer");
    }
}