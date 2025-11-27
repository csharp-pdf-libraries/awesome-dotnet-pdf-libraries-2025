// NuGet: Install-Package PDFBolt
using PDFBolt;
using System.IO;

class Program
{
    static void Main()
    {
        var converter = new HtmlToPdfConverter();
        var pdf = converter.ConvertUrl("https://www.example.com");
        File.WriteAllBytes("webpage.pdf", pdf);
    }
}