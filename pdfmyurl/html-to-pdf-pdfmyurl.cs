// PDFmyURL is a REST API SaaS — no NuGet package. Two integration options:
//   (1) Plain HttpClient / WebClient against https://pdfmyurl.com/api  (shown below)
//   (2) Official .NET DLL component PDFmyURL.NET.dll (PDFmyURLdotNET namespace),
//       downloaded from https://pdfmyurl.com/html-to-pdf-api-net-component
// Docs: https://pdfmyurl.com/html-to-pdf-api
using System;
using System.Net;

class Example
{
    static void Main()
    {
        string license = "your-license-key";
        string url = "https://example.com";

        try
        {
            using (var client = new WebClient())
            {
                client.QueryString.Add("license", license);
                client.QueryString.Add("url", url);
                // GET or POST against the single endpoint; PDF binary is returned in the response
                client.DownloadFile("https://pdfmyurl.com/api", "output.pdf");
            }
        }
        catch (WebException ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
    }
}
