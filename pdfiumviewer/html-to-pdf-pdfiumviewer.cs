// NuGet: Install-Package PdfiumViewer
// Repo:  https://github.com/pvginkel/PdfiumViewer (archived 2019-08-02)
//
// PdfiumViewer is a viewer/renderer wrapper around Google's PDFium —
// it has NO HTML-to-PDF capability. PDFium itself does not parse HTML.
// To produce a PDF from HTML you need a separate generator (wkhtmltopdf,
// IronPDF, PuppeteerSharp, etc.) and can then load the result with
// PdfiumViewer for display or rasterization.
using PdfiumViewer;
using System;

string htmlContent = "<h1>Hello World</h1><p>This is a test document.</p>";

// Step 1: PdfiumViewer cannot do this — generate the PDF elsewhere.
// (Pretend we produced "output.pdf" via another tool.)
string existingPdfPath = "output.pdf";

// Step 2: PdfiumViewer can load and render the resulting PDF.
using (var document = PdfDocument.Load(existingPdfPath))
{
    Console.WriteLine($"Loaded PDF with {document.PageCount} page(s).");

    // Render the first page to an image (true = forPrinting flags).
    var image = document.Render(0, 300, 300, true);
    Console.WriteLine("Rendered page 1 at 300 DPI.");
}
