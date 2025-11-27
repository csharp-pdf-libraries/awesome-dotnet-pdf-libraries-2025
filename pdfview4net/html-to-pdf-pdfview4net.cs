// NuGet: Install-Package O2S.Components.PDFView4NET
using O2S.Components.PDFView4NET;
using O2S.Components.PDFView4NET.HtmlToPdf;
using System;

class Program
{
    static void Main()
    {
        HtmlToPdfConverter converter = new HtmlToPdfConverter();
        converter.NavigateUri = new Uri("https://example.com");
        converter.ConvertHtmlToPdf();
        converter.SavePdf("output.pdf");
    }
}