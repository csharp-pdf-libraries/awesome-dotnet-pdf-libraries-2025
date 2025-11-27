// NuGet: Install-Package PdfForge
using PdfForge;
using System.IO;

class Program
{
    static void Main()
    {
        var converter = new HtmlToPdfConverter();
        var pdf = converter.ConvertUrl("https://example.com");
        File.WriteAllBytes("webpage.pdf", pdf);
    }
}