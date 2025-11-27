// NuGet: Install-Package Scryber.Core
using Scryber.Core;
using Scryber.Core.Html;
using System.Net.Http;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        using var client = new HttpClient();
        string html = await client.GetStringAsync("https://www.example.com");
        
        using (var doc = Document.ParseDocument(html, ParseSourceType.DynamicContent))
        {
            doc.SaveAsPDF("webpage.pdf");
        }
    }
}