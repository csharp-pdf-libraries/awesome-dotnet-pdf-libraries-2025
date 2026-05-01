// NuGet: Install-Package PrinceXMLWrapper
// Wrapper invokes the prince.exe binary; install Prince separately from princexml.com
// API reference: https://yeslogic.github.io/prince-csharp-wrapper/
using PrinceXML.Wrapper;
using System;

class Program
{
    static void Main()
    {
        Prince prince = new Prince("C:\\Program Files\\Prince\\engine\\bin\\prince.exe");
        // Current PrinceXMLWrapper exposes configuration via properties, not Set* methods
        prince.JavaScript = true;
        prince.Encrypt = true;
        prince.PdfTitle = "Website Export";
        prince.Convert("https://example.com", "webpage.pdf");
        Console.WriteLine("URL converted to PDF");
    }
}