// NuGet: Install-Package IronPdf
using IronPdf;
using System;

class Program
{
    static void Main()
    {
        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderUrlAsPdf("https://example.com");
        pdf.SaveAs("output.pdf");
        
        // Convert HTML string
        string html = "<h1>Hello World</h1><p>This is a PDF document.</p>";
        var pdfFromHtml = renderer.RenderHtmlAsPdf(html);
        pdfFromHtml.SaveAs("fromhtml.pdf");
    }
}