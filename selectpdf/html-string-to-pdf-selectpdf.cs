// NuGet: Install-Package Select.HtmlToPdf
using SelectPdf;
using System;

class Program
{
    static void Main()
    {
        string htmlContent = "<html><body><h1>Hello World</h1><p>This is a PDF document.</p></body></html>";
        
        HtmlToPdf converter = new HtmlToPdf();
        PdfDocument doc = converter.ConvertHtmlString(htmlContent);
        doc.Save("document.pdf");
        doc.Close();
        
        Console.WriteLine("PDF generated from HTML string");
    }
}