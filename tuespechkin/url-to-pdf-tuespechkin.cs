// NuGet: Install-Package TuesPechkin
// NuGet: Install-Package TuesPechkin.Wkhtmltox.Win64 (native binary, Windows x64)
using TuesPechkin;
using System.IO;

class Program
{
    static void Main()
    {
        var converter = new StandardConverter(
            new RemotingToolset<PdfToolset>(
                new Win64EmbeddedDeployment(
                    new TempFolderDeployment())));
        
        byte[] pdfBytes = converter.Convert(new HtmlToPdfDocument
        {
            Objects = {
                new ObjectSettings {
                    PageUrl = "https://www.example.com"
                }
            }
        });
        
        File.WriteAllBytes("webpage.pdf", pdfBytes);
    }
}