// NuGet: Install-Package PrinceXMLWrapper
using PrinceXMLWrapper;
using System;

class Program
{
    static void Main()
    {
        Prince prince = new Prince("C:\\Program Files\\Prince\\engine\\bin\\prince.exe");
        prince.Convert("input.html", "output.pdf");
        Console.WriteLine("PDF created successfully");
    }
}