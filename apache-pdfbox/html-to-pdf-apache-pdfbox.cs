// Apache PDFBox does not have official .NET port
// Community ports like PDFBox-dotnet are incomplete
// and do not support HTML to PDF conversion natively.
// You would need to use additional libraries like
// iText or combine with HTML renderers separately.

using PdfBoxDotNet.Pdmodel;
using System.IO;

// Note: This is NOT supported in PDFBox
// PDFBox is primarily for PDF manipulation, not HTML rendering
// You would need external HTML rendering engine