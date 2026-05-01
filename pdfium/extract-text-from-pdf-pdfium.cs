// NuGet: Install-Package PdfiumViewer  (Apache 2.0; archived Aug 2019, .NET Framework 2.0+)
//        or Install-Package PdfiumViewer.Updated  (maintained fork, .NET Core / .NET 6)
//        or Install-Package PDFiumCore             (Dtronix, .NET Standard 2.1 P/Invoke)
//        or Install-Package Pdfium.Net.SDK         (Patagames, commercial perpetual license)
// PDFium itself is Google's BSD-3-Clause C++ engine; .NET access is via these community wrappers.
using PdfiumViewer;
using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string pdfPath = "document.pdf";

        using (var document = PdfDocument.Load(pdfPath))
        {
            StringBuilder text = new StringBuilder();

            for (int i = 0; i < document.PageCount; i++)
            {
                // PdfiumViewer's text extraction is per-page raw text via GetPdfText(int).
                // No layout/format metadata is exposed.
                string pageText = document.GetPdfText(i);
                text.AppendLine(pageText);
            }

            Console.WriteLine(text.ToString());
        }
    }
}