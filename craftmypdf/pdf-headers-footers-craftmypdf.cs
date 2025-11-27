// NuGet: Install-Package RestSharp
using System;
using RestSharp;
using System.IO;

class Program
{
    static void Main()
    {
        var client = new RestClient("https://api.craftmypdf.com/v1/create");
        var request = new RestRequest(Method.POST);
        request.AddHeader("X-API-KEY", "your-api-key");
        request.AddJsonBody(new
        {
            template_id = "your-template-id",
            data = new
            {
                html = "<h1>Document Content</h1>",
                header = "<div>Page Header</div>",
                footer = "<div>Page {page} of {total_pages}</div>"
            }
        });
        
        var response = client.Execute(request);
        File.WriteAllBytes("document.pdf", response.RawBytes);
    }
}