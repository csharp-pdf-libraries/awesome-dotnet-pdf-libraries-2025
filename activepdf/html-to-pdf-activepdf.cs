// HTML-to-PDF in ActivePDF is handled by the WebGrabber product, not Toolkit.
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

        string htmlContent = "<html><body><h1>Hello World</h1></body></html>";

        // WebGrabber renders from a URL or HTML file path; write the HTML to a temp file first.
        string tempHtml = Path.Combine(Path.GetTempPath(), "input.html");
        File.WriteAllText(tempHtml, htmlContent);

        wg.URL = tempHtml;
        wg.OutputDirectory = Directory.GetCurrentDirectory();
        wg.OutputFilename = "output.pdf";

        // ConvertToPDF returns 0 on success.
        if (wg.ConvertToPDF() == 0)
        {
            Console.WriteLine("PDF created successfully");
        }
    }
}