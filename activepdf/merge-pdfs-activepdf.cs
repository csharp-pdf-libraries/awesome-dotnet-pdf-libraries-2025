// NuGet: Install-Package APToolkitNET
using ActivePDF.Toolkit;
using System;

class Program
{
    static void Main()
    {
        Toolkit toolkit = new Toolkit();
        
        if (toolkit.OpenOutputFile("merged.pdf") == 0)
        {
            toolkit.AddPDF("document1.pdf");
            toolkit.AddPDF("document2.pdf");
            toolkit.CloseOutputFile();
            Console.WriteLine("PDFs merged successfully");
        }
    }
}