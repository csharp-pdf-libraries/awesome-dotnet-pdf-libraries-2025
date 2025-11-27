using PeachPDF;
using System.IO;

class Program
{
    static void Main()
    {
        var converter = new HtmlToPdfConverter();
        var url = "https://www.example.com";
        var pdf = converter.ConvertUrl(url);
        File.WriteAllBytes("webpage.pdf", pdf);
    }
}