// NuGet: Install-Package Scryber.Core
// Scryber has no native URL-to-PDF method — fetch the HTML yourself, then parse.
// Note: Scryber requires valid XHTML; many live web pages will need cleanup
// (e.g., HtmlAgilityPack) before they can be parsed.
using Scryber.Components;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        using var client = new HttpClient();
        string html = await client.GetStringAsync("https://www.example.com");

        using (var reader = new StringReader(html))
        using (var doc = Document.ParseDocument(reader, string.Empty, ParseSourceType.DynamicContent))
        using (var stream = new FileStream("webpage.pdf", FileMode.Create))
        {
            doc.SaveAsPDF(stream);
        }
    }
}
