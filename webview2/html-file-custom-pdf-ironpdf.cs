// NuGet: Install-Package IronPdf
using IronPdf;
using IronPdf.Rendering;
using System;
using System.IO;

class Program
{
    static void Main()
    {
        var renderer = new ChromePdfRenderer();
        
        renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape;
        renderer.RenderingOptions.MarginTop = 50;
        renderer.RenderingOptions.MarginBottom = 50;
        
        string htmlFile = Path.Combine(Directory.GetCurrentDirectory(), "input.html");
        var pdf = renderer.RenderHtmlFileAsPdf(htmlFile);
        pdf.SaveAs("custom.pdf");
        
        Console.WriteLine("Custom PDF created");
    }
}