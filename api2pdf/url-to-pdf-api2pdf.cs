// NuGet: Install-Package Api2Pdf
using System;
using System.Threading.Tasks;
using Api2Pdf;

class Program
{
    static async Task Main(string[] args)
    {
        var client = new Api2Pdf("your-api-key");
        var result = await client.Chrome.UrlToPdfAsync(new ChromeUrlToPdfRequest
        {
            Url = "https://www.example.com"
        });

        if (result.Success)
            Console.WriteLine(result.FileUrl);
        else
            Console.WriteLine(result.Error);
    }
}
