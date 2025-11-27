// NuGet: Install-Package Docotic.Pdf
using BitMiracle.Docotic.Pdf;
using System;

class Program
{
    static void Main()
    {
        using (var pdf = new PdfDocument())
        {
            string html = "<html><body><h1>Hello World</h1><p>This is HTML to PDF conversion.</p></body></html>";
            
            pdf.CreatePage(html);
            pdf.Save("output.pdf");
        }
        
        Console.WriteLine("PDF created successfully");
    }
}