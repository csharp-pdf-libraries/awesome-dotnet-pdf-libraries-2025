// ZetPDF is NOT on NuGet — install via SDK ZIP from https://zetpdf.com/download/.
// ZetPDF does NOT provide a native URL-to-PDF converter; HTML/URL rendering is not
// part of the documented feature list (https://zetpdf.com/net-pdf-sdk/). Practical
// migration paths usually pair ZetPDF with an external HTML renderer or move to a
// Chromium-based library like IronPDF (see url-to-pdf-ironpdf.cs).
using ZetPDF;
using System;

class Program
{
    static void Main()
    {
        // No native ConvertUrlToPdf exists in ZetPDF — illustrative call only.
        var converter = new HtmlToPdfConverter();
        var url = "https://www.example.com";
        converter.ConvertUrlToPdf(url, "webpage.pdf");
        Console.WriteLine("PDF from URL created successfully");
    }
}