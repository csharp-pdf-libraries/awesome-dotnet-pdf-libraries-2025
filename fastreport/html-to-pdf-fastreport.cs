// NuGet: Install-Package FastReport.OpenSource
//        Install-Package FastReport.OpenSource.Export.PdfSimple
using FastReport;
using FastReport.Export.PdfSimple;
using System.Drawing;
using System.IO;

class Program
{
    static void Main()
    {
        using (Report report = new Report())
        {
            // Add a page and a report-title band — FastReport renders nothing
            // unless objects live on a band that lives on a ReportPage.
            ReportPage page = new ReportPage();
            report.Pages.Add(page);
            page.ReportTitle = new ReportTitleBand { Height = Units.Millimeters * 100 };

            // Create HTML object (note: FastReport class is HtmlObject, not HTMLObject)
            HtmlObject htmlObject = new HtmlObject();
            htmlObject.Bounds = new RectangleF(0, 0, Units.Millimeters * 190, Units.Millimeters * 100);
            htmlObject.Text = "<html><body><h1>Hello World</h1><p>This is a test PDF</p></body></html>";
            page.ReportTitle.Objects.Add(htmlObject);

            // Prepare report
            report.Prepare();

            // Export to PDF (PDFSimpleExport rasterizes each page — text is not selectable)
            PDFSimpleExport pdfExport = new PDFSimpleExport();
            using (FileStream fs = new FileStream("output.pdf", FileMode.Create))
            {
                report.Export(pdfExport, fs);
            }
        }
    }
}