// NuGet: Install-Package IronPdf
using IronPdf;
using System;
using System.Collections.Generic;

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
        
        var pdf = PdfDocument.Merge(pdfFiles);
        pdf.SaveAs("merged.pdf");
        
        Console.WriteLine("PDFs merged successfully");
    }
}