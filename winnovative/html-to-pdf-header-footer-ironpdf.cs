// NuGet: Install-Package IronPdf
using IronPdf;
using IronPdf.Rendering;
using System;

class Program
{
    static void Main()
    {
        // Create a PDF renderer
        var renderer = new ChromePdfRenderer();
        
        // Configure header and footer
        renderer.RenderingOptions.TextHeader = new TextHeaderFooter()
        {
            CenterText = "Document Header",
            FontSize = 12
        };
        
        renderer.RenderingOptions.TextFooter = new TextHeaderFooter()
        {
            CenterText = "Page {page} of {total-pages}",
            FontSize = 10
        };
        
        // Convert HTML to PDF
        string htmlString = "<html><body><h1>Document with Header and Footer</h1><p>Content goes here</p></body></html>";
        var pdf = renderer.RenderHtmlAsPdf(htmlString);
        
        // Save to file
        pdf.SaveAs("document.pdf");
        
        Console.WriteLine("PDF with header and footer created successfully");
    }
}