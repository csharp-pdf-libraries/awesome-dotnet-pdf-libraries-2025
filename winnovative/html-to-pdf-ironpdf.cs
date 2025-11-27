// NuGet: Install-Package IronPdf
using IronPdf;
using System;

class Program
{
    static void Main()
    {
        // Create a PDF renderer
        var renderer = new ChromePdfRenderer();
        
        // Convert HTML string to PDF
        string htmlString = "<html><body><h1>Hello World</h1></body></html>";
        var pdf = renderer.RenderHtmlAsPdf(htmlString);
        
        // Save to file
        pdf.SaveAs("output.pdf");
        
        Console.WriteLine("PDF created successfully");
    }
}