// NuGet: Install-Package IronPdf
using IronPdf;

string html = "<html><body><h1>Hello World</h1><p>This is a PDF document.</p></body></html>";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("output.pdf");