// NuGet: Install-Package O2S.Components.PDFView4NET.Win
// Text extraction in PDFView4NET goes through PDFPage.ExtractText().
// PDFDocument is loaded via the Load(string) or Load(Stream) overload.
using O2S.Components.PDFView4NET;
using System;
using System.IO;

class Program
{
    static void Main()
    {
        using (FileStream fs = File.OpenRead("document.pdf"))
        {
            PDFDocument document = new PDFDocument();
            document.Load(fs);
            string text = "";
            for (int i = 0; i < document.PageCount; i++)
            {
                text += document.Pages[i].ExtractText();
            }
            Console.WriteLine(text);
            document.Close();
        }
    }
}
