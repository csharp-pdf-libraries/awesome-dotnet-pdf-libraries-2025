// NuGet: Install-Package FastReport.OpenSource
using FastReport;
using FastReport.Export.PdfSimple;
using System.IO;

class Program
{
    static void Main()
    {
        using (Report report = new Report())
        {
            // Create HTML object
            FastReport.HTMLObject htmlObject = new FastReport.HTMLObject();
            htmlObject.Width = 500;
            htmlObject.Height = 300;
            htmlObject.Text = "<html><body><h1>Hello World</h1><p>This is a test PDF</p></body></html>";
            
            // Prepare report
            report.Prepare();
            
            // Export to PDF
            PDFSimpleExport pdfExport = new PDFSimpleExport();
            using (FileStream fs = new FileStream("output.pdf", FileMode.Create))
            {
                report.Export(pdfExport, fs);
            }
        }
    }
}