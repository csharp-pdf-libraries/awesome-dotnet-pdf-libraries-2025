// Adobe PDF Library SDK (Datalogics APDFL)
// NuGet: Install-Package Adobe.PDF.Library.LM.NET
// Namespace: Datalogics.PDFL
//
// IMPORTANT: APDFL does NOT have a built-in HTML-rendering engine. The vendor's
// feature page lists conversions like PDF/A, PDF/X, ZUGFeRD, EPS, PS, XPS, and
// Office formats, but not HTML-to-PDF. To produce a PDF from an HTML string with
// APDFL alone, you must build pages and content yourself (shown below) or pair
// APDFL with an external HTML rendering engine.
//
// References:
//   https://www.datalogics.com/adobe-pdf-library
//   https://github.com/datalogics/apdfl-csharp-dotnet-samples (DocumentConversion)
using Datalogics.PDFL;
using System;

class AdobeHtmlToPdf
{
    static void Main()
    {
        using (Library lib = new Library())
        {
            // Programmatic construction substitutes for HTML rendering.
            using (Document doc = new Document())
            {
                // US Letter in points (8.5 x 11 inches @ 72 DPI)
                Rect pageRect = new Rect(0, 0, 612, 792);
                using (Page page = doc.CreatePage(Document.BeforeFirstPage, pageRect))
                {
                    Content content = page.Content;
                    Font font = new Font("Helvetica", FontCreateFlags.Embedded);

                    Text text = new Text();
                    text.AddRun(new TextRun("Hello World", font, 24, new Point(72, 720)));
                    content.AddElement(text);

                    page.UpdateContent();
                }

                doc.Save(SaveFlags.Full, "output.pdf");
            }
        }
    }
}
