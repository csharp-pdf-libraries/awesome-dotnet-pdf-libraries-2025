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
            string html = "<html><body><h1>Hello World</h1></body></html>";
            client.convertStringToFile(html, "output.pdf");
        }
        catch(Error why)
        {
            Console.WriteLine("Error: " + why);
        }
    }
}