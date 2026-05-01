// NuGet: Install-Package VectSharp.PDF
// VectSharp is a vector graphics library; SaveAsPDF is an extension method
// from the VectSharp.PDF package. It has no HTML-to-PDF support.
using VectSharp;
using VectSharp.PDF;

class Program
{
    static void Main()
    {
        // VectSharp does not support HTML to PDF.
        // Documents must be built up with manual graphics calls.
        Document doc = new Document();
        Page page = new Page(595, 842); // A4 size in PDF points
        Graphics graphics = page.Graphics;

        Font font = new Font(
            FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.Helvetica),
            24);

        // FillText(x, y, text, font, colour) - colour is required.
        graphics.FillText(100, 100, "Hello from VectSharp", font, Colours.Black);

        doc.Pages.Add(page);
        doc.SaveAsPDF("output.pdf");
    }
}