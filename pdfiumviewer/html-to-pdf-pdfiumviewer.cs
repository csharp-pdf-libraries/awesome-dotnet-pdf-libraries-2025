// NuGet: Install-Package PdfiumViewer
using PdfiumViewer;
using System.IO;
using System.Drawing.Printing;

// PDFiumViewer is primarily a PDF viewer/renderer, not a generator
// It cannot directly convert HTML to PDF
// You would need to use another library to first create the PDF
// Then use PDFiumViewer to display it:

string htmlContent = "<h1>Hello World</h1><p>This is a test document.</p>";

// This functionality is NOT available in PDFiumViewer
// You would need a different library like wkhtmltopdf or similar
// PDFiumViewer can only open and display existing PDFs:

string existingPdfPath = "output.pdf";
using (var document = PdfDocument.Load(existingPdfPath))
{
    // Can only render/display existing PDF
    var image = document.Render(0, 300, 300, true);
}