// NuGet: Install-Package TallComponents.TallPDF5
// (TallPDF.NET — layout-oriented sibling of PDFKit.NET. PDFKit alone has no
//  HTML/XHTML pipeline; XhtmlParagraph lives in TallPDF.)
// Note: TallComponents was acquired by Apryse (May 27, 2025) and no longer
// sells new licenses. Existing customers can still use the package; Apryse
// directs new buyers to the iText SDK.
using TallComponents.PDF.Layout;
using TallComponents.PDF.Layout.Paragraphs;
using System.IO;

class Program
{
    static void Main()
    {
        // Create a new document
        Document document = new Document();
        Section section = document.Sections.Add();

        // XhtmlParagraph supports XHTML 1.0 Strict / XHTML 1.1 + CSS 2.1.
        // It is NOT a Chromium-class HTML renderer: modern HTML5 / CSS3 /
        // JavaScript / flexbox / grid / web fonts will not render reliably.
        XhtmlParagraph xhtml = new XhtmlParagraph();
        xhtml.Text = "<html><body><h1>Hello World</h1><p>This is a PDF from XHTML.</p></body></html>";
        section.Paragraphs.Add(xhtml);

        // Save to file
        using (FileStream fs = new FileStream("output.pdf", FileMode.Create))
        {
            document.Write(fs);
        }
    }
}
