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
                
                byte[] pdf1 = File.ReadAllBytes("document1.pdf");
                textControl.Load(pdf1, StreamType.AdobePDF);
                
                byte[] pdf2 = File.ReadAllBytes("document2.pdf");
                textControl.Load(pdf2, StreamType.AdobePDF, LoadAppendMode.Append);
                
                textControl.Save("merged.pdf", StreamType.AdobePDF);
            }
        }
    }
}