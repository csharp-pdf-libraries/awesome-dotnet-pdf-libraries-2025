using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;

class GotenbergCustomSize
{
    static async Task Main()
    {
        var gotenbergUrl = "http://localhost:3000/forms/chromium/convert/html";
        
        using var client = new HttpClient();
        using var content = new MultipartFormDataContent();
        
        var html = "<html><body><h1>Custom Size PDF</h1></body></html>";
        content.Add(new StringContent(html), "files", "index.html");
        content.Add(new StringContent("8.5"), "paperWidth");
        content.Add(new StringContent("11"), "paperHeight");
        content.Add(new StringContent("0.5"), "marginTop");
        content.Add(new StringContent("0.5"), "marginBottom");
        
        var response = await client.PostAsync(gotenbergUrl, content);
        var pdfBytes = await response.Content.ReadAsByteArrayAsync();
        
        await File.WriteAllBytesAsync("custom-size.pdf", pdfBytes);
        Console.WriteLine("Custom size PDF generated successfully");
    }
}