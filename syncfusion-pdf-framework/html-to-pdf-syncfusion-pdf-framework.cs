// NuGet: Install-Package Syncfusion.HtmlToPdfConverter.Net.Windows
// (also available: Syncfusion.HtmlToPdfConverter.Net.Linux / .Mac).
// Pulls in Syncfusion.Pdf.Net.Core as a dependency. Uses the Blink rendering engine.
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