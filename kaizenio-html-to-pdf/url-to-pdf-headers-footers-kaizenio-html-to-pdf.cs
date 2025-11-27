using Kaizen.IO;
using System;
using System.IO;

class Program
{
    static void Main()
    {
        var converter = new HtmlToPdfConverter();
        var options = new ConversionOptions
        {
            Header = new HeaderOptions { HtmlContent = "<div style='text-align:center'>Company Header</div>" },
            Footer = new FooterOptions { HtmlContent = "<div style='text-align:center'>Page {page} of {total}</div>" },
            MarginTop = 20,
            MarginBottom = 20
        };
        var pdfBytes = converter.ConvertUrl("https://example.com", options);
        File.WriteAllBytes("webpage.pdf", pdfBytes);
    }
}