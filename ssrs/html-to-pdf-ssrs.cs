// SSRS - SQL Server Reporting Services
using System;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Reporting.WebForms;
using System.IO;

class SSRSHtmlToPdf
{
    static void Main()
    {
        // Create a ReportViewer instance
        var reportViewer = new ReportViewer();
        reportViewer.ProcessingMode = ProcessingMode.Local;
        
        // Load RDLC report definition
        reportViewer.LocalReport.ReportPath = "Report.rdlc";
        
        // Add HTML content as a parameter or dataset
        var htmlContent = "<h1>Hello World</h1><p>This is HTML content.</p>";
        var param = new ReportParameter("HtmlContent", htmlContent);
        reportViewer.LocalReport.SetParameters(param);
        
        // Render the report to PDF
        string mimeType, encoding, fileNameExtension;
        string[] streams;
        Warning[] warnings;
        
        byte[] bytes = reportViewer.LocalReport.Render(
            "PDF",
            null,
            out mimeType,
            out encoding,
            out fileNameExtension,
            out streams,
            out warnings);
        
        File.WriteAllBytes("output.pdf", bytes);
    }
}