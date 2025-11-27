// NuGet: Install-Package IronPdf
using IronPdf;
using IronPdf.Rendering;
using System;

class IronPdfUrlToPdf
{
    static void Main()
    {
        // Create a ChromePdfRenderer instance
        var renderer = new ChromePdfRenderer();
        
        // Configure rendering options with header and footer
        renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter()
        {
            HtmlFragment = "<div style='text-align:center'>Company Report</div>"
        };
        
        renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter()
        {
            HtmlFragment = "<div style='text-align:center'>Page {page} of {total-pages} - " + DateTime.Now.ToString("MM/dd/yyyy") + "</div>"
        };
        
        // Convert URL to PDF
        string url = "https://example.com";
        var pdf = renderer.RenderUrlAsPdf(url);
        
        // Save the PDF file
        pdf.SaveAs("webpage.pdf");
    }
}