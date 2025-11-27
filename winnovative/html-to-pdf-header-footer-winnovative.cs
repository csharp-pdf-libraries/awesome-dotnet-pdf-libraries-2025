// NuGet: Install-Package Winnovative.WebToPdfConverter
using Winnovative;
using System;
using System.Drawing;

class Program
{
    static void Main()
    {
        // Create the HTML to PDF converter
        HtmlToPdfConverter htmlToPdfConverter = new HtmlToPdfConverter();
        
        // Set license key
        htmlToPdfConverter.LicenseKey = "your-license-key";
        
        // Enable header
        htmlToPdfConverter.PdfDocumentOptions.ShowHeader = true;
        htmlToPdfConverter.PdfHeaderOptions.HeaderHeight = 60;
        
        // Add header text
        TextElement headerText = new TextElement(0, 0, "Document Header", new Font("Arial", 12));
        htmlToPdfConverter.PdfHeaderOptions.AddElement(headerText);
        
        // Enable footer
        htmlToPdfConverter.PdfDocumentOptions.ShowFooter = true;
        htmlToPdfConverter.PdfFooterOptions.FooterHeight = 60;
        
        // Add footer with page number
        TextElement footerText = new TextElement(0, 0, "Page &p; of &P;", new Font("Arial", 10));
        htmlToPdfConverter.PdfFooterOptions.AddElement(footerText);
        
        // Convert HTML to PDF
        string htmlString = "<html><body><h1>Document with Header and Footer</h1><p>Content goes here</p></body></html>";
        byte[] pdfBytes = htmlToPdfConverter.ConvertHtml(htmlString, "");
        
        // Save to file
        System.IO.File.WriteAllBytes("document.pdf", pdfBytes);
        
        Console.WriteLine("PDF with header and footer created successfully");
    }
}