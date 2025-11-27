// NuGet: Install-Package O2S.Components.PDFView4NET
using O2S.Components.PDFView4NET;
using O2S.Components.PDFView4NET.HtmlToPdf;
using System;

class Program
{
    static void Main()
    {
        string htmlContent = "<html><body><h1>Hello World</h1><p>This is a PDF document.</p></body></html>";
        HtmlToPdfConverter converter = new HtmlToPdfConverter();
        converter.HtmlContent = htmlContent;
        converter.ConvertHtmlToPdf();
        converter.SavePdf("document.pdf");
    }
}