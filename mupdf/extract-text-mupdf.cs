// NuGet: Install-Package MuPDF.NET
using MuPDFCore;
using System;
using System.Text;

class Program
{
    static void Main()
    {
        using (MuPDFDocument document = new MuPDFDocument("input.pdf"))
        {
            StringBuilder allText = new StringBuilder();
            
            for (int i = 0; i < document.Pages.Count; i++)
            {
                string pageText = document.Pages[i].GetText();
                allText.AppendLine(pageText);
            }
            
            Console.WriteLine(allText.ToString());
        }
    }
}