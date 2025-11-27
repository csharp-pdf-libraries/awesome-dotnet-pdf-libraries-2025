// NuGet: Install-Package APToolkitNET
using ActivePDF.Toolkit;
using System;

class Program
{
    static void Main()
    {
        Toolkit toolkit = new Toolkit();
        
        string url = "https://www.example.com";
        
        if (toolkit.OpenOutputFile("webpage.pdf") == 0)
        {
            toolkit.AddURL(url);
            toolkit.CloseOutputFile();
            Console.WriteLine("PDF from URL created successfully");
        }
    }
}