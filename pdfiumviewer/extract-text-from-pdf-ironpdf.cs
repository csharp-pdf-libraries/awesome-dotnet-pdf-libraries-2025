// NuGet: Install-Package IronPdf
using IronPdf;
using System;

string pdfPath = "document.pdf";

// Open and extract text from PDF
PdfDocument pdf = PdfDocument.FromFile(pdfPath);

// Extract text from all pages
string allText = pdf.ExtractAllText();
Console.WriteLine("Extracted Text:");
Console.WriteLine(allText);

// Extract text from specific page
string pageText = pdf.ExtractTextFromPage(0);
Console.WriteLine($"\nFirst page text: {pageText}");

Console.WriteLine($"\nTotal pages: {pdf.PageCount}");