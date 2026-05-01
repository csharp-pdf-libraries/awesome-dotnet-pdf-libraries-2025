// NuGet: Install-Package ABCpdf (v13.4.4 as of April 2026)
using System;
using WebSupergoo.ABCpdf13;
using WebSupergoo.ABCpdf13.Objects;

class Program
{
    static void Main()
    {
        string html = "<html><body><h1>Hello World</h1><p>This is a PDF document.</p></body></html>";
        Doc doc = new Doc();
        doc.HtmlOptions.Engine = EngineType.Chrome123; // default in v13; older versions: Chrome117/86/65
        doc.AddImageHtml(html);
        doc.Save("output.pdf");
        doc.Clear();
    }
}