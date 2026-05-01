// NuGet: Install-Package PrinceXMLWrapper
// Wrapper invokes the prince.exe binary; install Prince separately from princexml.com
using PrinceXML.Wrapper;
using System;
using System.IO;

class Program
{
    static void Main()
    {
        string html = "<html><head><style>body { font-family: Arial; color: blue; }</style></head><body><h1>Hello World</h1></body></html>";

        Prince prince = new Prince("C:\\Program Files\\Prince\\engine\\bin\\prince.exe");
        // Prince accepts a string of input directly via ConvertString
        prince.ConvertString(html, "styled-output.pdf");
        Console.WriteLine("Styled PDF created");
    }
}