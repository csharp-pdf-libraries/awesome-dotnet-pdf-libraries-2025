// NuGet: Install-Package Select.HtmlToPdf
using SelectPdf;
using System;

class Program
{
    static void Main()
    {
        HtmlToPdf converter = new HtmlToPdf();
        
        converter.Options.PdfPageSize = PdfPageSize.A4;
        converter.Options.PdfPageOrientation = PdfPageOrientation.Portrait;
        converter.Options.MarginTop = 20;
        converter.Options.MarginBottom = 20;
        converter.Options.MarginLeft = 20;
        converter.Options.MarginRight = 20;
        
        string html = "<html><body><h1>Custom Page Settings</h1></body></html>";
        PdfDocument doc = converter.ConvertHtmlString(html);
        doc.Save("custom-settings.pdf");
        doc.Close();
        
        Console.WriteLine("PDF with custom settings created");
    }
}