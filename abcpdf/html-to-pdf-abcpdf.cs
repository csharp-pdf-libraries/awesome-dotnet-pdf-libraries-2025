// NuGet: Install-Package ABCpdf (v13.4.4 as of April 2026)
using System;
using WebSupergoo.ABCpdf13;
using WebSupergoo.ABCpdf13.Objects;

class Program
{
    static void Main()
    {
        Doc doc = new Doc();
        doc.HtmlOptions.Engine = EngineType.Chrome123; // default in v13; older versions: Chrome117/86/65
        doc.AddImageUrl("https://www.example.com");
        doc.Save("output.pdf");
        doc.Clear();
    }
}