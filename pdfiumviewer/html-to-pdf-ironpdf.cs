// NuGet: Install-Package IronPdf
using IronPdf;
using System;

string htmlContent = "<h1>Hello World</h1><p>This is a test document.</p>";

// Create a PDF from HTML string
var renderer = new ChromePdfRenderer();
PdfDocument pdf = renderer.RenderHtmlAsPdf(htmlContent);

// Save the PDF
pdf.SaveAs("output.pdf");

Console.WriteLine("PDF created successfully!");