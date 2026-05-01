// NuGet: Install-Package IronPdf
using IronPdf;
using System;

class Program
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
        var renderer = new ChromePdfRenderer();
        renderer.RenderingOptions.EnableJavaScript = true;
        renderer.RenderingOptions.Title = "Website Export";

        var pdf = renderer.RenderUrlAsPdf("https://example.com");
        // Apply password encryption via SecuritySettings
        pdf.SecuritySettings.OwnerPassword = "owner-password";
        pdf.SecuritySettings.UserPassword = "user-password";
        pdf.SaveAs("webpage.pdf");
        Console.WriteLine("URL converted to PDF");
    }
}