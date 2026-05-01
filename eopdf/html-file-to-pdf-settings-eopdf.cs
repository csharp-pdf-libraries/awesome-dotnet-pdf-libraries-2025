// NuGet: Install-Package EO.Pdf
using EO.Pdf;
using System;
using System.Drawing;

class Program
{
    static void Main()
    {
        HtmlToPdfOptions options = new HtmlToPdfOptions();
        options.PageSize = PdfPageSizes.A4;
        // OutputArea is in inches: x, y, width, height inside the page.
        options.OutputArea = new RectangleF(0.5f, 0.5f, 7.5f, 10.5f);

        HtmlToPdf.ConvertUrl("file:///C:/input.html", "output.pdf", options);
        Console.WriteLine("PDF with custom settings created.");
    }
}