// NuGet: Install-Package BCL.EasyPDF
using BCL.EasyPDF;
using System;

class Program
{
    static void Main()
    {
        var pdf = new PDFDocument();
        var htmlConverter = new HTMLConverter();
        htmlConverter.ConvertURL("https://example.com", pdf);
        pdf.Save("webpage.pdf");
        pdf.Close();
    }
}