// NuGet: Install-Package Scryber.Core
using Scryber.Core;
using Scryber.Core.Html;
using Scryber.Drawing;
using System.IO;

class Program
{
    static void Main()
    {
        string html = "<html><body><h1>Custom PDF</h1><p>With custom margins and settings.</p></body></html>";
        
        using (var doc = Document.ParseDocument(html, ParseSourceType.DynamicContent))
        {
            doc.RenderOptions.Compression = OutputCompressionType.FlateDecode;
            doc.RenderOptions.PaperSize = PaperSize.A4;
            doc.SaveAsPDF("custom.pdf");
        }
    }
}