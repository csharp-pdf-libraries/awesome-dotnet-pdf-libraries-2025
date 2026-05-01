// Sumatra PDF is a standalone Windows app (AGPLv3) — there is NO official NuGet package.
// Download SumatraPDF.exe from https://www.sumatrapdfreader.org/ and shell out to it.
// Sumatra is a viewer/printer only; it cannot convert HTML to PDF.
// You must use a separate HTML-to-PDF tool (e.g. wkhtmltopdf) and then view/print with Sumatra.
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