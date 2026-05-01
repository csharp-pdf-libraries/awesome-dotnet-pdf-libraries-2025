// SSRS - SQL Server Reporting Services (LocalReport, in-process RDLC rendering)
// NuGet: Install-Package Microsoft.ReportingServices.ReportViewerControl.WebForms (v150.1652.0, .NET Framework only)
// For .NET Core/.NET 5+ no official Microsoft package exists; community port is ReportViewerCore.NETCore.
// Note: SSRS itself does NOT render arbitrary HTML pages to PDF — it renders RDLC report definitions.
// The example below downloads HTML and stuffs it into an RDLC parameter, which is a workaround,
// not a native SSRS feature.
using System;
using System.IO;
using System.Net;
using Microsoft.Reporting.WebForms;

class SSRSUrlToPdf
{
    static void Main()
    {
        // Download HTML content from URL
        string url = "https://example.com";
        string htmlContent;
        
        using (var client = new WebClient())
        {
            htmlContent = client.DownloadString(url);
        }
        
        // Create RDLC report with header/footer configuration
        var reportViewer = new ReportViewer();
        reportViewer.ProcessingMode = ProcessingMode.Local;
        reportViewer.LocalReport.ReportPath = "WebReport.rdlc";
        
        // Set parameters for header and footer
        var parameters = new ReportParameter[]
        {
            new ReportParameter("HeaderText", "Company Report"),
            new ReportParameter("FooterText", "Page " + DateTime.Now.ToString()),
            new ReportParameter("HtmlContent", htmlContent)
        };
        reportViewer.LocalReport.SetParameters(parameters);
        
        // Render to PDF
        string mimeType, encoding, fileNameExtension;
        string[] streams;
        Warning[] warnings;
        
        byte[] bytes = reportViewer.LocalReport.Render(
            "PDF", null, out mimeType, out encoding,
            out fileNameExtension, out streams, out warnings);
        
        File.WriteAllBytes("webpage.pdf", bytes);
    }
}