// NuGet: dotnet add package PeachPDF (targets .NET 8)
// Repo: https://github.com/jhaygood86/PeachPDF (BSD-3-Clause)
using PeachPDF;
using PeachPDF.PdfSharpCore;
using System.IO;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        var html = "<html><body><h1>Hello World</h1></body></html>";

        var pdfConfig = new PdfGenerateConfig
        {
            PageSize = PageSize.Letter,
            PageOrientation = PageOrientation.Portrait
        };

        var generator = new PdfGenerator();
        var document = await generator.GeneratePdf(html, pdfConfig);

        using var stream = File.Create("output.pdf");
        document.Save(stream);
    }
}
