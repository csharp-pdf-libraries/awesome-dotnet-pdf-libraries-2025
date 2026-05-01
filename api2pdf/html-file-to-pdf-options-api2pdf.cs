// NuGet: Install-Package Api2Pdf
using System;
using System.IO;
using System.Threading.Tasks;
using Api2Pdf;

class Program
{
    static async Task Main(string[] args)
    {
        var client = new Api2Pdf("your-api-key");
        string html = File.ReadAllText("input.html");
        var result = await client.Chrome.HtmlToPdfAsync(new ChromeHtmlToPdfRequest
        {
            Html = html,
            Options = new ChromeHtmlToPdfOptions
            {
                Landscape = true,
                PrintBackground = true
            }
        });

        if (result.Success)
            Console.WriteLine(result.FileUrl);
        else
            Console.WriteLine(result.Error);
    }
}
