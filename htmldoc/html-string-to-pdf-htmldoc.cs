// HTMLDOC command-line with string input
using System.Diagnostics;
using System.IO;

class HtmlDocExample
{
    static void Main()
    {
        string htmlContent = "<html><body><h1>Hello World</h1></body></html>";
        
        // Write HTML to temporary file
        string tempFile = Path.GetTempFileName() + ".html";
        File.WriteAllText(tempFile, htmlContent);
        
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = "htmldoc",
            Arguments = $"--webpage -f output.pdf {tempFile}",
            UseShellExecute = false,
            CreateNoWindow = true
        };
        
        Process process = Process.Start(startInfo);
        process.WaitForExit();
        
        File.Delete(tempFile);
    }
}