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
        
        // Convert URL to PDF
        string url = "https://www.example.com";
        byte[] pdfBytes = htmlToPdfConverter.ConvertUrl(url);
        
        // Save to file
        System.IO.File.WriteAllBytes("webpage.pdf", pdfBytes);
        
        Console.WriteLine("PDF from URL created successfully");
    }
}