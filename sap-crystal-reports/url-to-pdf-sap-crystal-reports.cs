// SAP does not publish an official Crystal Reports NuGet package. Install the
// SAP Crystal Reports for Visual Studio (CR4VS) runtime MSI from help.sap.com
// and reference the GAC-installed CrystalDecisions.*.dll assemblies. Community
// NuGet wrappers (e.g. CrystalReports.Engine, publisher: ennerperez) exist
// for build automation but are not endorsed by SAP. Windows + .NET Framework 4.x only.
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