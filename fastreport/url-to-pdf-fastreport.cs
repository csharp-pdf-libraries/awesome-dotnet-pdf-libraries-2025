// NuGet: Install-Package FastReport.OpenSource
//        Install-Package FastReport.OpenSource.Export.PdfSimple
// Note: FastReport has no native URL-to-PDF. You must download the HTML
// yourself and feed it into HtmlObject — and HtmlObject only handles a
// limited HTML 4 subset, no JavaScript and no modern CSS.
using FastReport;
using FastReport.Export.PdfSimple;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        // Download HTML content from URL
        string htmlContent;
        using (var client = new HttpClient())
        {
            htmlContent = await client.GetStringAsync("https://example.com");
        }

        using (Report report = new Report())
        {
            ReportPage page = new ReportPage();
            report.Pages.Add(page);
            page.ReportTitle = new ReportTitleBand { Height = Units.Millimeters * 250 };

            HtmlObject htmlObject = new HtmlObject();
            htmlObject.Bounds = new RectangleF(0, 0, Units.Millimeters * 190, Units.Millimeters * 250);
            htmlObject.Text = htmlContent;
            page.ReportTitle.Objects.Add(htmlObject);

            report.Prepare();

            PDFSimpleExport pdfExport = new PDFSimpleExport();
            using (FileStream fs = new FileStream("webpage.pdf", FileMode.Create))
            {
                report.Export(pdfExport, fs);
            }
        }
    }
}