// NuGet: Install-Package Api2Pdf
using System;
using System.Threading.Tasks;
using Api2Pdf;

class Program
{
    static async Task Main(string[] args)
    {
        var client = new Api2Pdf("your-api-key");
        var result = await client.Chrome.HtmlToPdfAsync(new ChromeHtmlToPdfRequest
        {
            Html = "<h1>Hello World</h1>"
        });

        if (result.Success)
            Console.WriteLine(result.FileUrl);
        else
            Console.WriteLine(result.Error);
    }
}
