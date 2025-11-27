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
                
                string html = "<html><body><h1>Document Content</h1><p>Main body text.</p></body></html>";
                textControl.Load(html, StreamType.HTMLFormat);
                
                HeaderFooter header = new HeaderFooter(HeaderFooterType.Header);
                header.Text = "Document Header";
                textControl.Sections[0].HeadersAndFooters.Add(header);
                
                HeaderFooter footer = new HeaderFooter(HeaderFooterType.Footer);
                footer.Text = "Page {page} of {numpages}";
                textControl.Sections[0].HeadersAndFooters.Add(footer);
                
                textControl.Save("output.pdf", StreamType.AdobePDF);
            }
        }
    }
}