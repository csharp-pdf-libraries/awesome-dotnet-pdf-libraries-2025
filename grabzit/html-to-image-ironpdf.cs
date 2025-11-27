// NuGet: Install-Package IronPdf
using IronPdf;
using System;
using System.Drawing;

class Program
{
    static void Main()
    {
        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf("<html><body><h1>Hello World</h1></body></html>");
        var images = pdf.ToBitmap();
        images[0].Save("output.png", System.Drawing.Imaging.ImageFormat.Png);
    }
}