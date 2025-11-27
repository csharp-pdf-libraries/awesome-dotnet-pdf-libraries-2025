// NuGet: Install-Package Telerik.Documents.Flow
// NuGet: Install-Package Telerik.Documents.Flow.FormatProviders.Pdf
using Telerik.Windows.Documents.Flow.FormatProviders.Html;
using Telerik.Windows.Documents.Flow.FormatProviders.Pdf;
using Telerik.Windows.Documents.Flow.Model;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

string url = "https://example.com";

using HttpClient client = new HttpClient();
string html = await client.GetStringAsync(url);

HtmlFormatProvider htmlProvider = new HtmlFormatProvider();
RadFlowDocument document = htmlProvider.Import(html);

PdfFormatProvider pdfProvider = new PdfFormatProvider();
using (FileStream output = File.OpenWrite("webpage.pdf"))
{
    pdfProvider.Export(document, output);
}