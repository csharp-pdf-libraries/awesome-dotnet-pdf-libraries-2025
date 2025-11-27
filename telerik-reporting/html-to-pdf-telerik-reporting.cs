// NuGet: Install-Package Telerik.Reporting
using Telerik.Reporting;
using Telerik.Reporting.Processing;
using System.Collections.Specialized;

class TelerikExample
{
    static void Main()
    {
        var reportSource = new Telerik.Reporting.TypeReportSource();
        var instanceReportSource = new Telerik.Reporting.InstanceReportSource();
        instanceReportSource.ReportDocument = new Telerik.Reporting.Report()
        {
            Items = { new Telerik.Reporting.HtmlTextBox() { Value = "<h1>Hello World</h1><p>Sample HTML content</p>" } }
        };
        
        var reportProcessor = new ReportProcessor();
        var result = reportProcessor.RenderReport("PDF", instanceReportSource, null);
        
        using (var fs = new System.IO.FileStream("output.pdf", System.IO.FileMode.Create))
        {
            fs.Write(result.DocumentBytes, 0, result.DocumentBytes.Length);
        }
    }
}