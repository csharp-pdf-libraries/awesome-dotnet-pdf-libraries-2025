// NuGet: Install-Package IronPdf
using System;
using IronPdf;

class Program
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderUrlAsPdf("https://www.example.com");
        pdf.SaveAs("output.pdf");
    }
}