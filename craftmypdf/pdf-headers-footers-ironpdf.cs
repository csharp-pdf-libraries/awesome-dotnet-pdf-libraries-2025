// NuGet: Install-Package IronPdf
using System;
using IronPdf;

class Program
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var renderer = new ChromePdfRenderer();
        renderer.RenderingOptions.TextHeader = new TextHeaderFooter()
        {
            CenterText = "Page Header"
        };
        renderer.RenderingOptions.TextFooter = new TextHeaderFooter()
        {
            CenterText = "Page {page} of {total-pages}"
        };
        
        var pdf = renderer.RenderHtmlAsPdf("<h1>Document Content</h1>");
        pdf.SaveAs("document.pdf");
    }
}