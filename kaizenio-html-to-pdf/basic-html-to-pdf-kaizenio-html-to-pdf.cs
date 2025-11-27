using Kaizen.IO;
using System.IO;

class Program
{
    static void Main()
    {
        var converter = new HtmlToPdfConverter();
        var html = "<html><body><h1>Hello World</h1></body></html>";
        var pdfBytes = converter.Convert(html);
        File.WriteAllBytes("output.pdf", pdfBytes);
    }
}