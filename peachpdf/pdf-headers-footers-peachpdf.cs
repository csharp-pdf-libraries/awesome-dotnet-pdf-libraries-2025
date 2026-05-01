// NuGet: dotnet add package PeachPDF (targets .NET 8)
// Repo: https://github.com/jhaygood86/PeachPDF (BSD-3-Clause)
//
// PeachPDF (v0.7.x) does NOT expose a built-in header/footer API. The README
// documents PageSize, PageOrientation, network adapters, and font mapping --
// there is no Header/Footer property on PdfGenerator or PdfGenerateConfig.
// To approximate page-chrome, embed the markup directly inside the source HTML
// (e.g. with position:fixed) and let the renderer lay it out.
using PeachPDF;
using PeachPDF.PdfSharpCore;
using System.IO;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        var html = @"
            <html>
              <head>
                <style>
                  .header { position: fixed; top: 0; left: 0; right: 0; text-align: center; }
                  .footer { position: fixed; bottom: 0; left: 0; right: 0; text-align: center; }
                </style>
              </head>
              <body>
                <div class='header'>My Header</div>
                <h1>Document Content</h1>
                <div class='footer'>Footer text</div>
              </body>
            </html>";

        var pdfConfig = new PdfGenerateConfig
        {
            PageSize = PageSize.Letter,
            PageOrientation = PageOrientation.Portrait
        };

        var generator = new PdfGenerator();
        var document = await generator.GeneratePdf(html, pdfConfig);

        using var stream = File.Create("document.pdf");
        document.Save(stream);
    }
}
