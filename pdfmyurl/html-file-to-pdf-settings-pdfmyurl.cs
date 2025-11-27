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
            client.setPageSize("A4");
            client.setOrientation("landscape");
            client.setMarginTop("10mm");
            client.convertFileToFile("input.html", "output.pdf");
        }
        catch(Error why)
        {
            Console.WriteLine("Error: " + why);
        }
    }
}