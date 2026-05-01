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
        // Default URL is http://localhost:9423/service/rest
        PDFreactor pdfReactor = new PDFreactor();

        string html = "<html><body><h1>Hello World</h1></body></html>";

        Configuration config = new Configuration();
        config.Document = html;

        Result result = pdfReactor.Convert(config);

        File.WriteAllBytes("output.pdf", result.Document);
    }
}