// NuGet: Install-Package IronPdf
using IronPdf;
using System;

class IronPdfHtmlToPdf
{
    static void Main()
    {
        // Create a ChromePdfRenderer instance
        var renderer = new ChromePdfRenderer();
        
        // Convert HTML string to PDF
        var htmlContent = "<h1>Hello World</h1><p>This is HTML content.</p>";
        var pdf = renderer.RenderHtmlAsPdf(htmlContent);
        
        // Save the PDF file
        pdf.SaveAs("output.pdf");
    }
}