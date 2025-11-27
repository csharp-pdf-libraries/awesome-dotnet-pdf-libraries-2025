// NuGet: Install-Package FastReport.OpenSource
using FastReport;
using FastReport.Export.PdfSimple;
using System.IO;
using System.Net;

class Program
{
    static void Main()
    {
        // Download HTML content from URL
        string htmlContent;
        using (WebClient client = new WebClient())
        {
            htmlContent = client.DownloadString("https://example.com");
        }
        
        using (Report report = new Report())
        {
            FastReport.HTMLObject htmlObject = new FastReport.HTMLObject();
            htmlObject.Width = 800;
            htmlObject.Height = 600;
            htmlObject.Text = htmlContent;
            
            report.Prepare();
            
            PDFSimpleExport pdfExport = new PDFSimpleExport();
            using (FileStream fs = new FileStream("webpage.pdf", FileMode.Create))
            {
                report.Export(pdfExport, fs);
            }
        }
    }
}