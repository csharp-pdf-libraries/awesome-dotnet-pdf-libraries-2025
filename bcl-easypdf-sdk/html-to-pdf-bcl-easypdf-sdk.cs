// NuGet: Install-Package BCL.EasyPDF
using BCL.EasyPDF;
using System;

class Program
{
    static void Main()
    {
        var pdf = new PDFDocument();
        var htmlConverter = new HTMLConverter();
        htmlConverter.ConvertHTML("<h1>Hello World</h1>", pdf);
        pdf.Save("output.pdf");
        pdf.Close();
    }
}