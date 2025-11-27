// NuGet: Install-Package GrabzIt
using GrabzIt;
using GrabzIt.Parameters;
using System;

class Program
{
    static void Main()
    {
        var grabzIt = new GrabzItClient("YOUR_APPLICATION_KEY", "YOUR_APPLICATION_SECRET");
        var options = new PDFOptions();
        options.CustomId = "my-pdf";
        
        grabzIt.HTMLToPDF("<html><body><h1>Hello World</h1></body></html>", options);
        grabzIt.SaveTo("output.pdf");
    }
}