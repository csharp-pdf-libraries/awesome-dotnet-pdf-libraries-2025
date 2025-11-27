using PeachPDF;
using System.IO;

class Program
{
    static void Main()
    {
        var converter = new HtmlToPdfConverter();
        var html = "<html><body><h1>Hello World</h1></body></html>";
        var pdf = converter.Convert(html);
        File.WriteAllBytes("output.pdf", pdf);
    }
}