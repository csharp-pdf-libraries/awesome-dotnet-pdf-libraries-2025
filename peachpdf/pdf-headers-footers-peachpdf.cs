using PeachPDF;
using System.IO;

class Program
{
    static void Main()
    {
        var converter = new HtmlToPdfConverter();
        converter.Header = "<div style='text-align:center'>My Header</div>";
        converter.Footer = "<div style='text-align:center'>Page {page}</div>";
        var html = "<html><body><h1>Document Content</h1></body></html>";
        var pdf = converter.Convert(html);
        File.WriteAllBytes("document.pdf", pdf);
    }
}