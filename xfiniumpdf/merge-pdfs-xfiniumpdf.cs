// NuGet: Install-Package Xfinium.Pdf
using Xfinium.Pdf;
using System.IO;

class Program
{
    static void Main()
    {
        PdfFixedDocument output = new PdfFixedDocument();
        
        FileStream file1 = File.OpenRead("document1.pdf");
        PdfFixedDocument pdf1 = new PdfFixedDocument(file1);
        
        FileStream file2 = File.OpenRead("document2.pdf");
        PdfFixedDocument pdf2 = new PdfFixedDocument(file2);
        
        for (int i = 0; i < pdf1.Pages.Count; i++)
        {
            output.Pages.Add(pdf1.Pages[i]);
        }
        
        for (int i = 0; i < pdf2.Pages.Count; i++)
        {
            output.Pages.Add(pdf2.Pages[i]);
        }
        
        output.Save("merged.pdf");
        
        file1.Close();
        file2.Close();
    }
}