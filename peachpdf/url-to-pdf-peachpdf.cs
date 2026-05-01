// NuGet: dotnet add package PeachPDF (targets .NET 8)
// Repo: https://github.com/jhaygood86/PeachPDF (BSD-3-Clause)
// PeachPDF has no ConvertUrl helper; URL fetching is wired via HttpClientNetworkAdapter.
using PeachPDF;
using PeachPDF.Network;
using PeachPDF.PdfSharpCore;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        var httpClient = new HttpClient();

        var pdfConfig = new PdfGenerateConfig
        {
            PageSize = PageSize.Letter,
            PageOrientation = PageOrientation.Portrait,
            NetworkAdapter = new HttpClientNetworkAdapter(httpClient, new Uri("https://www.example.com"))
        };

        var generator = new PdfGenerator();

        // Passing null tells PeachPDF to fetch HTML via the configured network adapter.
        var document = await generator.GeneratePdf(null, pdfConfig);

        using var stream = File.Create("webpage.pdf");
        document.Save(stream);
    }
}
