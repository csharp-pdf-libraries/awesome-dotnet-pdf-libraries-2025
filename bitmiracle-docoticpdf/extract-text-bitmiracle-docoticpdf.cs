// NuGet: Install-Package Docotic.Pdf
using BitMiracle.Docotic.Pdf;
using System;

class Program
{
    static void Main()
    {
        using (var pdf = new PdfDocument("document.pdf"))
        {
            string allText = "";
            
            foreach (var page in pdf.Pages)
            {
                allText += page.GetText();
            }
            
            Console.WriteLine("Extracted text:");
            Console.WriteLine(allText);
        }
    }
}