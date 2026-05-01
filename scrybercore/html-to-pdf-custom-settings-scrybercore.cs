// NuGet: Install-Package Scryber.Core
// Page size and margins are normally declared in the XHTML/CSS template;
// the runtime RenderOptions object exposes output-side settings such as compression.
using Scryber.Components;
using Scryber.PDF;
using System.IO;

class Program
{
    static void Main()
    {
        // Page size and margins go in CSS @page; this is the Scryber idiom.
        string html = @"<html xmlns='http://www.w3.org/1999/xhtml'>
            <head>
                <style>
                    @page { size: A4 portrait; margin: 40pt; }
                </style>
            </head>
            <body><h1>Custom PDF</h1><p>With custom margins and settings.</p></body>
        </html>";

        using (var reader = new StringReader(html))
        using (var doc = Document.ParseDocument(reader, string.Empty, ParseSourceType.DynamicContent))
        using (var stream = new FileStream("custom.pdf", FileMode.Create))
        {
            // Output-side options (compression / conformance) are on RenderOptions.
            doc.RenderOptions.Compression = OutputCompressionType.FlateDecode;
            doc.SaveAsPDF(stream);
        }
    }
}
