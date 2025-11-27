// NuGet: Install-Package IronPdf
using IronPdf;
using System;

class IronPdfHtmlToPdf
{
    static void Main()
    {
        var renderer = new ChromePdfRenderer();
        string htmlContent = "<html><body><h1>Hello World</h1></body></html>";
        
        // Convert HTML to PDF with simple API
        var pdf = renderer.RenderHtmlAsPdf(htmlContent);
        pdf.SaveAs("output.pdf");
    }
}