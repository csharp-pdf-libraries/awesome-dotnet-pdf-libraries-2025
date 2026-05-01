// PdfPig does NOT support HTML to PDF conversion.
// PdfPig (NuGet: PdfPig, namespace UglyToad.PdfPig) is primarily a PDF
// reading/parsing library. It exposes a limited writer surface
// (UglyToad.PdfPig.Writer.PdfDocumentBuilder) for coordinate-based
// document creation and UglyToad.PdfPig.Writer.PdfMerger for merging,
// but it has no HTML/CSS rendering engine. The PdfPig wiki explicitly
// lists "Converting HTML or other formats to PDF" under "Things you
// can't do" -- see https://github.com/UglyToad/PdfPig/wiki
// Use IronPDF, wkhtmltopdf, or another HTML rendering library instead.