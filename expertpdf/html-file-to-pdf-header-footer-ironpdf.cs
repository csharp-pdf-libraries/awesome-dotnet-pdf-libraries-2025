// NuGet: Install-Package IronPdf
using IronPdf;
using System;

class Program
{
    static void Main()
    {
        // Create a PDF renderer
        var renderer = new ChromePdfRenderer();
        
        // Configure header
        renderer.RenderingOptions.TextHeader = new TextHeaderFooter()
        {
            CenterText = "Document Header",
            DrawDividerLine = true
        };
        
        // Configure footer with page numbers
        renderer.RenderingOptions.TextFooter = new TextHeaderFooter()
        {
            RightText = "Page {page} of {total-pages}",
            DrawDividerLine = true
        };
        
        // Convert HTML file to PDF
        var pdf = renderer.RenderHtmlFileAsPdf("input.html");
        
        // Save to file
        pdf.SaveAs("output-with-header-footer.pdf");
        
        Console.WriteLine("PDF with headers and footers created successfully!");
    }
}