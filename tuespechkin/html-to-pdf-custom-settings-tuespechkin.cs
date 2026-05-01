// NuGet: Install-Package TuesPechkin
// NuGet: Install-Package TuesPechkin.Wkhtmltox.Win64 (native binary, Windows x64)
using TuesPechkin;
using System.Drawing.Printing;
using System.IO;

class Program
{
    static void Main()
    {
        var converter = new StandardConverter(
            new RemotingToolset<PdfToolset>(
                new Win64EmbeddedDeployment(
                    new TempFolderDeployment())));

        string html = "<html><body><h1>Custom PDF</h1></body></html>";

        var document = new HtmlToPdfDocument
        {
            GlobalSettings = {
                Orientation = GlobalSettings.PaperOrientation.Landscape,
                PaperSize = PaperKind.A4,
                Margins = new MarginSettings { Unit = Unit.Millimeters, Top = 10, Bottom = 10 }
            },
            Objects = {
                new ObjectSettings { HtmlText = html }
            }
        };

        byte[] pdfBytes = converter.Convert(document);
        File.WriteAllBytes("custom.pdf", pdfBytes);
    }
}