// SAP does not publish an official Crystal Reports NuGet package. Install the
// SAP Crystal Reports for Visual Studio (CR4VS) runtime MSI from help.sap.com
// and reference the GAC-installed CrystalDecisions.*.dll assemblies. Community
// NuGet wrappers (e.g. CrystalReports.Engine, publisher: ennerperez) exist
// for build automation but are not endorsed by SAP. Windows + .NET Framework 4.x only.
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using System;

class Program
{
    static void Main()
    {
        // Crystal Reports requires a .rpt file template
        ReportDocument reportDocument = new ReportDocument();
        reportDocument.Load("Report.rpt");
        
        // Crystal Reports doesn't directly support HTML
        // You need to bind data to the report template
        // reportDocument.SetDataSource(dataSet);
        
        ExportOptions exportOptions = reportDocument.ExportOptions;
        exportOptions.ExportDestinationType = ExportDestinationType.DiskFile;
        exportOptions.ExportFormatType = ExportFormatType.PortableDocFormat;
        
        DiskFileDestinationOptions diskOptions = new DiskFileDestinationOptions();
        diskOptions.DiskFileName = "output.pdf";
        exportOptions.DestinationOptions = diskOptions;
        
        reportDocument.Export();
        reportDocument.Close();
        reportDocument.Dispose();
    }
}