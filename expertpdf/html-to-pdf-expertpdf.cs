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
        
        // Convert HTML string to PDF
        byte[] pdfBytes = pdfConverter.GetPdfBytesFromHtmlString("<h1>Hello World</h1><p>This is a PDF document.</p>");
        
        // Save to file
        System.IO.File.WriteAllBytes("output.pdf", pdfBytes);
        
        Console.WriteLine("PDF created successfully!");
    }
}