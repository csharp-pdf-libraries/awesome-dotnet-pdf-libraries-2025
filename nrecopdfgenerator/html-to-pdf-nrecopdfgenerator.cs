// NuGet: Install-Package NReco.PdfGenerator
using NReco.PdfGenerator;
using System.IO;

class Program
{
    static void Main()
    {
        var htmlToPdf = new HtmlToPdfConverter();
        var htmlContent = "<html><body><h1>Hello World</h1><p>This is a PDF document.</p></body></html>";
        var pdfBytes = htmlToPdf.GeneratePdf(htmlContent);
        File.WriteAllBytes("output.pdf", pdfBytes);
    }
}