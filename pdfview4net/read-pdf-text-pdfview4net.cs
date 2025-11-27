// NuGet: Install-Package O2S.Components.PDFView4NET
using O2S.Components.PDFView4NET;
using System;
using System.IO;

class Program
{
    static void Main()
    {
        using (FileStream fs = File.OpenRead("document.pdf"))
        {
            PDFDocument document = new PDFDocument(fs);
            string text = "";
            for (int i = 0; i < document.Pages.Count; i++)
            {
                text += document.Pages[i].ExtractText();
            }
            Console.WriteLine(text);
        }
    }
}