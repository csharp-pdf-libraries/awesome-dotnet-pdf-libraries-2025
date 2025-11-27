// NuGet: Install-Package Telerik.Documents.Fixed
using Telerik.Windows.Documents.Fixed.FormatProviders.Pdf;
using Telerik.Windows.Documents.Fixed.Model;
using System.IO;

PdfFormatProvider provider = new PdfFormatProvider();

RadFixedDocument document1;
using (FileStream input = File.OpenRead("document1.pdf"))
{
    document1 = provider.Import(input);
}

RadFixedDocument document2;
using (FileStream input = File.OpenRead("document2.pdf"))
{
    document2 = provider.Import(input);
}

RadFixedDocument mergedDocument = new RadFixedDocument();
foreach (var page in document1.Pages)
{
    mergedDocument.Pages.Add(page);
}
foreach (var page in document2.Pages)
{
    mergedDocument.Pages.Add(page);
}

using (FileStream output = File.OpenWrite("merged.pdf"))
{
    provider.Export(mergedDocument, output);
}