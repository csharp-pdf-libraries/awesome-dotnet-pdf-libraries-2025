// NuGet: Install-Package ABCpdf
using System;
using WebSupergoo.ABCpdf13;
using WebSupergoo.ABCpdf13.Objects;

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