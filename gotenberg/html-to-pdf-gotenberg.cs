using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;

class GotenbergExample
{
    static async Task Main()
    {
        var gotenbergUrl = "http://localhost:3000/forms/chromium/convert/html";
        
        using var client = new HttpClient();
        using var content = new MultipartFormDataContent();
        
        var html = "<html><body><h1>Hello from Gotenberg</h1></body></html>";
        content.Add(new StringContent(html), "files", "index.html");
        
        var response = await client.PostAsync(gotenbergUrl, content);
        var pdfBytes = await response.Content.ReadAsByteArrayAsync();
        
        await File.WriteAllBytesAsync("output.pdf", pdfBytes);
        Console.WriteLine("PDF generated successfully");
    }
}