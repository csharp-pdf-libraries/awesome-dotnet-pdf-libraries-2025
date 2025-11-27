// NuGet: Install-Package IronPdf
using IronPdf;
using IronPdf.Rendering;
using System;

class Program
{
    static void Main()
    {
        var renderer = new ChromePdfRenderer();
        
        renderer.RenderingOptions.TextHeader = new TextHeaderFooter()
        {
            CenterText = "Header Text"
        };
        
        renderer.RenderingOptions.TextFooter = new TextHeaderFooter()
        {
            CenterText = "Page {page}"
        };
        
        string html = "<html><body><h1>Document with Headers</h1><p>Content here</p></body></html>";
        
        var pdf = renderer.RenderHtmlAsPdf(html);
        
        pdf.SaveAs("document.pdf");
    }
}