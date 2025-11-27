// NuGet: Install-Package SumatraPDF.CommandLine (or direct executable)
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