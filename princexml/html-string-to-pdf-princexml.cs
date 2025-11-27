// NuGet: Install-Package PrinceXMLWrapper
using PrinceXMLWrapper;
using System;
using System.IO;

class Program
{
    static void Main()
    {
        string html = "<html><head><style>body { font-family: Arial; color: blue; }</style></head><body><h1>Hello World</h1></body></html>";
        File.WriteAllText("temp.html", html);
        
        Prince prince = new Prince("C:\\Program Files\\Prince\\engine\\bin\\prince.exe");
        prince.Convert("temp.html", "styled-output.pdf");
        Console.WriteLine("Styled PDF created");
    }
}