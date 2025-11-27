// NuGet: Install-Package IronPdf
using IronPdf;
using System;
using System.Linq;

string pdfPath = "document.pdf";
string outputImage = "page1.png";

// Open PDF and convert to images
PdfDocument pdf = PdfDocument.FromFile(pdfPath);

// Convert first page to image
var firstPageImage = pdf.ToBitmap(0);
firstPageImage[0].Save(outputImage);
Console.WriteLine($"Page rendered to {outputImage}");

// Convert all pages to images
var allPageImages = pdf.ToBitmap();
for (int i = 0; i < allPageImages.Length; i++)
{
    allPageImages[i].Save($"page_{i + 1}.png");
    Console.WriteLine($"Saved page {i + 1}");
}

Console.WriteLine($"Total pages converted: {pdf.PageCount}");