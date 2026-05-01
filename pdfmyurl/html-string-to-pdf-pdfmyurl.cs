// PDFmyURL REST API — no NuGet SDK. Send the raw HTML in the `html` parameter.
// Docs: https://pdfmyurl.com/html-to-pdf-api
using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;

class Example
{
    static void Main()
    {
        string license = "your-license-key";
        string html = "<html><body><h1>Hello World</h1></body></html>";

        try
        {
            using (var client = new WebClient())
            {
                var values = new NameValueCollection
                {
                    { "license", license },
                    { "html", html }
                };
                // POST form-encoded; response body is the PDF binary
                byte[] pdfBytes = client.UploadValues("https://pdfmyurl.com/api", "POST", values);
                File.WriteAllBytes("output.pdf", pdfBytes);
            }
        }
        catch (WebException ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
    }
}
