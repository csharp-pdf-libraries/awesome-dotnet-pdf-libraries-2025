// NuGet: Install-Package Telerik.Documents.Flow
// NuGet: Install-Package Telerik.Documents.Flow.FormatProviders.Pdf
using Telerik.Windows.Documents.Flow.FormatProviders.Html;
using Telerik.Windows.Documents.Flow.FormatProviders.Pdf;
using Telerik.Windows.Documents.Flow.Model;
using System.IO;

string html = "<html><body><h1>Hello World</h1><p>This is a PDF document.</p></body></html>";

HtmlFormatProvider htmlProvider = new HtmlFormatProvider();
RadFlowDocument document = htmlProvider.Import(html);

PdfFormatProvider pdfProvider = new PdfFormatProvider();
using (FileStream output = File.OpenWrite("output.pdf"))
{
    pdfProvider.Export(document, output);
}