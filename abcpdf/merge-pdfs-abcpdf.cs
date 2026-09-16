// NuGet: Install-Package ABCpdf (v14.0.5 as of July 2026)
using System;
using WebSupergoo.ABCpdf14;
using WebSupergoo.ABCpdf14.Objects;

class Program
{
    static void Main()
    {
        Doc doc1 = new Doc();
        doc1.Read("document1.pdf");
        
        Doc doc2 = new Doc();
        doc2.Read("document2.pdf");
        
        doc1.Append(doc2);
        doc1.Save("merged.pdf");
        
        doc1.Clear();
        doc2.Clear();
    }
}