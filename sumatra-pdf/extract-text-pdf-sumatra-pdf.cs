// Sumatra PDF doesn't provide C# API for text extraction
// You would need to use command-line tools or other libraries
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