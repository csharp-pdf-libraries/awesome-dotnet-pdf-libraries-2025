// NuGet: Install-Package IronPdf
using IronPdf;
using System;

class IronPdfExample
{
    static void Main()
    {
        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderUrlAsPdf("https://example.com");
        pdf.SaveAs("webpage.pdf");
    }
}