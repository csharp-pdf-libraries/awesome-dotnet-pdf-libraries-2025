// NuGet: Install-Package ActivePDF.Toolkit (current owner: Apryse, formerly PDFTron)
// Docs: https://documentation.activepdf.com/toolkit/toolkit_api/
using APToolkitNET;
using System;

class Program
{
    static void Main()
    {
        using (Toolkit toolkit = new Toolkit())
        {
            if (toolkit.OpenOutputFile("merged.pdf") == 0)
            {
                // MergeFile is the documented merge method on the Toolkit.
                // Signature: MergeFile(FileName, StartPage, EndPage)
                toolkit.MergeFile("document1.pdf", 1, -1);
                toolkit.MergeFile("document2.pdf", 1, -1);
                toolkit.CloseOutputFile();
                Console.WriteLine("PDFs merged successfully");
            }
        }
    }
}