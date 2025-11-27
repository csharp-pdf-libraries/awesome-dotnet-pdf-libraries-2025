// NuGet: Install-Package IronPdf
using System;
using IronPdf;

class IronPdfExample
{
    static void Main()
    {
        var renderer = new ChromePdfRenderer();
        
        var html = "<html><body><h1>Hello from IronPDF</h1></body></html>";
        var pdf = renderer.RenderHtmlAsPdf(html);
        
        pdf.SaveAs("output.pdf");
        Console.WriteLine("PDF generated successfully");
    }
}