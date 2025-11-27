// NuGet: Install-Package SumatraPDF (Note: Sumatra is primarily a viewer, not a generator)
// Sumatra PDF doesn't have direct C# integration for HTML to PDF conversion
// You would need to use external tools or libraries and then open with Sumatra
using System.Diagnostics;
using System.IO;

class Program
{
    static void Main()
    {
        // Sumatra PDF cannot directly convert HTML to PDF
        // You'd need to use wkhtmltopdf or similar, then view in Sumatra
        string htmlFile = "input.html";
        string pdfFile = "output.pdf";
        
        // Using wkhtmltopdf as intermediary
        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = "wkhtmltopdf.exe",
            Arguments = $"{htmlFile} {pdfFile}",
            UseShellExecute = false
        };
        Process.Start(psi)?.WaitForExit();
        
        // Then open with Sumatra
        Process.Start("SumatraPDF.exe", pdfFile);
    }
}