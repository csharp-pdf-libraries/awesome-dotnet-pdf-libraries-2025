// NuGet: Install-Package PDFPrinting.NET
using PDFPrinting.NET;
using System;

class Program
{
    static void Main()
    {
        var converter = new HtmlToPdfConverter();
        converter.HeaderText = "Company Report";
        converter.FooterText = "Page {page} of {total}";
        string html = "<html><body><h1>Document Content</h1></body></html>";
        converter.ConvertHtmlToPdf(html, "report.pdf");
        Console.WriteLine("PDF with headers/footers created");
    }
}