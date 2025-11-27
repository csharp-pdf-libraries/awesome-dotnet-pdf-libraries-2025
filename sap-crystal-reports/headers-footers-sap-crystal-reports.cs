// NuGet: Install-Package CrystalReports.Engine
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