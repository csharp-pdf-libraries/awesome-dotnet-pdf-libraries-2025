// NuGet: Install-Package Scryber.Core
// License: LGPL v3 — see https://github.com/richard-scryber/scryber.core/blob/master/LICENSE.md
using Scryber.Components;
using System.IO;

class Program
{
    static void Main()
    {
        // Scryber requires well-formed XHTML wrapped in an html root with the xhtml namespace
        string html = @"<html xmlns='http://www.w3.org/1999/xhtml'>
            <body><h1>Hello World</h1><p>This is a PDF document.</p></body>
        </html>";

        using (var reader = new StringReader(html))
        using (var doc = Document.ParseDocument(reader, string.Empty, ParseSourceType.DynamicContent))
        using (var stream = new FileStream("output.pdf", FileMode.Create))
        {
            doc.SaveAsPDF(stream);
        }
    }
}
