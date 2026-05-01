// NuGet: Install-Package itext.pdfhtml
// (pdfHTML 6.3.2 is a separate paid add-on; pulls in itext 9.6.0 as a dependency.
//  Both packages are AGPL or commercial — see https://itextpdf.com/pricing)
using iText.Html2pdf;
using System.IO;

class Program
{
    static void Main()
    {
        string html = "<h1>Hello World</h1><p>This is a PDF from HTML.</p>";
        string outputPath = "output.pdf";
        
        using (FileStream fs = new FileStream(outputPath, FileMode.Create))
        {
            HtmlConverter.ConvertToPdf(html, fs);
        }
    }
}