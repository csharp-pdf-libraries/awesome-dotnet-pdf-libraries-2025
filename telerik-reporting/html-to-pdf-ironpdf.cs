// NuGet: Install-Package IronPdf
using IronPdf;
using System;

class IronPdfExample
{
    static void Main()
    {
        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf("<h1>Hello World</h1><p>Sample HTML content</p>");
        pdf.SaveAs("output.pdf");
    }
}