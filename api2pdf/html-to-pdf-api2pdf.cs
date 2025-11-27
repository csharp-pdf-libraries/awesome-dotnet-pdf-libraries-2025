// NuGet: Install-Package Api2Pdf.DotNet
using System;
using System.Threading.Tasks;
using Api2Pdf.DotNet;

class Program
{
    static async Task Main(string[] args)
    {
        var a2pClient = new Api2PdfClient("your-api-key");
        var apiResponse = await a2pClient.HeadlessChrome.FromHtmlAsync("<h1>Hello World</h1>");
        Console.WriteLine(apiResponse.Pdf);
    }
}