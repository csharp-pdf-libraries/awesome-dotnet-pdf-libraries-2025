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
        
        string html = "<html><body><h1>Hello World</h1></body></html>";
        byte[] pdfBytes = converter.Convert(new HtmlToPdfDocument
        {
            Objects = { new ObjectSettings { HtmlText = html } }
        });
        
        File.WriteAllBytes("output.pdf", pdfBytes);
    }
}