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
        await page.SetContentAsync("<h1>Hello World</h1><p>This is a PDF document.</p>");
        await page.PdfAsync("output.pdf");
    }
}