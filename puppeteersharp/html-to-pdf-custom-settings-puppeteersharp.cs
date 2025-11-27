// NuGet: Install-Package PuppeteerSharp
using PuppeteerSharp;
using PuppeteerSharp.Media;
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
        await page.SetContentAsync("<h1>Custom PDF</h1><p>With landscape orientation and margins.</p>");
        
        await page.PdfAsync("custom.pdf", new PdfOptions
        {
            Format = PaperFormat.A4,
            Landscape = true,
            MarginOptions = new MarginOptions
            {
                Top = "20mm",
                Bottom = "20mm",
                Left = "20mm",
                Right = "20mm"
            }
        });
    }
}