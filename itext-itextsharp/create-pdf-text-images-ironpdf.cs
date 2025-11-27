// NuGet: Install-Package IronPdf
using IronPdf;

class Program
{
    static void Main()
    {
        var renderer = new ChromePdfRenderer();
        
        string html = @"
            <h1>Sample PDF Document</h1>
            <p>This document contains text and an image.</p>
            <img src='image.jpg' width='200' />";
        
        var pdf = renderer.RenderHtmlAsPdf(html);
        pdf.SaveAs("document.pdf");
    }
}