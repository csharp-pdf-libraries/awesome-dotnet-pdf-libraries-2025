// NuGet: Install-Package Haukcode.WkHtmlToPdfDotNet
// Wraps the wkhtmltopdf native binary (project archived on GitHub Jan 2, 2023; last release 0.12.6, June 2020).
using Haukcode.WkHtmlToPdfDotNet;
using Haukcode.WkHtmlToPdfDotNet.Contracts;
using System.IO;

class Program
{
    static void Main()
    {
        var converter = new SynchronizedConverter(new PdfTools());
        var doc = new HtmlToPdfDocument()
        {
            GlobalSettings = {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Landscape,
                PaperSize = PaperKind.A4,
                Margins = new MarginSettings() { Top = 10, Bottom = 10, Left = 10, Right = 10 }
            },
            Objects = {
                new ObjectSettings()
                {
                    Page = "input.html",
                    WebSettings = { DefaultEncoding = "utf-8" }
                }
            }
        };
        byte[] pdf = converter.Convert(doc);
        File.WriteAllBytes("custom-output.pdf", pdf);
    }
}