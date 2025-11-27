// NuGet: Install-Package PDFFilePrint
using System;
using PDFFilePrint;

class Program
{
    static void Main()
    {
        var pdf = new PDFFile();
        pdf.LoadFromFile("document.pdf");
        pdf.Print("Default Printer");
        Console.WriteLine("PDF sent to printer");
    }
}