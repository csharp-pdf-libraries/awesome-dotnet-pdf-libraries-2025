// NuGet: Install-Package IronPdf
using IronPdf;

string url = "https://example.com";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderUrlAsPdf(url);
pdf.SaveAs("webpage.pdf");