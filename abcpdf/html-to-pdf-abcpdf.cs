// NuGet: Install-Package ABCpdf
using System;
using WebSupergoo.ABCpdf13;
using WebSupergoo.ABCpdf13.Objects;

class Program
{
    static void Main()
    {
        Doc doc = new Doc();
        doc.HtmlOptions.Engine = EngineType.Chrome;
        doc.AddImageUrl("https://www.example.com");
        doc.Save("output.pdf");
        doc.Clear();
    }
}