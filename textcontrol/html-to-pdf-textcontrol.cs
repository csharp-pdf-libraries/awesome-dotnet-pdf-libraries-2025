// NuGet: Install-Package TXTextControl.Server
using TXTextControl;
using System.IO;

namespace TextControlExample
{
    class Program
    {
        static void Main(string[] args)
        {
            using (ServerTextControl textControl = new ServerTextControl())
            {
                textControl.Create();
                
                string html = "<html><body><h1>Hello World</h1><p>This is a PDF document.</p></body></html>";
                
                textControl.Load(html, StreamType.HTMLFormat);
                textControl.Save("output.pdf", StreamType.AdobePDF);
            }
        }
    }
}