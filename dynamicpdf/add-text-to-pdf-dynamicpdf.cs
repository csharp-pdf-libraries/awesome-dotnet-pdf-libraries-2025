// NuGet: Install-Package ceTe.DynamicPDF.CoreSuite.NET
using ceTe.DynamicPDF;
using ceTe.DynamicPDF.PageElements;

class Program
{
    static void Main()
    {
        Document document = new Document();
        Page page = new Page(PageSize.Letter);
        Label label = new Label("Hello from DynamicPDF!", 0, 0, 504, 100);
        page.Elements.Add(label);
        document.Pages.Add(page);
        document.Draw("output.pdf");
    }
}