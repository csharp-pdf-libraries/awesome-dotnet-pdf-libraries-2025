// NuGet: Install-Package IronPdf
using IronPdf;
using System;

class Program
{
    static void Main()
    {
        // Create a PDF from HTML string
        var renderer = new ChromePdfRenderer();
        
        string htmlContent = "<h1>Hello World</h1><p>This is a PDF generated from HTML.</p>";
        
        var pdf = renderer.RenderHtmlAsPdf(htmlContent);
        pdf.SaveAs("output.pdf");
        
        Console.WriteLine("PDF created successfully!");
    }
}