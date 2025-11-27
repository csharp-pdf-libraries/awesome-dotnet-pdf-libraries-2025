using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;

class GotenbergUrlToPdf
{
    static async Task Main()
    {
        var gotenbergUrl = "http://localhost:3000/forms/chromium/convert/url";
        
        using var client = new HttpClient();
        using var content = new MultipartFormDataContent();
        
        content.Add(new StringContent("https://example.com"), "url");
        
        var response = await client.PostAsync(gotenbergUrl, content);
        var pdfBytes = await response.Content.ReadAsByteArrayAsync();
        
        await File.WriteAllBytesAsync("webpage.pdf", pdfBytes);
        Console.WriteLine("PDF from URL generated successfully");
    }
}