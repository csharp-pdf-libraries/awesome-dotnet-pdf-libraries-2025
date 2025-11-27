// NuGet: Install-Package PDFBolt
using PDFBolt;
using System.IO;

class Program
{
    static void Main()
    {
        var converter = new HtmlToPdfConverter();
        converter.PageSize = PageSize.A4;
        converter.MarginTop = 20;
        converter.MarginBottom = 20;
        var html = File.ReadAllText("input.html");
        var pdf = converter.ConvertHtmlString(html);
        File.WriteAllBytes("output.pdf", pdf);
    }
}