// NuGet: Install-Package itext7
using iText.Html2pdf;
using System.IO;

class Program
{
    static void Main()
    {
        string html = "<h1>Hello World</h1><p>This is a PDF from HTML.</p>";
        string outputPath = "output.pdf";
        
        using (FileStream fs = new FileStream(outputPath, FileMode.Create))
        {
            HtmlConverter.ConvertToPdf(html, fs);
        }
    }
}