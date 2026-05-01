// NuGet: Install-Package Haukcode.WkHtmlToPdfDotNet (renamed from Haukcode.DinkToPdf in Oct 2024)
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
                PaperSize = PaperKind.Letter,
                Margins = new MarginSettings() { Top = 10, Bottom = 10, Left = 10, Right = 10 }
            },
            Objects = {
                new ObjectSettings() {
                    HtmlContent = "<html><body><h1>Landscape Document</h1><p>Custom page settings</p></body></html>",
                }
            }
        };
        
        byte[] pdf = converter.Convert(doc);
        File.WriteAllBytes("landscape.pdf", pdf);
    }
}