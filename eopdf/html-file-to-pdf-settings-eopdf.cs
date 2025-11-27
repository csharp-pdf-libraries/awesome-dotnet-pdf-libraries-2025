// NuGet: Install-Package EO.Pdf
using EO.Pdf;
using System;

class Program
{
    static void Main()
    {
        HtmlToPdfOptions options = new HtmlToPdfOptions();
        options.PageSize = PdfPageSizes.A4;
        options.OutputArea = new RectangleF(0.5f, 0.5f, 7.5f, 10.5f);
        
        HtmlToPdf.ConvertUrl("file:///C:/input.html", "output.pdf", options);
        Console.WriteLine("PDF with custom settings created.");
    }
}