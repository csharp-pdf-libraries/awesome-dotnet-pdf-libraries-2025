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
        options.PageSize = PageSize.A4;
        
        grabzIt.URLToPDF("https://www.example.com", options);
        grabzIt.SaveTo("webpage.pdf");
    }
}