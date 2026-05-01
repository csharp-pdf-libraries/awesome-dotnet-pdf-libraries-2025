// NuGet: Install-Package ExpertPdf.HtmlToPdf.NetCore   (modern .NET / .NET Core, .NET 5-9)
//        Install-Package ExpertPdfHtmlToPdf            (legacy .NET Framework)
// Vendor: Outside Software Inc. — https://www.html-to-pdf.net/
using ExpertPdf.HtmlToPdf;
using System;

class Program
{
    static void Main()
    {
        // Create the PDF converter
        PdfConverter pdfConverter = new PdfConverter();
        
        // Set page size and orientation
        pdfConverter.PdfDocumentOptions.PdfPageSize = PdfPageSize.A4;
        pdfConverter.PdfDocumentOptions.PdfPageOrientation = PdfPageOrientation.Portrait;
        
        // Convert URL to PDF
        byte[] pdfBytes = pdfConverter.GetPdfBytesFromUrl("https://www.example.com");
        
        // Save to file
        System.IO.File.WriteAllBytes("webpage.pdf", pdfBytes);
        
        Console.WriteLine("PDF from URL created successfully!");
    }
}