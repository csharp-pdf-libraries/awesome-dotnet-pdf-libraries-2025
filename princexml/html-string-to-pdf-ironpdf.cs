// NuGet: Install-Package IronPdf
using IronPdf;
using System;

class Program
{
    static void Main()
    {
        string html = "<html><head><style>body { font-family: Arial; color: blue; }</style></head><body><h1>Hello World</h1></body></html>";
        
        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf(html);
        pdf.SaveAs("styled-output.pdf");
        Console.WriteLine("Styled PDF created");
    }
}