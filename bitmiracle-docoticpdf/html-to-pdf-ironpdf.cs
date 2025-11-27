// NuGet: Install-Package IronPdf
using IronPdf;
using System;

class Program
{
    static void Main()
    {
        var renderer = new ChromePdfRenderer();
        string html = "<html><body><h1>Hello World</h1><p>This is HTML to PDF conversion.</p></body></html>";
        
        var pdf = renderer.RenderHtmlAsPdf(html);
        pdf.SaveAs("output.pdf");
        
        Console.WriteLine("PDF created successfully");
    }
}