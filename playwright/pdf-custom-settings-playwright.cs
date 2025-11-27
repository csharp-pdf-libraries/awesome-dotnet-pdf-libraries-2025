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
        
        string html = "<h1>Custom PDF</h1><p>With margins and headers.</p>";
        await page.SetContentAsync(html);
        
        await page.PdfAsync(new PagePdfOptions
        {
            Path = "custom.pdf",
            Format = "A4",
            Margin = new Margin { Top = "1cm", Bottom = "1cm", Left = "1cm", Right = "1cm" },
            DisplayHeaderFooter = true,
            HeaderTemplate = "<div style='font-size:10px; text-align:center;'>Header</div>",
            FooterTemplate = "<div style='font-size:10px; text-align:center;'>Page <span class='pageNumber'></span></div>"
        });
        
        await browser.CloseAsync();
    }
}