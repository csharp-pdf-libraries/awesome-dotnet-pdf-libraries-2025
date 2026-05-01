// Sumatra PDF is a standalone Windows viewer (AGPLv3) — no official NuGet package
// and no programmatic text-extraction API. To extract text you must shell out to a
// separate tool such as pdftotext (Xpdf / Poppler).
using System;
using System.Diagnostics;
using System.IO;

class Program
{
    static void Main()
    {
        // Sumatra PDF is a viewer, not a text extraction library
        // You'd need to use PDFBox, iTextSharp, or similar for extraction
        
        string pdfFile = "document.pdf";
        
        // This would require external tools like pdftotext
        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = "pdftotext.exe",
            Arguments = $"{pdfFile} output.txt",
            UseShellExecute = false
        };
        
        Process.Start(psi)?.WaitForExit();
        
        string extractedText = File.ReadAllText("output.txt");
        Console.WriteLine(extractedText);
    }
}