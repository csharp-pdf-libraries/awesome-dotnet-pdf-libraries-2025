// NuGet: Install-Package TuesPechkin
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