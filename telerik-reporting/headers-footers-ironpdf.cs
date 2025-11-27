// NuGet: Install-Package IronPdf
using IronPdf;
using IronPdf.Rendering;
using System;

class IronPdfExample
{
    static void Main()
    {
        var renderer = new ChromePdfRenderer();
        
        // Configure header and footer
        renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter()
        {
            HtmlFragment = "<div style='text-align:center'>Document Header</div>"
        };
        
        renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter()
        {
            HtmlFragment = "<div style='text-align:center'>Page {page} of {total-pages}</div>"
        };
        
        var pdf = renderer.RenderHtmlAsPdf("<h1>Report Content</h1><p>This is the main content.</p>");
        pdf.SaveAs("report_with_headers.pdf");
    }
}