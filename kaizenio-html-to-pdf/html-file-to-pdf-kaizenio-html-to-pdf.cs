using Kaizen.IO;
using System;
using System.IO;

class Program
{
    static void Main()
    {
        var converter = new HtmlToPdfConverter();
        var htmlContent = File.ReadAllText("input.html");
        var options = new ConversionOptions
        {
            PageSize = PageSize.A4,
            Orientation = Orientation.Portrait
        };
        var pdfBytes = converter.Convert(htmlContent, options);
        File.WriteAllBytes("document.pdf", pdfBytes);
    }
}