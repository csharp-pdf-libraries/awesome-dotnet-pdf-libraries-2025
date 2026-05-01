// NuGet: Install-Package IronPdf
using IronPdf;
using System;

class Program
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlFileAsPdf("input.html");
        pdf.SaveAs("output.pdf");
        Console.WriteLine("PDF created successfully");
    }
}