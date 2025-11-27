// NuGet: Install-Package Select.HtmlToPdf
using SelectPdf;
using System;

class Program
{
    static void Main()
    {
        HtmlToPdf converter = new HtmlToPdf();
        PdfDocument doc = converter.ConvertUrl("https://www.example.com");
        doc.Save("output.pdf");
        doc.Close();
        
        Console.WriteLine("PDF created successfully");
    }
}