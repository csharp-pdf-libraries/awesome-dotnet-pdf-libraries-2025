// NuGet: Install-Package Ghostscript.NET
using Ghostscript.NET;
using Ghostscript.NET.Processor;
using System.IO;
using System.Text;

class GhostscriptExample
{
    static void Main()
    {
        // Ghostscript cannot directly convert HTML to PDF
        // You need to first convert HTML to PS/EPS using another tool
        // then use Ghostscript to convert PS to PDF
        
        string htmlContent = "<html><body><h1>Hello World</h1></body></html>";
        string psFile = "temp.ps";
        string outputPdf = "output.pdf";
        
        // This is a workaround - Ghostscript primarily works with PostScript
        GhostscriptProcessor processor = new GhostscriptProcessor();
        
        List<string> switches = new List<string>
        {
            "-dNOPAUSE",
            "-dBATCH",
            "-dSAFER",
            "-sDEVICE=pdfwrite",
            $"-sOutputFile={outputPdf}",
            psFile
        };
        
        processor.Process(switches.ToArray());
    }
}