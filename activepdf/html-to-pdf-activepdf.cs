// NuGet: Install-Package APToolkitNET
using ActivePDF.Toolkit;
using System;

class Program
{
    static void Main()
    {
        Toolkit toolkit = new Toolkit();
        
        string htmlContent = "<html><body><h1>Hello World</h1></body></html>";
        
        if (toolkit.OpenOutputFile("output.pdf") == 0)
        {
            toolkit.AddHTML(htmlContent);
            toolkit.CloseOutputFile();
            Console.WriteLine("PDF created successfully");
        }
    }
}