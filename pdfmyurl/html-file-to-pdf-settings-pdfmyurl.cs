// PDFmyURL REST API — no NuGet SDK. Page settings are sent as query/form parameters.
// Parameter reference: https://pdfmyurl.com/html-to-pdf-api
using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;

class Example
{
    static void Main()
    {
        string license = "your-license-key";
        string html = File.ReadAllText("input.html");

        try
        {
            using (var client = new WebClient())
            {
                var values = new NameValueCollection
                {
                    { "license",      license },
                    { "html",         html },
                    { "page_size",    "A4" },
                    { "orientation",  "landscape" },
                    { "top",          "10" },
                    { "unit",         "mm" }
                };
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
