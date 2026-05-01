// NuGet: Install-Package IronPdf
using System;
using IronPdf;

class Program
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf("<h1>Hello World</h1><p>This is a PDF from HTML</p>");
        pdf.SaveAs("output.pdf");
    }
}