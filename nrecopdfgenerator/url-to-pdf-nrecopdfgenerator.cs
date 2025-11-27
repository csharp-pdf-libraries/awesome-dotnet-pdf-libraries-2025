// NuGet: Install-Package NReco.PdfGenerator
using NReco.PdfGenerator;
using System.IO;

class Program
{
    static void Main()
    {
        var htmlToPdf = new HtmlToPdfConverter();
        var pdfBytes = htmlToPdf.GeneratePdfFromFile("https://www.example.com", null);
        File.WriteAllBytes("webpage.pdf", pdfBytes);
    }
}