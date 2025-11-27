// NuGet: Install-Package PdfiumViewer
using PdfiumViewer;
using System.IO;
using System.Drawing.Printing;

// Note: PdfiumViewer is primarily for viewing/rendering PDFs, not creating them from HTML
// For HTML to PDF with Pdfium.NET, you would need additional libraries
// This example shows a limitation of Pdfium.NET
class Program
{
    static void Main()
    {
        // Pdfium.NET does not have native HTML to PDF conversion
        // You would need to use a separate library to convert HTML to PDF
        // then use Pdfium for manipulation
        string htmlContent = "<h1>Hello World</h1>";
        
        // This functionality is not directly available in Pdfium.NET
        Console.WriteLine("HTML to PDF conversion not natively supported in Pdfium.NET");
    }
}