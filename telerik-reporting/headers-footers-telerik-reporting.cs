// NuGet: Install-Package Telerik.Reporting
using Telerik.Reporting;
using Telerik.Reporting.Processing;
using Telerik.Reporting.Drawing;

class TelerikExample
{
    static void Main()
    {
        var report = new Telerik.Reporting.Report();
        
        // Add page header
        var pageHeader = new Telerik.Reporting.PageHeaderSection();
        pageHeader.Height = new Unit(0.5, UnitType.Inch);
        pageHeader.Items.Add(new Telerik.Reporting.TextBox()
        {
            Value = "Document Header",
            Location = new PointU(0, 0),
            Size = new SizeU(new Unit(6, UnitType.Inch), new Unit(0.3, UnitType.Inch))
        });
        report.PageHeaderSection = pageHeader;
        
        // Add page footer
        var pageFooter = new Telerik.Reporting.PageFooterSection();
        pageFooter.Height = new Unit(0.5, UnitType.Inch);
        pageFooter.Items.Add(new Telerik.Reporting.TextBox()
        {
            Value = "Page {PageNumber} of {PageCount}",
            Location = new PointU(0, 0),
            Size = new SizeU(new Unit(6, UnitType.Inch), new Unit(0.3, UnitType.Inch))
        });
        report.PageFooterSection = pageFooter;
        
        // Add content
        var htmlTextBox = new Telerik.Reporting.HtmlTextBox()
        {
            Value = "<h1>Report Content</h1><p>This is the main content.</p>"
        };
        report.Items.Add(htmlTextBox);
        
        var instanceReportSource = new Telerik.Reporting.InstanceReportSource();
        instanceReportSource.ReportDocument = report;
        
        var reportProcessor = new ReportProcessor();
        var result = reportProcessor.RenderReport("PDF", instanceReportSource, null);
        
        using (var fs = new System.IO.FileStream("report_with_headers.pdf", System.IO.FileMode.Create))
        {
            fs.Write(result.DocumentBytes, 0, result.DocumentBytes.Length);
        }
    }
}