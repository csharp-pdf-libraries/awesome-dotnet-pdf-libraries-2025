// NuGet: Install-Package BitMiracle.Docotic.Pdf
// NuGet: Install-Package BitMiracle.Docotic.Pdf.HtmlToPdf  (separate add-on required)
using BitMiracle.Docotic.Pdf;
using System;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        // HtmlConverter is async-only and downloads Chromium on first use
        using var converter = await HtmlConverter.CreateAsync();

        string html = "<html><body><h1>Hello World</h1><p>This is HTML to PDF conversion.</p></body></html>";

        using var pdf = await converter.CreatePdfFromStringAsync(html);
        pdf.Save("output.pdf");

        Console.WriteLine("PDF created successfully");
    }
}
