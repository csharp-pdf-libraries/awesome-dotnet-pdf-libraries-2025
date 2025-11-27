// NuGet: Install-Package HiQPdf
using HiQPdf;
using System;

class Program
{
    static void Main()
    {
        HtmlToPdf htmlToPdfConverter = new HtmlToPdf();
        byte[] pdfBuffer = htmlToPdfConverter.ConvertUrlToMemory("https://example.com");
        System.IO.File.WriteAllBytes("output.pdf", pdfBuffer);
        
        // Convert HTML string
        string html = "<h1>Hello World</h1><p>This is a PDF document.</p>";
        byte[] pdfFromHtml = htmlToPdfConverter.ConvertHtmlToMemory(html, "");
        System.IO.File.WriteAllBytes("fromhtml.pdf", pdfFromHtml);
    }
}