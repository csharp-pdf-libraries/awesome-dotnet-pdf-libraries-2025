// NuGet: Install-Package Telerik.Reporting
using Telerik.Reporting;
using Telerik.Reporting.Processing;
using System.Net;

class TelerikExample
{
    static void Main()
    {
        string htmlContent;
        using (var client = new WebClient())
        {
            htmlContent = client.DownloadString("https://example.com");
        }
        
        var report = new Telerik.Reporting.Report();
        var htmlTextBox = new Telerik.Reporting.HtmlTextBox()
        {
            Value = htmlContent
        };
        report.Items.Add(htmlTextBox);
        
        var instanceReportSource = new Telerik.Reporting.InstanceReportSource();
        instanceReportSource.ReportDocument = report;
        
        var reportProcessor = new ReportProcessor();
        var result = reportProcessor.RenderReport("PDF", instanceReportSource, null);
        
        using (var fs = new System.IO.FileStream("webpage.pdf", System.IO.FileMode.Create))
        {
            fs.Write(result.DocumentBytes, 0, result.DocumentBytes.Length);
        }
    }
}