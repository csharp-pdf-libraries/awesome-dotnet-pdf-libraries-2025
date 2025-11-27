// NuGet: Install-Package PdfForge
using PdfForge;
using System.IO;

class Program
{
    static void Main()
    {
        var converter = new HtmlToPdfConverter();
        converter.PageSize = PageSize.A4;
        converter.Orientation = PageOrientation.Landscape;
        var htmlContent = File.ReadAllText("input.html");
        var pdf = converter.ConvertHtmlString(htmlContent);
        File.WriteAllBytes("output.pdf", pdf);
    }
}