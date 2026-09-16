// NuGet: Install-Package ABCpdf (v14.0.5 as of July 2026)
using System;
using WebSupergoo.ABCpdf14;
using WebSupergoo.ABCpdf14.Objects;

class Program
{
    static void Main()
    {
        Doc doc = new Doc();
        doc.HtmlOptions.Engine = EngineType.Chrome146; // default in v14; also Chrome123/117/86/65, Gecko, WebKit, MSHtml
        doc.AddImageUrl("https://www.example.com");
        doc.Save("output.pdf");
        doc.Clear();
    }
}