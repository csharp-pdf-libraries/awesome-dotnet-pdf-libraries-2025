// ZetPDF is NOT distributed via NuGet. Download SDK ZIP from https://zetpdf.com/download/
// and reference the ZetPDF.dll manually. ZetPDF does NOT expose a native HTML-to-PDF
// converter — its documented surface covers PDF view/print, annotations, AES256 encryption,
// form fields, and text extraction (see https://zetpdf.com/net-pdf-sdk/). The snippet
// below is illustrative of how a wrapper would look; in practice, ZetPDF users typically
// build PDFs via the low-level XGraphics API inherited from its PDFsharp lineage.
using ZetPDF;
using System;

class Program
{
    static void Main()
    {
        // No native HtmlToPdfConverter exists in ZetPDF — illustrative call only.
        var converter = new HtmlToPdfConverter();
        var htmlContent = "<html><body><h1>Hello World</h1></body></html>";
        converter.ConvertHtmlToPdf(htmlContent, "output.pdf");
        Console.WriteLine("PDF created successfully");
    }
}