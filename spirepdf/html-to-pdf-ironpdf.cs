// NuGet: Install-Package IronPdf
using IronPdf;
using System;

class Program
{
    static void Main()
    {
        var renderer = new ChromePdfRenderer();
        
        string htmlString = "<html><body><h1>Hello World</h1><p>This is a PDF from HTML.</p></body></html>";
        
        var pdf = renderer.RenderHtmlAsPdf(htmlString);
        pdf.SaveAs("output.pdf");
    }
}