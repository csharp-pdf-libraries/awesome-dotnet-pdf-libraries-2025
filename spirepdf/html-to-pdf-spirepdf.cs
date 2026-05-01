// NuGet: Install-Package Spire.PDF
// Verified against e-iceblue docs: https://www.e-iceblue.com/Knowledgebase/Spire.PDF/Program-Guide/Convert-HTML-to-PDF.html
using Spire.Pdf;
using Spire.Pdf.Graphics;
using System;

class Program
{
    static void Main()
    {
        PdfDocument pdf = new PdfDocument();
        PdfPageSettings setting = new PdfPageSettings();
        setting.Size = PdfPageSize.A4;
        PdfHtmlLayoutFormat htmlLayoutFormat = new PdfHtmlLayoutFormat();
        htmlLayoutFormat.IsWaiting = false;

        string htmlString = "<html><body><h1>Hello World</h1><p>This is a PDF from HTML.</p></body></html>";

        // HTML-string overload: LoadFromHTML(string, bool autoDetectPageBreak, PdfPageSettings, PdfHtmlLayoutFormat)
        // (4-bool overload is for URL: LoadFromHTML(url, enableJS, enableHyperlinks, autoDetectPageBreak))
        pdf.LoadFromHTML(htmlString, true, setting, htmlLayoutFormat);
        pdf.SaveToFile("output.pdf");
        pdf.Close();
    }
}