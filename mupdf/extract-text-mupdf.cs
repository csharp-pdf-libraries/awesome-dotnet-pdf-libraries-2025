// NuGet: Install-Package MuPDF.NET
// Artifex's official C# bindings for MuPDF (mirrors PyMuPDF surface).
// See https://mupdfnet.readthedocs.io/
using MuPDF.NET;
using System;
using System.Text;

class Program
{
    static void Main()
    {
        Document doc = new Document("input.pdf");

        StringBuilder allText = new StringBuilder();

        for (int i = 0; i < doc.PageCount; i++)
        {
            string pageText = doc[i].GetText();
            allText.AppendLine(pageText);
        }

        Console.WriteLine(allText.ToString());
    }
}
