// NuGet: Install-Package WkHtmlToPdf-DotNet
using WkHtmlToPdfDotNet;
using WkHtmlToPdfDotNet.Contracts;
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