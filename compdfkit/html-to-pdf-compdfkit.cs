// NuGet: Install-Package ComPDFKit.NetCore
// ComPDFKit has no native HTML-to-PDF rendering. The closest equivalent is to
// create a blank document, insert a page, and place text via the page editor.
// See https://www.compdf.com/blog/edit-pdfs-programmatically-using-c-sharp
using ComPDFKit.PDFDocument;
using System;

class Program
{
    static void Main()
    {
        // License must be verified before any document operation
        // CPDFSDKVerifier.LicenseVerify(devKey, devSecret, userKey, userSecret);

        var document = CPDFDocument.CreateDocument();

        // Insert a blank A4 page (595 x 842 points) at index 0
        document.InsertPage(0, 595, 842, string.Empty);

        // ComPDFKit does NOT render HTML/CSS. To approximate the output you
        // would need to lay out text + images manually via the page editor,
        // or use a separate HTML rendering library to rasterize the page.
        //
        // For real HTML-to-PDF, ComPDFKit recommends their cloud Conversion
        // API (separate product) — see https://api.compdf.com/api-libraries

        document.WriteToFilePath("output.pdf");
        document.Release();
    }
}
