// NuGet: Install-Package IronPdf
using IronPdf;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

string url = "https://example.com";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderUrlAsPdf(url);
pdf.SaveAs("webpage.pdf");