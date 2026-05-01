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
        // Crystal Reports requires design-time configuration
        ReportDocument reportDocument = new ReportDocument();
        reportDocument.Load("Report.rpt");
        
        // Headers and footers must be designed in the .rpt file
        // using Crystal Reports designer
        // You can set parameter values programmatically
        reportDocument.SetParameterValue("HeaderText", "Company Name");
        reportDocument.SetParameterValue("FooterText", "Page ");
        
        // Crystal Reports handles page numbers through formula fields
        // configured in the designer
        
        reportDocument.ExportToDisk(ExportFormatType.PortableDocFormat, "output.pdf");
        reportDocument.Close();
        reportDocument.Dispose();
    }
}