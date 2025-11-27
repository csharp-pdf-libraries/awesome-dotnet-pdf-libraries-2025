// NuGet: Install-Package Scryber.Core
using Scryber.Core;
using Scryber.Core.Html;
using System.IO;

class Program
{
    static void Main()
    {
        string html = "<html><body><h1>Hello World</h1><p>This is a PDF document.</p></body></html>";
        
        using (var doc = Document.ParseDocument(html, ParseSourceType.DynamicContent))
        {
            doc.SaveAsPDF("output.pdf");
        }
    }
}