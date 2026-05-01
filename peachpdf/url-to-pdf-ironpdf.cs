// NuGet: Install-Package IronPdf
using IronPdf;
using System;

class Program
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var renderer = new ChromePdfRenderer();
        var url = "https://www.example.com";
        var pdf = renderer.RenderUrlAsPdf(url);
        pdf.SaveAs("webpage.pdf");
    }
}