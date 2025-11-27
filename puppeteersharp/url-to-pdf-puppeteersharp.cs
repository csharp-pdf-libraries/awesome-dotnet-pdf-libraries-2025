// NuGet: Install-Package PuppeteerSharp
using PuppeteerSharp;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        var browserFetcher = new BrowserFetcher();
        await browserFetcher.DownloadAsync();
        
        await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
        {
            Headless = true
        });
        
        await using var page = await browser.NewPageAsync();
        await page.GoToAsync("https://www.example.com");
        await page.PdfAsync("webpage.pdf");
    }
}