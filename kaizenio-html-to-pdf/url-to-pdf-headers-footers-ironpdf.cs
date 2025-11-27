// NuGet: Install-Package IronPdf
using IronPdf;
using System;
using System.IO;

class Program
{
    static void Main()
    {
        var renderer = new ChromePdfRenderer();
        renderer.RenderingOptions.TextHeader.CenterText = "Company Header";
        renderer.RenderingOptions.TextFooter.CenterText = "Page {page} of {total-pages}";
        renderer.RenderingOptions.MarginTop = 20;
        renderer.RenderingOptions.MarginBottom = 20;
        var pdf = renderer.RenderUrlAsPdf("https://example.com");
        pdf.SaveAs("webpage.pdf");
    }
}