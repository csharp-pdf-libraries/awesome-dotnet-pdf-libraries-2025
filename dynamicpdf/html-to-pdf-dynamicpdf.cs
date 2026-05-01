// NuGet: Install-Package ceTe.DynamicPDF.Converter.NET
// HTML-to-PDF is a separate add-on; CoreSuite alone does not include it.
using ceTe.DynamicPDF;
using ceTe.DynamicPDF.Conversion;

class Program
{
    static void Main()
    {
        string html = "<html><body><h1>Hello World</h1></body></html>";
        HtmlConverter converter = new HtmlConverter(html);
        converter.Convert("output.pdf");
    }
}