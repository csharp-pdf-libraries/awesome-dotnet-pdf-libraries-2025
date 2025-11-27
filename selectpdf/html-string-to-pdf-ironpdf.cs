// NuGet: Install-Package IronPdf
using IronPdf;
using System;

class Program
{
    static void Main()
    {
        string htmlContent = "<html><body><h1>Hello World</h1><p>This is a PDF document.</p></body></html>";
        
        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf(htmlContent);
        pdf.SaveAs("document.pdf");
        
        Console.WriteLine("PDF generated from HTML string");
    }
}