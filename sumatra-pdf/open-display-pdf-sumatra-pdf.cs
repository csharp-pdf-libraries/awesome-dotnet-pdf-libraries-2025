// Sumatra PDF is a standalone Windows app (AGPLv3) — no official NuGet package.
// Download SumatraPDF.exe from https://www.sumatrapdfreader.org/ and call it via Process.Start.
// Useful CLI flags: -page <n>, -print-to-default, -print-to "<printer>", -print-settings, -silent.
using System.Diagnostics;
using System.IO;

class Program
{
    static void Main()
    {
        string pdfPath = "document.pdf";
        
        // Sumatra PDF excels at viewing PDFs
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = "SumatraPDF.exe",
            Arguments = $"\"{pdfPath}\"",
            UseShellExecute = true
        };
        
        Process.Start(startInfo);
        
        // Optional: Open specific page
        // Arguments = $"-page 5 \"{pdfPath}\""
    }
}