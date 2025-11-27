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
        
        await page.GotoAsync("https://www.example.com");
        await page.PdfAsync(new PagePdfOptions 
        { 
            Path = "webpage.pdf",
            Format = "A4"
        });
        
        await browser.CloseAsync();
    }
}