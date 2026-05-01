// NuGet: Install-Package PdfiumViewer
//        Install-Package PdfiumViewer.Native.x86.v8-xfa
//        Install-Package PdfiumViewer.Native.x86_64.v8-xfa
// Repo:  https://github.com/pvginkel/PdfiumViewer (archived 2019-08-02, last release 2.13.0 from 2017-11-06)
using PdfiumViewer;
using System;
using System.Text;

string pdfPath = "document.pdf";

// PdfiumViewer wraps Google's PDFium and exposes basic page-level text
// extraction via PdfDocument.GetPdfText(int page). It returns raw page
// text only — no layout, no per-word coordinates, no OCR for image-only
// pages. The library is WinForms-only and the upstream repo is archived.
using (var document = PdfDocument.Load(pdfPath))
{
    int pageCount = document.PageCount;
    Console.WriteLine($"Total pages: {pageCount}");

    var sb = new StringBuilder();
    for (int i = 0; i < pageCount; i++)
    {
        // GetPdfText(int page) is the only built-in text API.
        string pageText = document.GetPdfText(i);
        sb.AppendLine($"--- Page {i + 1} ---");
        sb.AppendLine(pageText);
    }

    Console.WriteLine(sb.ToString());
}
