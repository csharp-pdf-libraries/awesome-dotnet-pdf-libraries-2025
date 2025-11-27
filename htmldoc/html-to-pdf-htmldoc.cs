// HTMLDOC command-line approach
using System.Diagnostics;

class HtmlDocExample
{
    static void Main()
    {
        // HTMLDOC requires external executable
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = "htmldoc",
            Arguments = "--webpage -f output.pdf input.html",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            CreateNoWindow = true
        };
        
        Process process = Process.Start(startInfo);
        process.WaitForExit();
    }
}