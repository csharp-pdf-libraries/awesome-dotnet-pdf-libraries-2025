// NuGet: Install-Package IronPdf
using IronPdf;
using System;

class Program
{
    static void Main()
    {
        // IronPDF has native HTML to PDF rendering
        var renderer = new ChromePdfRenderer();
        
        string html = "<h1>Hello from IronPDF</h1><p>Easy HTML to PDF conversion</p>";
        var pdf = renderer.RenderHtmlAsPdf(html);
        
        pdf.SaveAs("output.pdf");
    }
}