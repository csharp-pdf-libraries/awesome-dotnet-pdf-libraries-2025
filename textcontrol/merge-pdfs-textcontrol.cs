// Requires TX Text Control .NET Server for ASP.NET (licensed installer; TXTextControl.Web on
// public NuGet is the companion editor package). The byte[] overloads use BinaryStreamType,
// and additional content is concatenated via the Append method with AppendSettings.
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

                // Byte[] overloads take BinaryStreamType (per docs.textcontrol.com).
                byte[] pdf1 = File.ReadAllBytes("document1.pdf");
                textControl.Load(pdf1, BinaryStreamType.AdobePDF);

                // To merge, append the second document. ServerTextControl.Append takes
                // BinaryStreamType plus an AppendSettings flags value (e.g. StartWithNewSection).
                byte[] pdf2 = File.ReadAllBytes("document2.pdf");
                textControl.Append(pdf2, BinaryStreamType.AdobePDF, AppendSettings.StartWithNewSection);

                textControl.Save("merged.pdf", StreamType.AdobePDF);
            }
        }
    }
}