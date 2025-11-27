// HTMLDOC command-line with URL and headers
using System.Diagnostics;

class HtmlDocExample
{
    static void Main()
    {
        // HTMLDOC has limited support for URLs and headers
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = "htmldoc",
            Arguments = "--webpage --header \"Page #\" --footer \"t\" -f output.pdf https://example.com",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
        
        Process process = Process.Start(startInfo);
        process.WaitForExit();
        
        // Note: HTMLDOC may not render modern web pages correctly
    }
}