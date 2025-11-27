// NuGet: Install-Package Aspose.PDF
using Aspose.Pdf;
using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string htmlContent = "<html><body><h1>Hello World</h1><p>This is a PDF from HTML string.</p></body></html>";
        
        using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(htmlContent)))
        {
            var htmlLoadOptions = new HtmlLoadOptions();
            var document = new Document(stream, htmlLoadOptions);
            document.Save("output.pdf");
        }
        
        Console.WriteLine("PDF created from HTML string");
    }
}