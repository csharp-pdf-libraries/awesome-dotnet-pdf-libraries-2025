// NuGet: Install-Package CrystalReports.Engine
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using System;
using System.Net;

class Program
{
    static void Main()
    {
        // Crystal Reports cannot directly convert URLs to PDF
        // You need to create a report template first
        
        // Download HTML content
        WebClient client = new WebClient();
        string htmlContent = client.DownloadString("https://example.com");
        
        // Crystal Reports requires .rpt template and data binding
        // This approach is not straightforward for URL conversion
        ReportDocument reportDocument = new ReportDocument();
        reportDocument.Load("WebReport.rpt");
        
        // Manual data extraction and binding required
        // reportDocument.SetDataSource(extractedData);
        
        reportDocument.ExportToDisk(ExportFormatType.PortableDocFormat, "output.pdf");
        reportDocument.Close();
        reportDocument.Dispose();
    }
}