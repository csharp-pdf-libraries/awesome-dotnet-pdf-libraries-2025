// Kaizen.io HTML-to-PDF is a self-hosted Docker REST API — there is no
// official .NET SDK / NuGet package. Run the container first:
//   docker run -d --rm -p 8080:8080 -e KAIZEN_PDF_LICENSE=your_license \
//     --pull=always --name kaizen-pdf kaizenio.azurecr.io/html-to-pdf:latest
// Then POST JSON to http://localhost:8080/html-to-pdf — see https://www.kaizen.io/products/html-to-pdf/
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        using var http = new HttpClient();
        var payload = JsonSerializer.Serialize(new { html = "<html><body><h1>Hello World</h1></body></html>" });
        var content = new StringContent(payload, Encoding.UTF8, "application/json");
        var response = await http.PostAsync("http://localhost:8080/html-to-pdf", content);
        response.EnsureSuccessStatusCode();
        var pdfBytes = await response.Content.ReadAsByteArrayAsync();
        File.WriteAllBytes("output.pdf", pdfBytes);
    }
}
