// NuGet: Install-Package PrinceXMLWrapper
using PrinceXMLWrapper;
using System;

class Program
{
    static void Main()
    {
        Prince prince = new Prince("C:\\Program Files\\Prince\\engine\\bin\\prince.exe");
        prince.SetJavaScript(true);
        prince.SetEncrypt(true);
        prince.SetPDFTitle("Website Export");
        prince.Convert("https://example.com", "webpage.pdf");
        Console.WriteLine("URL converted to PDF");
    }
}