// NuGet: Install-Package Ghostscript.NET
using Ghostscript.NET;
using Ghostscript.NET.Processor;
using System.Collections.Generic;

class GhostscriptExample
{
    static void Main()
    {
        string outputPdf = "merged.pdf";
        string[] inputFiles = { "file1.pdf", "file2.pdf", "file3.pdf" };
        
        GhostscriptProcessor processor = new GhostscriptProcessor();
        
        List<string> switches = new List<string>
        {
            "-dNOPAUSE",
            "-dBATCH",
            "-dSAFER",
            "-sDEVICE=pdfwrite",
            $"-sOutputFile={outputPdf}"
        };
        
        switches.AddRange(inputFiles);
        
        processor.Process(switches.ToArray());
    }
}