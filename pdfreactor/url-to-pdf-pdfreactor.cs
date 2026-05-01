// PDFreactor .NET wrapper is a Web Service client (no NuGet package).
// Reference PDFreactor.dll from <PDFreactor-install>/clients/netstandard2/bin/
// and run the PDFreactor Web Service (Java/Jetty) locally or remotely.
// Docs: https://www.pdfreactor.com/product/doc_html/manual-ws.html
using RealObjects.PDFreactor.Webservice.Client;
using System.IO;

class Program
{
    static void Main()
    {
        PDFreactor pdfReactor = new PDFreactor();

        Configuration config = new Configuration();
        config.Document = "https://www.example.com";

        Result result = pdfReactor.Convert(config);

        File.WriteAllBytes("webpage.pdf", result.Document);
    }
}