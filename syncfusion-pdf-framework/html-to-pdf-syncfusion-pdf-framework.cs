// NuGet: Install-Package Syncfusion.Pdf.Net.Core
using Syncfusion.HtmlConverter;
using Syncfusion.Pdf;
using System.IO;

class Program
{
    static void Main()
    {
        // Initialize HTML to PDF converter
        HtmlToPdfConverter htmlConverter = new HtmlToPdfConverter();
        
        // Convert URL to PDF
        PdfDocument document = htmlConverter.Convert("https://www.example.com");
        
        // Save the document
        FileStream fileStream = new FileStream("Output.pdf", FileMode.Create);
        document.Save(fileStream);
        document.Close(true);
        fileStream.Close();
    }
}