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
                Orientation = Orientation.Portrait,
                PaperSize = PaperKind.A4
            },
            Objects = {
                new ObjectSettings()
                {
                    HtmlContent = "<h1>Hello World</h1><p>This is a PDF from HTML.</p>"
                }
            }
        };
        byte[] pdf = converter.Convert(doc);
        File.WriteAllBytes("output.pdf", pdf);
    }
}