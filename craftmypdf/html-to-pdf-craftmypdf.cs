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
                html = "<h1>Hello World</h1><p>This is a PDF from HTML</p>"
            }
        });
        
        var response = client.Execute(request);
        File.WriteAllBytes("output.pdf", response.RawBytes);
    }
}