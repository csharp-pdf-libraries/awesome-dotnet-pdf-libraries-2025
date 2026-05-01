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
        
        // Enable header
        pdfConverter.PdfHeaderOptions.ShowHeader = true;
        pdfConverter.PdfHeaderOptions.HeaderText = "Document Header";
        pdfConverter.PdfHeaderOptions.HeaderTextAlignment = HorizontalTextAlign.Center;
        
        // Enable footer with page numbers
        pdfConverter.PdfFooterOptions.ShowFooter = true;
        pdfConverter.PdfFooterOptions.FooterText = "Page &p; of &P;";
        pdfConverter.PdfFooterOptions.FooterTextAlignment = HorizontalTextAlign.Right;
        
        // Convert HTML file to PDF
        byte[] pdfBytes = pdfConverter.GetPdfBytesFromHtmlFile("input.html");
        
        // Save to file
        System.IO.File.WriteAllBytes("output-with-header-footer.pdf", pdfBytes);
        
        Console.WriteLine("PDF with headers and footers created successfully!");
    }
}