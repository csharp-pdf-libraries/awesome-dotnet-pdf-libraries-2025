// Install PDFmyURL SDK
using System;
using Pdfcrowd;

class Example
{
    static void Main()
    {
        try
        {
            var client = new HtmlToPdfClient("username", "apikey");
            client.convertUrlToFile("https://example.com", "output.pdf");
        }
        catch(Error why)
        {
            Console.WriteLine("Error: " + why);
        }
    }
}