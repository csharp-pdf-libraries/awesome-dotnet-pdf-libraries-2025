// Requires TX Text Control .NET Server for ASP.NET (installer/MSI from textcontrol.com).
// The TXTextControl.Web NuGet package is the public companion; the headless ServerTextControl
// runtime is delivered via the licensed installer or the vendor's private NuGet feed.
using TXTextControl;
using System.IO;

namespace TextControlExample
{
    class Program
    {
        // ServerTextControl historically requires STA-compatible hosting in ASP.NET pipelines.
        [System.STAThread]
        static void Main(string[] args)
        {
            using (ServerTextControl textControl = new ServerTextControl())
            {
                textControl.Create();

                string html = "<html><body><h1>Hello World</h1><p>This is a PDF document.</p></body></html>";

                // String overload uses StringStreamType (per textcontrol.com docs/blog).
                textControl.Load(html, StringStreamType.HTMLFormat);
                // File-path Save overload uses StreamType.
                textControl.Save("output.pdf", StreamType.AdobePDF);
            }
        }
    }
}