// NuGet: Install-Package Aspose.PDF
using Aspose.Pdf;
using System;

class Program
{
    static void Main()
    {
        var htmlLoadOptions = new HtmlLoadOptions();
        var document = new Document("input.html", htmlLoadOptions);
        document.Save("output.pdf");
        Console.WriteLine("PDF created successfully");
    }
}