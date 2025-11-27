// NuGet: Install-Package PDFFilePrint
using System;
using PDFFilePrint;

class Program
{
    static void Main()
    {
        var pdf = new PDFFile();
        pdf.CreateFromUrl("https://www.example.com");
        pdf.SaveToFile("webpage.pdf");
    }
}