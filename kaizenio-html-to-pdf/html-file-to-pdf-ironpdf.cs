// NuGet: Install-Package IronPdf
using IronPdf;
using System;
using System.IO;

class Program
{
    static void Main()
    {
        var renderer = new ChromePdfRenderer();
        renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
        renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Portrait;
        var pdf = renderer.RenderHtmlFileAsPdf("input.html");
        pdf.SaveAs("document.pdf");
    }
}