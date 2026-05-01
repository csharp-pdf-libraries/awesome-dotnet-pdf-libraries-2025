// Apache PDFBox does not support HTML-to-PDF rendering — neither the
// Java original nor any of the .NET ports (Pdfbox-IKVM, PdfBox_DotNet_Version,
// MASES.NetPDF) ship an HTML/CSS renderer. PDFBox is a low-level PDF
// manipulation library: you build pages and content streams by hand.
//
// To produce a PDF "from HTML" with PDFBox you must:
//   1. Render the HTML to text/images yourself (external engine), or
//   2. Hand-construct a PDPage and PDPageContentStream, drawing strings
//      and shapes at explicit coordinates.
//
// The closest minimal example below creates a single page with text — it
// does NOT parse or layout HTML. There is no PDFBox API that takes
// "<h1>Hello</h1>" and produces a styled PDF.

using org.apache.pdfbox.pdmodel;
using org.apache.pdfbox.pdmodel.font;
using System;

class Program
{
    static void Main()
    {
        PDDocument document = new PDDocument();
        try
        {
            PDPage page = new PDPage();
            document.addPage(page);

            PDPageContentStream cs = new PDPageContentStream(document, page);
            cs.beginText();
            cs.setFont(PDType1Font.HELVETICA_BOLD, 24);
            cs.newLineAtOffset(72, 700);
            cs.showText("Hello World"); // No HTML parsing — literal text only
            cs.endText();
            cs.close();

            document.save("output.pdf");
            Console.WriteLine("PDF created (text only — no HTML rendering)");
        }
        finally
        {
            document.close();
        }
    }
}
