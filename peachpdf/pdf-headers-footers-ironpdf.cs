// NuGet: Install-Package IronPdf
using IronPdf;
using IronPdf.Rendering;
using System;

class Program
{
    static void Main()
    {
        var renderer = new ChromePdfRenderer();
        renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter() { HtmlFragment = "<div style='text-align:center'>My Header</div>" };
        renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter() { HtmlFragment = "<div style='text-align:center'>Page {page}</div>" };
        var html = "<html><body><h1>Document Content</h1></body></html>";
        var pdf = renderer.RenderHtmlAsPdf(html);
        pdf.SaveAs("document.pdf");
    }
}