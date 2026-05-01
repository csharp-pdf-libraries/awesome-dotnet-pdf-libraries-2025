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
            report.Load("template.frx");

            // Cast to ReportPage (Report.Pages can hold dialog pages too)
            ReportPage page = report.Pages[0] as ReportPage;

            // Page header — assigned to the band slot on the page, not added via Bands.Add
            page.PageHeader = new PageHeaderBand { Height = Units.Millimeters * 15 };
            TextObject headerText = new TextObject
            {
                Bounds = new RectangleF(0, 0, Units.Millimeters * 190, Units.Millimeters * 10),
                Text = "Document Header",
                HorzAlign = HorzAlign.Center
            };
            page.PageHeader.Objects.Add(headerText);

            // Page footer with FastReport page tokens [Page] and [TotalPages]
            page.PageFooter = new PageFooterBand { Height = Units.Millimeters * 15 };
            TextObject footerText = new TextObject
            {
                Bounds = new RectangleF(0, 0, Units.Millimeters * 190, Units.Millimeters * 10),
                Text = "Page [Page] of [TotalPages]",
                HorzAlign = HorzAlign.Right
            };
            page.PageFooter.Objects.Add(footerText);

            report.Prepare();

            PDFSimpleExport pdfExport = new PDFSimpleExport();
            using (FileStream fs = new FileStream("report.pdf", FileMode.Create))
            {
                report.Export(pdfExport, fs);
            }
        }
    }
}