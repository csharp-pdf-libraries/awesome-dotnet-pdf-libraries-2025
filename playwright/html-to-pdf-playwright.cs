// NuGet: Install-Package Microsoft.Playwright
using Microsoft.Playwright;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        var playwright = await Playwright.CreateAsync();
        var browser = await playwright.Chromium.LaunchAsync();
        var page = await browser.NewPageAsync();
        
        string html = "<h1>Hello World</h1><p>This is a test PDF.</p>";
        await page.SetContentAsync(html);
        await page.PdfAsync(new PagePdfOptions { Path = "output.pdf" });
        
        await browser.CloseAsync();
    }
}