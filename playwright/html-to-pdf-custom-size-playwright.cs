// NuGet: Install-Package Microsoft.Playwright
using Microsoft.Playwright;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync();
        var page = await browser.NewPageAsync();
        await page.SetContentAsync("<h1>Custom PDF</h1><p>Letter size with margins</p>");
        await page.PdfAsync(new PagePdfOptions 
        { 
            Path = "custom.pdf",
            Format = "Letter",
            Margin = new Margin { Top = "1in", Bottom = "1in", Left = "0.5in", Right = "0.5in" }
        });
    }
}