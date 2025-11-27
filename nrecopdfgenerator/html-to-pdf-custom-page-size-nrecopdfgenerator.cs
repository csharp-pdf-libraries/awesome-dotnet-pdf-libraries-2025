// NuGet: Install-Package NReco.PdfGenerator
using NReco.PdfGenerator;
using System.IO;

class Program
{
    static void Main()
    {
        var htmlToPdf = new HtmlToPdfConverter();
        htmlToPdf.PageWidth = 210;
        htmlToPdf.PageHeight = 297;
        htmlToPdf.Margins = new PageMargins { Top = 10, Bottom = 10, Left = 10, Right = 10 };
        var htmlContent = "<html><body><h1>Custom Page Size</h1><p>A4 size document with margins.</p></body></html>";
        var pdfBytes = htmlToPdf.GeneratePdf(htmlContent);
        File.WriteAllBytes("custom-size.pdf", pdfBytes);
    }
}