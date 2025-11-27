// NuGet: Install-Package Winnovative.WebToPdfConverter
using Winnovative;
using System;

class Program
{
    static void Main()
    {
        // Create the HTML to PDF converter
        HtmlToPdfConverter htmlToPdfConverter = new HtmlToPdfConverter();
        
        // Set license key
        htmlToPdfConverter.LicenseKey = "your-license-key";
        
        // Convert HTML string to PDF
        string htmlString = "<html><body><h1>Hello World</h1></body></html>";
        byte[] pdfBytes = htmlToPdfConverter.ConvertHtml(htmlString, "");
        
        // Save to file
        System.IO.File.WriteAllBytes("output.pdf", pdfBytes);
        
        Console.WriteLine("PDF created successfully");
    }
}