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
                Orientation = Orientation.Portrait,
                PaperSize = PaperKind.A4,
            },
            Objects = {
                new ObjectSettings() {
                    Page = "https://www.example.com",
                }
            }
        };
        
        byte[] pdf = converter.Convert(doc);
        File.WriteAllBytes("webpage.pdf", pdf);
    }
}