// Requires TX Text Control .NET Server for ASP.NET (licensed installer/private NuGet feed).
// HeaderFooter objects are added to a Section's HeadersAndFooters collection.
using TXTextControl;
using System.IO;

namespace TextControlExample
{
    class Program
    {
        [System.STAThread]
        static void Main(string[] args)
        {
            using (ServerTextControl textControl = new ServerTextControl())
            {
                textControl.Create();

                string html = "<html><body><h1>Document Content</h1><p>Main body text.</p></body></html>";
                // String load overload uses StringStreamType.
                textControl.Load(html, StringStreamType.HTMLFormat);

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