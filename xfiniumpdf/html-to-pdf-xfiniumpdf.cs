// NuGet: Install-Package Xfinium.Pdf.NetStandard (or .NetCore)
// Note: XFINIUM.PDF has NO native HTML-to-PDF engine. The closest in-box
// approach uses the flow document API with plain/formatted text content -
// HTML markup is NOT parsed. Vendors ship a sample HTML-to-PDF converter
// that walks XHTML with XmlReader and emits PdfFormattedContent for a
// limited tag set (p, font, b, i, u, ul, li); see xfiniumpdf.blog.
using Xfinium.Pdf;
using Xfinium.Pdf.FlowDocument;
using Xfinium.Pdf.Graphics;
using System.IO;

class Program
{
    static void Main()
    {
        PdfFlowDocument flowDocument = new PdfFlowDocument();

        // Flow text content - no HTML parsing. You must build content as
        // PdfFlowTextContent / PdfFlowHeadingContent / PdfFlowImageContent etc.
        PdfFlowHeadingContent heading = new PdfFlowHeadingContent(
            "Hello World",
            new PdfStandardFont(PdfStandardFontFace.HelveticaBold, 18));
        flowDocument.AddContent(heading);

        PdfFlowTextContent text = new PdfFlowTextContent(
            "This is a PDF generated from text content (not HTML).",
            new PdfStandardFont(PdfStandardFontFace.Helvetica, 12));
        flowDocument.AddContent(text);

        // Save the flow document directly
        using (FileStream fs = new FileStream("output.pdf", FileMode.Create))
        {
            flowDocument.Save(fs);
        }
    }
}
