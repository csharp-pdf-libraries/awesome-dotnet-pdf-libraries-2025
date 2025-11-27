// Adobe PDF Library SDK
using Datalogics.PDFL;
using System;

class AdobeHtmlToPdf
{
    static void Main()
    {
        using (Library lib = new Library())
        {
            // Adobe PDF Library requires complex setup with HTML conversion parameters
            HTMLConversionParameters htmlParams = new HTMLConversionParameters();
            htmlParams.PaperSize = PaperSize.Letter;
            htmlParams.Orientation = Orientation.Portrait;
            
            string htmlContent = "<html><body><h1>Hello World</h1></body></html>";
            
            // Convert HTML to PDF
            Document doc = Document.CreateFromHTML(htmlContent, htmlParams);
            doc.Save(SaveFlags.Full, "output.pdf");
            doc.Dispose();
        }
    }
}