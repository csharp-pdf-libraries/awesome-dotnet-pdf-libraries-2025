// NuGet: Install-Package PrinceXMLWrapper
// Wrapper invokes the prince.exe binary; install Prince separately from princexml.com
using PrinceXML.Wrapper;
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