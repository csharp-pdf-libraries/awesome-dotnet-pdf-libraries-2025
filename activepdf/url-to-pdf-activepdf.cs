// URL-to-PDF in ActivePDF is handled by the WebGrabber product, not Toolkit.
// NuGet: Install-Package ActivePDF.WebGrabber
// Docs: https://documentation.activepdf.com/webgrabber_api/
using APWebGrabber;
using System;
using System.IO;

class Program
{
    static void Main()
    {
        WebGrabber wg = new WebGrabber();

        wg.URL = "https://www.example.com";
        wg.OutputDirectory = Directory.GetCurrentDirectory();
        wg.OutputFilename = "webpage.pdf";

        // ConvertToPDF returns 0 on success.
        if (wg.ConvertToPDF() == 0)
        {
            Console.WriteLine("PDF from URL created successfully");
        }
    }
}