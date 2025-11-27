// NuGet: Install-Package PdfiumViewer
using PdfiumViewer;
using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string pdfPath = "document.pdf";
        
        using (var document = PdfDocument.Load(pdfPath))
        {
            StringBuilder text = new StringBuilder();
            
            for (int i = 0; i < document.PageCount; i++)
            {
                // Note: PdfiumViewer has limited text extraction capabilities
                // Text extraction requires additional work with Pdfium.NET
                string pageText = document.GetPdfText(i);
                text.AppendLine(pageText);
            }
            
            Console.WriteLine(text.ToString());
        }
    }
}